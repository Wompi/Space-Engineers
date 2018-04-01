
IMyPistonBase myPiston = null;
IMyMotorStator myRotor = null;


const String PISTON_NAME = "Piston";
const String ROTOR_NAME = "CGI - MB01 Advanced Rotor (Grinderpit)";

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


    float aRotorAngle = myRotor.Angle;
    float aCurrentPistonPosition = myPiston.CurrentPosition;


    double aExtend = (5+1.25) / Math.Cos(aRotorAngle);


    Echo("Piston: "+aCurrentPistonPosition);
    Echo("Rotor: "+aRotorAngle);

    Echo("Extend: " + (aExtend-6.125));
    Echo("Delta: "+(aCurrentPistonPosition-(aExtend-6.125)));

    myPiston.Velocity = -(float)(aCurrentPistonPosition-(aExtend-6.125));



}

public double GetPistonSpeed(float pAngle)
{
    return   (-0.00422740453419959120 * Math.Pow(pAngle,0) + 0.027903335149905922 * Math.Pow(pAngle,1));          
}
