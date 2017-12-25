/**
 *    Author: Casegard
 *    Program: CGI - Kowari Control
 *
 *    Version:
 *              v0.10   - all the good stuff to make live easier on a small welding ship
 *                          - batteries will be recharge if connected to another grid
 *                          - tool will be switched of if jumping out of the cockpit
 *                          - reactor switched of when connected to another grid
 *              v0.12   - only grab the blocks on the grid - nice adjustment to the 'GetFirstBlockOfType' generic
 *
 *
 */

IMyShipConnector myShipConnector = null;
IMyLandingGear myLandingGear = null;
IMyReactor myReactor = null;
IMyGyro myGyro = null;
IMyCockpit myCockpit = null;
IMyRemoteControl myRemoteControl = null;
IMyGasGenerator myGasGenerator = null;

IMyShipToolBase myTool = null;
//IMyShipWelder myWelder = null;
//IMyShipGrinder myGrinder = null;
IMyRadioAntenna myAntenna = null;

List<IMyBatteryBlock> myBatteries = new List<IMyBatteryBlock>();
List<IMySolarPanel> mySolarPanels = new List<IMySolarPanel>();
List<IMyTextPanel> myTextPanels = new List<IMyTextPanel>();


static double ANTENNA_ENERGY_FACTOR = 0.004;    // NOTE: could be changed in future versions for now the energy input is linear (4W)

int mTick = 0;

public void GetFirstBlockOfType<T>( ref T pBlock, Func<IMyTerminalBlock, bool> pCheck = null ) where T : class
{
    List<T> aList = new List<T>();
    GridTerminalSystem.GetBlocksOfType<T>(aList, pCheck);
    if (aList.Count > 0) pBlock = aList[0];
}

public Program()
{
    Runtime.UpdateFrequency  = UpdateFrequency.Update10;

    Func<IMyTerminalBlock, bool> aCheck = b => b.CubeGrid == Me.CubeGrid;

    GetFirstBlockOfType(ref myShipConnector, aCheck);
    GetFirstBlockOfType(ref myLandingGear, aCheck);
    GetFirstBlockOfType(ref myReactor, aCheck);
    GetFirstBlockOfType(ref myGyro, aCheck);
    GetFirstBlockOfType(ref myCockpit, aCheck);
    GetFirstBlockOfType(ref myRemoteControl, aCheck);
    GetFirstBlockOfType(ref myGasGenerator, aCheck);
    //GetFirstBlockOfType(ref myWelder, aCheck);
    //GetFirstBlockOfType(ref myGrinder, aCheck);
    GetFirstBlockOfType(ref myTool, aCheck);
    GetFirstBlockOfType(ref myAntenna, aCheck);


    GridTerminalSystem.GetBlocksOfType(myBatteries, aCheck);
    GridTerminalSystem.GetBlocksOfType(mySolarPanels, aCheck);
    GridTerminalSystem.GetBlocksOfType(myTextPanels, aCheck);


    foreach( IMyTextPanel aPanel in myTextPanels)
    {
        aPanel.FontSize = 1f;
        aPanel.Font = "Monospace";
        aPanel.ShowPublicTextOnScreen();
    }
}

public void Save() {}

