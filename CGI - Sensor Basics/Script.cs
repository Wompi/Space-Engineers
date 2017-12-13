/**
 *  Author:     Casegard
 *  Program: CGI - Sensor Basics
 *
 *  Version:
 *          v0.10 - just a simple state where some properties are displayed
 *
 */

List<IMySensorBlock> mSensors = new List<IMySensorBlock>();
IMyTextPanel myPanel = null;
static string SENSOR_PANEL_NAME = "CGI - Echidna Panel 03";

public Program() 
{ 
    Runtime.UpdateFrequency = UpdateFrequency.Update10;
    GridTerminalSystem.GetBlocksOfType<IMySensorBlock>(mSensors);

    // TODO: the panel handling should be done with the CustomData as well 
    myPanel = GridTerminalSystem.GetBlockWithName(SENSOR_PANEL_NAME) as IMyTextPanel; 

} 
 
public void Save() {} 
 
public void Main(string argument, UpdateType updateSource) 
{ 
    string aType = "";
    //if ((updateSource & UpdateType.Antenna) != 0)
    {
        aType = aType + String.Format("Update: Antenna {0} \n",argument);
    }



    string aPanelOutput = "";
    string aGPS = "";
    foreach(IMySensorBlock aSensor in mSensors)
    {
        List<MyDetectedEntityInfo> aDetections = new List<MyDetectedEntityInfo>();
        aSensor.DetectedEntities(aDetections);

        if (aDetections.Count > 0)
        {
            Echo("Name: " + aSensor.DisplayNameText+ " Detected: " + aDetections.Count);

            string aOutput = "";
            foreach( MyDetectedEntityInfo aEntityInfo in aDetections)
            {
                aOutput = aOutput + String.Format(
                                        "ID: {0} \n Name: {1} \n Type: {2} \n HitPosition: {3} \n Orientation: {4} \n Velocity: {5} \n  TimeStamp: {6}  \n\n ",
                                    aEntityInfo.EntityId,
                                    aEntityInfo.Name,
                                    aEntityInfo.Type,
                                    aEntityInfo.Position.ToString(),
                                    aEntityInfo.Orientation.ToString(),
                                    aEntityInfo.Velocity.ToString(),
                                    aEntityInfo.TimeStamp
                                );
                Echo("ID: "+aEntityInfo.EntityId);
                aGPS = aGPS + String.Format("GPS: {0}:{1}:{2}:{3}:\n", 
                            aEntityInfo.EntityId,aEntityInfo.Position.X,aEntityInfo.Position.Y,aEntityInfo.Position.Z); 
            }
            Me.CustomData = aType; 
            Me.CustomData = Me.CustomData + "\n" + aOutput;
            Me.CustomData = Me.CustomData + "\n" + aGPS;
            aPanelOutput = aPanelOutput + aOutput;
        }
    }

    myPanel.WritePublicText(aType,true); 
    //myPanel.WritePublicText(aPanelOutput,true);
    //myPanel.WritePublicText(aGPS,true);

} 
