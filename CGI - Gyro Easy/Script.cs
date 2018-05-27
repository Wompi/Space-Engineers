/**
 *      Author: Casegard
 *      Program:  CGI - Gyro Easy
 *
 *      Version:
 *              v0.10 - implement the very basic steps to allign a ship with gyros to the gravity
 *
 */


List<IMyGyro> mGyros = new List<IMyGyro>();
List<IMyCockpit> mCockpits = new List<IMyCockpit>();


﻿public Program()
{
    Runtime.UpdateFrequency = UpdateFrequency.Update10;

    Func<IMyTerminalBlock, bool> aCheck = b => b.CubeGrid == Me.CubeGrid;

    GridTerminalSystem.GetBlocksOfType(mGyros, aCheck);
    GridTerminalSystem.GetBlocksOfType(myCockpits, aCheck);
}

public void Save()
{
}

public void Main(string argument, UpdateType updateSource)
{
    if (mCockpits.Count == 0 )
    {
        Echo("Derp we need a cockpit - dummy!\n");
    }
    if (mGyros.Count == 0)
    {
        Echo("Derp we need some gyros - dummy!\n");
    }
    IMyCockpit aCockpit = mCockpits[0];

    string aOut = AlignToGravity(-aCockpit.GetNaturalGravity(),aCockpit);

    Echo(aOut);
}


// TODO: use this if or better when you have understand it
//Whip's ApplyGyroOverride Method v9 - 8/19/17
//void ApplyGyroOverride(Vector3D pTargetVector, IMyTerminal pReference)
//{

	//var rotationVec = new Vector3D(-pitch_speed, yaw_speed, roll_speed); //because keen does some weird stuff with signs
//	Matrix aReferenceMatrix = pReference.WorldMatrix;
//	Vector3D aRelativeRotationVec = Vector3D.TransformNormal(pTargetVector, aReferenceMatrix);
//	foreach (IMyGyro aGyro in mGyros)
//	{
//		Matrix aGyroMatrix = aGyro.WorldMatrix;
//		var aTransformedRotationVec = Vector3D.TransformNormal(aRelativeRotationVec, Matrix.Transpose(aGyroMatrix));
//		aGyro.Pitch = (float)aTransformedRotationVec.X;
//		aGyro.Yaw = (float)aTransformedRotationVec.Y;
//		aGyro.Roll = (float)aTransformedRotationVec.Z;
//	}
//}



public string AlignToGravity(Vector3D pTargetVector, IMyTerminal pReference)
{
    string aResult = "";

    Vector3D aUp = pReference.WorldMatrix.Up;
    Vector3D aLeft = pReference.WorldMatrix.Left;
    Vector3D aForward = pReference.WorldMatrix.Forward;

    Vector3D aLocalGravity = Vector3D.Zero;
    aLocalGravity.X = pTargetVector.Dot(aUp);
    aLocalGravity.Y = pTargetVector.Dot(aLeft);
    aLocalGravity.Z = pTargetVector.Dot(aForward);

    double aLen = pTargetVector.Length();

    // NOTE: the one is just to point out that the aUp.Length() -> X, the aLeft.Length() -> Y and
    // the aForward.Length() -> Z should be multiplied (these are normalized and therefor 1)
    Vector3D aBuffy = aLocalGravity / (aLen * 1);

    Vector3D aAngleVector = Vector3D.Zero;
    aAngleVector.X = Math.Acos(aBuffy.X); // YAW (not needed)
    aAngleVector.Y = Math.Acos(aBuffy.Y) - Math.PI/2f; // ROLL
    aAngleVector.Z = Math.Acos(aBuffy.Z) - Math.PI/2f; // PITCH

    // DEBUG: show me the angles
    //Vector3D aDegreeAngleVector = (aAngleVector) * 180.00f / Math.PI;
    //aResult += String.Format("Angle: Gravity -> Ship\n Pitch: {0:00.000}\n Roll: {1:00.000}\n Yaw: {2:00.000}\n",
    //                aDegreeAngleVector.Z,
    //                aDegreeAngleVector.Y,
    //                aDegreeAngleVector.X);

   foreach (IMyGyro aGyro in mGyros)
   {
       // TODO: put this into an external layer - or make it ontime only
       Vector3D aGyroForward = aGyro.WorldMatrix.Forward;
       Vector3D aGyroLeft = aGyro.WorldMatrix.Left;
       Vector3D aGyroUp = aGyro.WorldMatrix.Up;
       if (aForward.Equals(aGyroForward) && aLeft.Equals(aGyroLeft) && aUp.Equals(aGyroUp))
       {
       }
       else
       {
           aResult += String.Format("Gyro not aligned ({0})\n",aGyro.CustomName);
       }
       // end TODO

       if (aAngleVector.Z != 0)
       {
           aGyro.Pitch = (float) -aAngleVector.Z;
       }
       if (aAngleVector.Y != 0)
       {
           aGyro.Roll = (float) aAngleVector.Y;
       }
   }

   // NOTE: bring this in another layer
   aResult += String.Format("Gyro: \n Pitch: {0:00.000}\n Roll: {1:00.000}\n Yaw: {2:00.000}\n",
                     mGyros[0].Pitch * 180 / Math.PI,
                     mGyros[0].Roll * 180 / Math.PI,
                     mGyros[0].Yaw * 180 / Math.PI);



   return aResult;
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