public void Main(string argument, UpdateType updateSource)
{
    mTick++;


    Echo(myShipConnector.Status.ToString());
    Echo(myLandingGear.LockMode.ToString());


    Echo("Batteries: "+myBatteries.Count);
    foreach ( IMyBatteryBlock aBattery in myBatteries)
    {
        string isRecharge = aBattery.OnlyRecharge ? "Y" : "N";
        double aBatteryInput = aBattery.CurrentInput;
        double aBatteryOutput = aBattery.CurrentOutput;
        double aBatteryStore = aBattery.CurrentStoredPower;

        double aCurrentUse = aBatteryOutput - aBatteryInput;
        double aBatteryTime = -1;
        if (aCurrentUse > 0)
        {
            aBatteryTime = aBatteryStore / aCurrentUse;
        }


        Echo(String.Format("   IN: {0:0.000} - Store: {1:0.000} Recharge: {2}",aBatteryInput,aBatteryStore, isRecharge));
        Echo(String.Format("   Livetime: {0:0.000}",aBatteryTime));
    }
    bool aBatteryState = HandleBatteries();
    Echo(aBatteryState ? "Connected: batteries recharge" : "Not connected: batteries work");


    Echo("Reactor: "+myReactor.CurrentOutput);
    bool aReactorState = HandleReactor();
    Echo(aReactorState ? "Connected: reactor offline" : "Not Connected: reactor online");

    double aSolarOutput = 0;
    foreach ( IMySolarPanel aPanel in mySolarPanels)
    {
        aSolarOutput += aPanel.CurrentOutput;
    }

    Echo("Solar: "+mySolarPanels.Count + " - "+aSolarOutput.ToString("0.000"));


    HandleTool();





    double aRadius =  myAntenna.Radius;
    Echo("Antenna: "+aRadius.ToString("0.00")+ " Input: "+(aRadius * ANTENNA_ENERGY_FACTOR).ToString("0.000"));


    string aIndicator = myCockpit.MoveIndicator.ToString("00.00");



    string aOutput = "\uE120";
    MyShipMass aMass = myRemoteControl.CalculateShipMass();
    Vector3D aGravity = myRemoteControl.GetNaturalGravity();
    aOutput  = aOutput + String.Format("{0}: \n {1} / {2} \n Gravity: {3} \n\n",
                myRemoteControl.CustomName,aMass.BaseMass,aMass.TotalMass,aGravity.Length().ToString("0.000"));

    aOutput = aOutput + aIndicator + "\n";


    Vector3D aCenterOfMass = myCockpit.CenterOfMass;
    Vector3D aVolumeCenter  = Me.CubeGrid.WorldVolume.Center;
    double aVolumeRadius = Me.CubeGrid.WorldVolume.Radius;
    double aSize = Me.CubeGrid.GridSize;


    double  aDistance = Vector3D.Distance(aVolumeCenter,aCenterOfMass);

    aOutput += String.Format("Grid Volume:\n Center: {0}\n Radius: {1}\n Size: {2}\n Blocks: {3}\n",
                aDistance,
                aVolumeRadius,
                aSize,
                aVolumeRadius/aSize);

     Me.CustomData = String.Format("GPS: Ship Center:{0}:{1}:{2}:",aVolumeCenter.X,aVolumeCenter.Y,aVolumeCenter.Z);

    Vector3D aLinearVelocity = myCockpit.GetShipVelocities().LinearVelocity;
    double aSpeed = aLinearVelocity.Length();

    Vector3D aUp = myCockpit.WorldMatrix.Up;
    Vector3D aLeft = myCockpit.WorldMatrix.Left;
    Vector3D aForward = myCockpit.WorldMatrix.Forward;

    double aUpVelo = aLinearVelocity.Dot(aUp);
    double aForwardVelo = aLinearVelocity.Dot(aForward);
    double aLeftVelo = aLinearVelocity.Dot(aLeft);


   // double aBreakDistanceUp = -aSpeed / (2 * )


    aOutput += String.Format("Speed: {0}\n [F] Speed: {1}\n [L] Speed: {2}\n [U] Speed: {3}",
                aSpeed,
                aForwardVelo,
                aLeftVelo,
                aUpVelo);


    myTextPanels[0].WritePublicText(aOutput,false);
}

/**
  *  Note: for now set the batteries to recharge when we are connected
  *
  *  TODO: make this a bit more robust and enhance the functionality - should be dependent on energy / landing gear and so on
  */
public bool HandleBatteries()
{
    bool aResult = false;

    bool isRecharge = false;
    if (myShipConnector.Status == MyShipConnectorStatus.Connected)
    {
        isRecharge = true;
        aResult = true;
    }

     foreach( IMyBatteryBlock aBattery in myBatteries)
     {
        aBattery.OnlyRecharge = isRecharge;
     }

    return aResult;
}

public bool HandleReactor()
{
    bool aResult = false;

    string aAction = "OnOff_On";
    if (myShipConnector.Status == MyShipConnectorStatus.Connected)
    {
        aAction = "OnOff_Off";
        aResult = true;
    }

    myReactor.ApplyAction(aAction);
    return aResult;
}

/**
  *    TODO: find a better way to check which tool is equipped
  *                 - a tool can be changed by grinding it down and replace it
  *                 - also a tool can be damaged
  *
  *     TODO: implement a remote control check
  *                 - a ship can be active remote controlled or be controlled by the autopilot
  *
  *     TODO: handle the return result
  */
public bool HandleTool()
{
    bool aResult = false;

    if (myTool == null || !myTool.IsFunctional)
    {
        // NOTE: after the new check we wait one cycle by jumping out of the method
        GetFirstBlockOfType(ref myTool, b => b.CubeGrid == Me.CubeGrid);
    }
    else
    {
        if (!myCockpit.IsUnderControl)
        {
            if (myTool.Enabled)
            {
                myTool.ApplyAction("OnOff_Off");
            }
        }
        aResult = true;
    }
    return aResult;
}
