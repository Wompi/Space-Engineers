
List<IMyDoor> myDoors = new List<IMyDoor>();
List<IMySensorBlock> mySensors = new List<IMySensorBlock>();

public Program()
{
    Runtime.UpdateFrequency = UpdateFrequency.Update10;
    GridTerminalSystem.GetBlocksOfType(myDoors);
    GridTerminalSystem.GetBlocksOfType(mySensors);
}

public void Save()
{

}

public void Main(string argument, UpdateType updateSource)
{
    String aOut = "";
    aOut += String.Format("Doors: {0}\nSensors: {1}\n",myDoors.Count,mySensors.Count);

    foreach(IMyDoor aDoor in myDoors)
    {
        Vector3D aPos = aDoor.GetPosition();
        aOut += String.Format("{0}\nx: {1:0.00} y: {2:0.00} z: {3:0.00}\n ",aDoor.CustomName,aPos.X,aPos.Y,aPos.Z);
    }

    foreach(IMySensorBlock aSensor in mySensors)
    {
        MyDetectedEntityInfo aEntity = aSensor.LastDetectedEntity;
        if (!aEntity.IsEmpty())
        {
            Vector3D aPos = aEntity.Position;
            IMyDoor aDoorClosest = null;
            foreach(IMyDoor aDoor in myDoors)
            {
                double aDist = Vector3D.Distance(aDoor.GetPosition(),aPos);
                if (aDoorClosest == null || Vector3D.Distance(aDoorClosest.GetPosition(),aPos) > aDist)
                {
                    aDoorClosest = aDoor;
                } 
            }
            double aD = Vector3D.Distance(aDoorClosest.GetPosition(),aPos);
            if (aDoorClosest.Status == DoorStatus.Closed && aD < 3.0f)
            {
                aDoorClosest.OpenDoor();
            }

            foreach(IMyDoor aDoor in myDoors)
            {
                if (aDoor.EntityId != aDoorClosest.EntityId)
                {
                    aDoor.CloseDoor();
                }
            }


            aOut += String.Format("Sensor: {0}\nDoor: {1}\nDist: {2}\n",aEntity.Type,aDoorClosest.CustomName,aD);
        }
    }


    Echo(aOut);
}
