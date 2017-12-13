IMyShipController mShipController = null;
List<IMySensorBlock> mSensors = new List<IMySensorBlock>();

static string SHIP_CONTROLLER_NAME = "CGI - Sensor Planetary Remote Control";

public Program() 
{
    mShipController = GridTerminalSystem.GetBlockWithName(SHIP_CONTROLLER_NAME) as IMyShipController;
    GridTerminalSystem.GetBlocksOfType<IMySensorBlock>(mSensors);
} 
 
public void Save() {} 
 
public void Main(string argument) 
{ 
    //string aOutput = "";
    string aCSV = Me.CustomData;
    if (aCSV.Length == 0)
    {
        aCSV = "posX;posY;posZ;planet_posX;planet_posY;planet_posZ;planet_elevation_surface;planet_gravityX;planet_gravityY;planet_gravityZ;planet_gravity \n";     
    }    



    //string aPosition = Me.GetPosition().ToString();
    Vector3D aPosition = Me.GetPosition();
    //Echo(aPosition);
    //aOutput = aOutput + String.Format("Position: {0} \n",aPosition);
   aCSV = aCSV + String.Format("{0};{1};{2};",aPosition.X,aPosition.Y,aPosition.Z); 

 
    Vector3D aPlanetPosition = new Vector3D(0,0,0); 
    if (mShipController.TryGetPlanetPosition(out aPlanetPosition)) 
    { 
           //aOutput = aOutput + String.Format("PlanetPosition: {0:F2} \n",aPlanetPosition); 
           aCSV = aCSV + String.Format("{0};{1};{2};",aPlanetPosition.X,aPlanetPosition.Y,aPlanetPosition.Z);  
    } 
    else 
    { 
       // aOutput  = aOutput  + "PlanetPosition: unknown \n"; 
    } 
 
    double aElevation = 0; 
    if (mShipController.TryGetPlanetElevation(MyPlanetElevation.Surface, out aElevation)) 
    { 
        //aOutput  = aOutput  + String.Format("PlanetElevation: {0:F2} (surface) \n",aElevation); 
        aCSV = aCSV + String.Format("{0};",aElevation);
    } 
    else 
    { 
        //aOutput  = aOutput  + "PlanetElevation: unknown \n"; 
    } 

    Vector3D aNaturalGravity = mShipController.GetNaturalGravity(); 
   // double aLength = aNaturalGravity.Normalize();    // the same as the original length aka the gravity value in [m/s^2] (8.8 on mars) 
    //aOutput  = aOutput  +  
      //          String.Format("NaturalGravity: {0} G: {1}  \n",  
        //                        aNaturalGravity.ToString("0.00"), 
          //                      aLength.ToString("0.00"));
    aCSV = aCSV  + String.Format("{0};{1};{2}",aNaturalGravity.X,aNaturalGravity.Y,aNaturalGravity.Z);


 
    foreach(IMySensorBlock aSensor in mSensors) 
    { 
        List<MyDetectedEntityInfo> aDetections = new List<MyDetectedEntityInfo>(); 
        aSensor.DetectedEntities(aDetections); 
 
        if (aDetections.Count > 0) 
        { 
            Echo("Name: " + aSensor.DisplayNameText+ " Detected: " + aDetections.Count); 
 
           string aSensorOutput = ""; 
            foreach( MyDetectedEntityInfo aEntityInfo in aDetections) 
            { 
                aSensorOutput = aSensorOutput + String.Format( 
                                        "ID: {0} \n Name: {1} \n Type: {2} \n HitPosition: {3} \n Orientation: {4} \n Velocity: {5} \n  TimeStamp: {6}  \n\n ", 
                                    aEntityInfo.EntityId, 
                                    aEntityInfo.Name, 
                                    aEntityInfo.Type, 
                                    aEntityInfo.HitPosition.ToString(), 
                                    aEntityInfo.Orientation.ToString(), 
                                    aEntityInfo.Velocity,ToString(), 
                                    aEntityInfo.TimeStamp 
                                ); 
                Echo("ID: "+aEntityInfo.EntityId); 
            } 
            aSensor.CustomData = aSensorOutput; 
        } 
    }

    //Me.CustomData = aOutput;
   // Echo(aOutput);
   Me.CustomData = aCSV + "\n"; 
   Echo(aCSV);
} 
