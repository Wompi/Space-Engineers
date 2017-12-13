



IMyShipController myController = null;
IMyTextPanel myPanel = null;

static string PANEL_NAME = "CGI - Echidna Panel 08";

public Program()
{
    Runtime.UpdateFrequency = UpdateFrequency.Update10;
    

    List<IMyShipController> aList = new List<IMyShipController>();
    GridTerminalSystem.GetBlocksOfType(aList);

    myController = aList[0];

    myPanel = GridTerminalSystem.GetBlockWithName(PANEL_NAME) as IMyTextPanel;

}

public void Save(){}

public void Main(string argument, UpdateType updateSource)
{
    string aOut = "";

    MyShipVelocities aVelocity = myController.GetShipVelocities();

    double aSpeed = myController.GetShipSpeed();
    Vector3D aAngular = aVelocity.AngularVelocity;
    if (aAngular == null)
    {
        Echo("Derp Angular!");
    }

    Vector3D aLinear = aVelocity.LinearVelocity;
    if (aLinear == null) 
    { 
        Echo("Derp Linear!") ;
    }

    MatrixD inverse = MatrixD.Transpose(myController.WorldMatrix); 
    Vector3D aLocalAngular = Vector3D.TransformNormal(aAngular, inverse); 
    Vector3D aLocalLinear = Vector3D.TransformNormal(aLinear, inverse);

    aLocalAngular = aLocalAngular * 180 / Math.PI;
    aAngular = aAngular * 180 / Math.PI;

    aOut = aOut + String.Format("Velocities: {0}\n Speed: {1} \n\n Angular: \n x={2} - {3}\n y={4} - {5}\n z={6} - {7}\n\n Linear: \n x={8} - {9}\n y={10} - {11}\n z={12} - {13}",
                        myController.CustomName,
                        aSpeed.ToString("000.000"),
                        aAngular.X.ToString("00.000"),aAngular.Y.ToString("00.000"),aAngular.Z.ToString("00.000"),
                                aLocalAngular.X.ToString("00.000"),aLocalAngular.Y.ToString("00.000"),aLocalAngular.Z.ToString("00.000"),
                        aLinear.X.ToString("00.000"),aLinear.Y.ToString("00.000"),aLinear.Z.ToString("00.000"),
                                aLocalLinear.X.ToString("00.000"),aLocalLinear.Y.ToString("00.000"),aLocalLinear.Z.ToString("00.000"));

    myPanel.WritePublicText(aOut,false);


}
