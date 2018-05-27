
IMyPistonBase myPiston = null;
IMyMotorStator myRotor = null;


const String PISTON_NAME = "CGI - MB01 Piston 02 (Piston Experiment)";
const String ROTOR_NAME = "CGI - MB01 Rotor 27 (Piston Experiment)";

public Program() 
{ 
    Runtime.UpdateFrequency = UpdateFrequency.Update1;

    myPiston = GridTerminalSystem.GetBlockWithName(PISTON_NAME) as IMyPistonBase;
    myRotor = GridTerminalSystem.GetBlockWithName(ROTOR_NAME) as IMyMotorStator;
} 
 
public void Save() 
{ 
  
} 
 
public void Main(string argument, UpdateType updateSource) 
{ 
    if (myPiston == null)
    {
        Echo("No Piston found!");
        return;
    } 
    if (myRotor == null)
    {
        Echo("No Rotor found!");
        return;
    }


    float aRotorAngle = myRotor.Angle + (float)(Math.PI/2.0);
    float aCurrentPistonPosition = myPiston.CurrentPosition;


    double aExtend = (3+0.25) / Math.Cos(aRotorAngle);


    Echo("Piston: "+aCurrentPistonPosition);
    Echo("Rotor: "+aRotorAngle);

    Echo("Extend: " + (aExtend-3.25));
    Echo("Delta: "+(aCurrentPistonPosition-(aExtend-3.25)));

    myPiston.Velocity = -(float)(aCurrentPistonPosition-(aExtend-3.25));



} 

public double GetPistonSpeed(float pAngle)
{
    return   (-0.00422740453419959120 * Math.Pow(pAngle,0) + 0.027903335149905922 * Math.Pow(pAngle,1));          
}
