﻿/**
 *      Author: Casegard
 *      Program:  CGI - Gyro Bsics
 *
 *      Version:
 *              v0.10 - just the basic functions for a gyro block - also maybe a gyro test routine to check the angle acceleration
 *              v0.20 - lets see if we can make out way to align with the gravity vector
 *              v0.30 - make the script capable of taking a vector for a scanned position and allign to this position
 *
 */

CGI_GyroManager myGyroManager = new CGI_GyroManager();

IMyShipController myCockpit = null;
List<IMyTextPanel> myLCDPanels = new List<IMyTextPanel>();


int mTick = 0;

public Program()
{
    Runtime.UpdateFrequency = UpdateFrequency.Update10;
    List<IMyShipController> aList = new List<IMyShipController>();
    GridTerminalSystem.GetBlocksOfType(aList, b => b.CubeGrid == Me.CubeGrid);
    myCockpit = aList[0];

    GridTerminalSystem.GetBlocksOfType(myLCDPanels, b => b.CubeGrid == Me.CubeGrid);



}

public void Save() {}



public string mArgument = "";

public void Main(string argument, UpdateType updateSource)
{
    string aOut = String.Format("Tick: {0}\n",mTick++);


    myGyroManager.LoadEntities(GridTerminalSystem,myCockpit);


    if (argument.Equals("GyroCheck"))
    {
        mArgument = argument;
    }

    aOut += myGyroManager.Statistics();

    if (mArgument.Equals("GyroCheck"))
    {
        aOut += myGyroManager.ProcessGyroCheck();
    }

    aOut += myGyroManager.ProcessGravityAlignment();

    if (myLCDPanels.Count > 0)
    {
        myLCDPanels[6].WritePublicText(aOut,false);
    }
    else
    {
        Echo(aOut);
    }
}


public class CGI_GyroManager
{
    private List<IMyGyro> mGyros = new List<IMyGyro>();
    private IMyShipController mReference = null;

    private bool isStart = true;
    private Vector3D aCheckStartPosition = Vector3D.Zero;
    private float POWER_CHECK_DELTA = 0.001f;
    private float GYRO_DELTA =  (float) (1 * Math.PI / 180f);

    public string LoadEntities(IMyGridTerminalSystem pGTS, IMyShipController pReference)
    {
        string aOut = "";
        pGTS.GetBlocksOfType(mGyros);
        mReference = pReference;
        return aOut;
    }

    public string ProcessGravityAlignment()
    {
        string aResult = "";
        Vector3D aWorldGravity = -mReference.GetNaturalGravity();

        Vector3D aUp = mReference.WorldMatrix.Up;
        Vector3D aLeft = mReference.WorldMatrix.Left;
        Vector3D aForward = mReference.WorldMatrix.Forward;

        Vector3D aLocalGravity = Vector3D.Zero;

        aLocalGravity.X = aWorldGravity.Dot(aUp);
        aLocalGravity.Y = aWorldGravity.Dot(aLeft);
        aLocalGravity.Z = aWorldGravity.Dot(aForward);

        Vector3D aRadianAngleVector = Vector3D.Zero;
        aRadianAngleVector.X = Math.Acos(aLocalGravity.X / (aWorldGravity.Length() * aUp.Length())); // YAW (not needed)
        aRadianAngleVector.Y = Math.Acos(aLocalGravity.Y / (aWorldGravity.Length() * aLeft.Length())) - Math.PI/2f; // ROLL
        aRadianAngleVector.Z = Math.Acos(aLocalGravity.Z / (aWorldGravity.Length() * aForward.Length())) - Math.PI/2f; // PITCH

        Vector3D aDegreeAngleVector = (aRadianAngleVector) * 180.00f / Math.PI;

        aResult += String.Format("Angle: Gravity -> Ship\n Pitch: {0:00.000}\n Roll: {1:00.000}\n Yaw: {2:00.000}\n",
                        aDegreeAngleVector.Z,
                        aDegreeAngleVector.Y,
                        aDegreeAngleVector.X);

       foreach (IMyGyro aGyro in mGyros)
       {
             if (aDegreeAngleVector.Z != 0)
             {
                    aGyro.Pitch = (float) -aRadianAngleVector.Z;
             }
             if (aDegreeAngleVector.Y != 0)
             {
                    aGyro.Roll = (float) aRadianAngleVector.Y;
             }
       }



      aResult += String.Format("Gyro: \n Pitch: {0:00.000}\n Roll: {1:00.000}\n Yaw: {2:00.000}\n",
                        mGyros[0].Pitch * 180 / Math.PI,
                        mGyros[0].Roll * 180 / Math.PI,
                        mGyros[0].Yaw * 180 / Math.PI);


        return aResult;
    }



    public string ProcessGyroCheck()
    {
        string aOut = "";
        if (isStart)
        {
            aCheckStartPosition = mGyros[0].WorldMatrix.Forward + Vector3D.Forward;
            foreach ( IMyGyro aGyro in mGyros)
            {
                    aGyro.GyroPower = 0;
                    aGyro.GyroOverride = true;
                    aGyro.Pitch = 0;
                    aGyro.Yaw = 0;
                    aGyro.Roll = 0;
            }
            isStart = false;
            aOut = aOut + "Start\n\n";
        }
        else
        {
            Vector3D aCheckVector = mGyros[0].WorldMatrix.Forward + Vector3D.Forward;

            if (aCheckVector.Equals(aCheckStartPosition))
            {
                 aOut = aOut + "Run\n";

                 foreach ( IMyGyro aGyro in mGyros)
                 {
                      aGyro.GyroPower += POWER_CHECK_DELTA ;
                      aGyro.Yaw = 6.283f; // pi * 2
                      aOut = aOut + String.Format("{0} - power: {1:000.000}\n",aGyro.CustomName,aGyro.GyroPower);

                 }
            }
            else
            {
                 foreach ( IMyGyro aGyro in mGyros)
                 {
                      aGyro.GyroOverride = false;
                      aOut = aOut + String.Format("Complete {0} - power: {1:000.000}\n",aGyro.CustomName,aGyro.GyroPower);
                 }
            }
        }
        return aOut;
    }

    public string Statistics()
    {
        string aOut = "";
        aOut = aOut + String.Format("Gyros: {0}\n\n",mGyros.Count);

        Vector3D aForwardReference = mReference.WorldMatrix.Forward;
        Vector3D aLeftReference = mReference.WorldMatrix.Left;
        Vector3D aUpReference = mReference.WorldMatrix.Up;

        foreach( IMyGyro aGyro in mGyros)
        {
                string aAlign = "not aligned";
                Vector3D aGyroForward = aGyro.WorldMatrix.Forward;
                Vector3D aGyroLeft = aGyro.WorldMatrix.Left;
                Vector3D aGyroUp = aGyro.WorldMatrix.Up;

                if (aForwardReference.Equals(aGyroForward) && aLeftReference.Equals(aGyroLeft) && aUpReference.Equals(aGyroUp))
                {
                        aAlign = "aligned";
                }

                aOut = aOut + String.Format("{0} - {1}\n",aGyro.CustomName,aAlign);
        }

        return aOut;
    }
}
