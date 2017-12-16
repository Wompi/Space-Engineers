/**
 *      Author: Casegard
 *      Program:  CGI - Gyro Bsics
 *
 *      Version:
 *              v0.10 - just the basic functions for a gyro block - also maybe a gyro test routine to check the angle acceleration 
 *
 */

CGI_GyroManager myGyroManager = new CGI_GyroManager();

IMyCockpit myCockpit = null;

public Program() 
{
    Runtime.UpdateFrequency = UpdateFrequency.Update10;
    List<IMyCockpit> aList = new List<IMyCockpit>();  
    GridTerminalSystem.GetBlocksOfType(aList);  
    myCockpit = aList[0]; 


    myGyroManager.LoadEntities(GridTerminalSystem,myCockpit);

}

public void Save() {}



public string mArgument = "";

public void Main(string argument, UpdateType updateSource)
{
      string aOut = "";

      if (argument.Equals("GyroCheck"))
      {
            mArgument = argument;
      }




      aOut = myGyroManager.Statistics();

      if (mArgument.Equals("GyroCheck"))
      {
            aOut = aOut + myGyroManager.ProcessGyroCheck();
      }


      Echo(aOut); 
}


public class CGI_GyroManager
{
    private List<IMyGyro> mGyros = new List<IMyGyro>();
    private IMyCockpit mReference = null;

    private bool isStart = true;
    private Vector3D aCheckStartPosition = Vector3D.Zero;
    private float POWER_CHECK_DELTA = 0.001f; 




    public string LoadEntities(IMyGridTerminalSystem pGTS, IMyCockpit pReference)
    {
        string aOut = "";
        pGTS.GetBlocksOfType(mGyros);
        mReference = pReference;
        return aOut;
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
                      aGyro.Yaw = 6.283f;
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

        foreach( IMyGyro aGyro in mGyros)
        {
                string aAlign = "not aligned"; 
                Vector3D aGyroForward = aGyro.WorldMatrix.Forward;
        
                if (aForwardReference.Equals(aGyroForward))
                {
                        aAlign = "aligned";
                }

                aOut = aOut + String.Format("{0} - {1}\n",aGyro.CustomName,aAlign);
        }

        return aOut;
    }
}