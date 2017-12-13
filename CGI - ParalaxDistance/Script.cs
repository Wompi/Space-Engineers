public Program()  {} 
 
public void Save() {} 
 
public void Main(string argument) 
{ 
    IMyMotorStator mRotorLeft = GridTerminalSystem.GetBlockWithName("Rotor") as IMyMotorStator ; 

    IMyCameraBlock mC1 = GridTerminalSystem.GetBlockWithName("C1") as IMyCameraBlock; 
    IMyCameraBlock mC2 = GridTerminalSystem.GetBlockWithName("C2") as IMyCameraBlock;  


    Vector3D aC1Position = mC1.GetPosition();
    Vector3D aC2Position = mC2.GetPosition(); 

    Vector3D aResult = aC1Position - aC2Position;
    
   // var aDistance =  Math.sqrt(aResult[0]*aResult[0]+aResult[1]*aResult[1]+aResult[2]*aResult[2]);

    var aDistance = Vector3D.Distance(aC1Position,aC2Position);

    if (mRotorLeft == null)
    {
        Debug("No LeftRotor found!",false);
        return;
    }
    else
    {
//        var aAngle = mRotorLeft.GetProperty("Angle");
        var aAngle = mRotorLeft.Angle;

        var aAngleDegree = aAngle*180/Math.PI;

        var aTargetDistance = aDistance / Math.Asin(aAngle);

        Echo(aAngle+"  "+aTargetDistance+" "+aDistance);

       // Debug("Left Rotor clear to engage!",false);
      //  Debug("Status: "+mRotorLeft.DetailedInfo,true);

        






      //  Debug("Angle: "+aAngle+"\n C1: "+mC1.GetPosition()+"\n C2: "+mC2.GetPosition()+"\n Result: "+aDistance+"\n TargetDistance: "+aTargetDistance,true);
    }

} 


void Debug(string pText, bool pAppend)
{
        Echo(pText);

//    IMyTextPanel aPanel = GridTerminalSystem.GetBlockWithName("CGI - LCD D1") as IMyTextPanel;

//    aPanel.WritePublicText("Debug: "+pText+"\n",pAppend);

}