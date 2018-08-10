
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
    // NOTE: polynom calulated for rotation speed 60 (only viable for small rotors I guess)
    double[] aCoefficients = {
        -4.2274045341959120e-003,
         2.7903335149905922e-002,
         2.5230300449510649e-003,
        -2.6622603046188090e-004,
         1.6280318195653176e-005,
        -5.5129434077977062e-007,
         1.0840860664005237e-008,
        -1.1421442081557958e-010,
         5.1096372466975643e-013,
    };

    double aResult = 0;
    for (int i=0; i < aCoefficients.Length; i++)
    {
        aResult+= aCoefficients[i] * Math.Pow(pAngle,i);
    }

    return aResult;
}
