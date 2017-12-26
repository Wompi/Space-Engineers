/**
 *    Author: Casegard
 *    Program: CGI - Rakali Control
 *
 *    Version:
 *              v0.10   - all the good stuff to make live easier on a small space mining ship
 *                          - batteries will be recharge if connected to another grid
 *                          - handling for the drills
 *                          - reactor switched of when connected to another grid
 *
 */

IMyShipConnector myShipConnector = null;
IMyCockpit myCockpit = null;
IMyRemoteControl myRemoteControl = null;
IMyGasGenerator myGasGenerator = null;
IMyRadioAntenna myAntenna = null;
IMyCargoContainer myContainer = null;
IMyOreDetector myOreDetector = null;


List<IMyBatteryBlock> myBatteries = new List<IMyBatteryBlock>();        // The ship has 4
List<IMyTextPanel> myLCDPanels = new List<IMyTextPanel>();             // The ship can have as many as it wants
List<IMyShipDrill> myDrills = new List<IMyShipDrill>();                 // The ship has 10 (maybe this will be adjusted in the future)
List<IMyShipConnector> myEjectors = new List<IMyShipConnector>();       // The ship has 2
List<IMyCameraBlock> myCameras = new List<IMyCameraBlock>();            // The ship can have as many as it wants but each camera should have a protocoll
List<IMyProgrammableBlock> myCodeBlocks = new List<IMyProgrammableBlock>();   // The ship can have as many as it wants
List<IMySmallGatlingGun> myGattlingGuns = new List<IMySmallGatlingGun>(); // The ship has 2
List<IMyGyro> myGyros = new List<IMyGyro>();                            // The ship has 2
List<IMyReactor> myReactors = new List<IMyReactor>();                   // the ship has 2 small reactors
List<IMyConveyorSorter> mySorters = new List<IMyConveyorSorter>();       // The ship has 4 mainly for stone sorting but could be used for ejection as well
List<IMyThrust> myThrusters = new List<IMyThrust>();                    // The ship has ?? small ion thrusters and 2 large small ion thruster
List<IMyReflectorLight> mySpotLights = new List<IMyReflectorLight>();   // The ship has 2 in the front


const double ANTENNA_ENERGY_FACTOR = 0.004;    // NOTE: could be changed in future versions for now the energy input is linear (4W)

const char C_RED = '\uE200';
const char C_GREEN = '\uE120';
const char C_BLUE = '\uE104';
const char C_YELLOW = '\uE220';
const char C_WHITE = '\uE2FF';
const char C_BLACK = '\uE100';

public void GetFirstBlockOfType<T>( ref T pBlock, Func<T, bool> pCheck = null ) where T : class
{
    List<T> aList = new List<T>();
    GridTerminalSystem.GetBlocksOfType<T>(aList, pCheck);
    if (aList.Count > 0) pBlock = aList[0];
}

﻿public Program()
{
    Runtime.UpdateFrequency  = UpdateFrequency.Update10;
    Func<IMyTerminalBlock, bool> aCheck = b => b.CubeGrid == Me.CubeGrid;

    // NOTE: maybe this should be handled with groups IDK
    Func<IMyTerminalBlock, bool> aEjectorCheck = b => b.CubeGrid == Me.CubeGrid && b.BlockDefinition.SubtypeName.Equals("ConnectorSmall");
    Func<IMyTerminalBlock, bool> aConnectorCheck = b => b.CubeGrid == Me.CubeGrid && b.BlockDefinition.SubtypeName.Equals("ConnectorMedium");

    GetFirstBlockOfType(ref myShipConnector, aConnectorCheck);
    GetFirstBlockOfType(ref myCockpit, aCheck);
    GetFirstBlockOfType(ref myRemoteControl, aCheck);
    GetFirstBlockOfType(ref myGasGenerator, aCheck);
    GetFirstBlockOfType(ref myAntenna, aCheck);
    GetFirstBlockOfType(ref myContainer, aCheck);
    GetFirstBlockOfType(ref myOreDetector, aCheck);

    GridTerminalSystem.GetBlocksOfType(myBatteries, aCheck);
    GridTerminalSystem.GetBlocksOfType(myDrills, aCheck);
    GridTerminalSystem.GetBlocksOfType(myEjectors, aEjectorCheck);
    GridTerminalSystem.GetBlocksOfType(myLCDPanels, aCheck);
    GridTerminalSystem.GetBlocksOfType(myCodeBlocks, aCheck);
    GridTerminalSystem.GetBlocksOfType(myGattlingGuns, aCheck);
    GridTerminalSystem.GetBlocksOfType(myGyros, aCheck);
    GridTerminalSystem.GetBlocksOfType(myReactors, aCheck);
    GridTerminalSystem.GetBlocksOfType(mySorters, aCheck);
    GridTerminalSystem.GetBlocksOfType(myCameras, aCheck);
    GridTerminalSystem.GetBlocksOfType(myThrusters, aCheck);
    GridTerminalSystem.GetBlocksOfType(mySpotLights, aCheck);




    // NOTE: uncomment this if the bug for chrashing the game is be fixed
    // foreach( IMyTextPanel aPanel in myTextPanels)
    // {
    //     aPanel.FontSize = 1f;
    //     aPanel.Font = "Monospace";
    //     aPanel.ShowPublicTextOnScreen();
    // }

}

public void Save() {}

public void Main(string argument, UpdateType updateSource)
{
    string aOut = "";
    // Branch out the functionallity if we are connected or if we are in space
    if (myShipConnector.Status == MyShipConnectorStatus.Connected)
    {
        aOut += HandleBatteries(true);
        aOut += HandleDrills(true);
    }
    else
    {
        aOut += HandleBatteries(false);
    }

    Debug();

    myLCDPanels[1].WritePublicText(aOut,false);
}

/**
  *  NOTE: for now set the batteries to recharge when we are connected
  *
  *  TODO: make this a bit more robust and enhance the functionality - should be dependent on energy / landing gear and so on
  */
public string HandleBatteries(bool isConnected)
{
    string aResult = "";

    bool isRecharge = isConnected;

    string aStatusString = "";

    double aBatteryInput = 0;
    double aBatteryOutput = 0;
    double aBatteryStore = 0;
    double aBatteryMaxStore = 0;

    foreach( IMyBatteryBlock aBattery in myBatteries)
    {
        if (!aBattery.IsFunctional || !aBattery.Enabled)
        {
            aStatusString += C_RED;
            continue;
        }

        aBattery.OnlyRecharge = isRecharge;
        aStatusString += isRecharge ? C_YELLOW : C_GREEN;

        aBatteryInput += aBattery.CurrentInput;
        aBatteryOutput += aBattery.CurrentOutput;
        aBatteryStore += aBattery.CurrentStoredPower;
        aBatteryMaxStore += aBattery.MaxStoredPower;
    }

    double aCurrentUse = aBatteryOutput - aBatteryInput;

    if (aCurrentUse > 0)
    {
        TimeSpan aSpan = new TimeSpan((long)(aBatteryStore / aCurrentUse * 60 * 60 * 10000000));
        aResult += aStatusString + String.Format(" <{0}{1}{2}> ",C_RED,aSpan.ToString("hh\\:mm\\:ss"),C_RED);
    }
    else if (aCurrentUse < 0)
    {
        TimeSpan aSpan = new TimeSpan((long)((aBatteryStore - aBatteryMaxStore) / aCurrentUse * 60 * 60 * 10000000));
        aResult += aStatusString + String.Format(" >{0}{1}{2}< ",C_GREEN,aSpan.ToString("hh\\:mm\\:ss"),C_GREEN);
    }
    else
    {
        aResult += aStatusString + "  --:--:--  ";
    }

    //Echo(String.Format("   IN: {0:0.000} - Store: {1:0.000} Recharge: {2}",aBatteryInput,aBatteryStore, isRecharge));
    //Echo(String.Format("   Livetime: {0:0.000}",aBatteryTime));

    return aResult;
}

/**
 * NOTE: -  make the drills stop when they are full
 *       - maybe make the drills start when close to an asteroid (camera raycast)
 *       - thinking about a inventory shuffle function - then I would not need the sorters and can shift
 *         ore to the drills that are almost full
 *
 */
public string HandleDrills(bool isConnected)
{
    string aResult = "";
    // NOTE: not realy something to do when connected right now
    //       when connected and unloading I want to see when the drill is empty

    // if (isConnected)
    // {
    //     // hmm when connected it should shut down the drills - but this is very unlikely and there are alot of
    //     // checks to do hmmmmmmm idk
    // }
    // else
    {
        foreach ( IMyShipDrill aDrill in myDrills)
        {
            // Check the gneral functionality of the drill
            if (!aDrill.IsFunctional)
            {
                aResult += aResult += String.Format("Drill: {0}\n  Status: {1} (damaged)\n\n",
                            aDrill.CustomName,
                            C_RED);
                continue;
            }
            // Check if the drill is working - if not check the inventory and maybe the other drill
            // - the whole working process is a bit finicky
            // - states: normal flight - no Drills
            //           connected - no Drills
            //           drilling - drills on
            //           drilling but one drill is full - drill of
            string aStatus = aDrill.Enabled ? C_GREEN + " (online)" : C_RED + " (offline)";

            IMyInventory aInventory = aDrill.GetInventory(0);
            float aFillPercent = (float) aInventory.CurrentVolume / (float) aInventory.MaxVolume;

            // TODO: switch the drill off when full - this is a bit tricky because of manual toggle both drills
            // if (aFillPercent > 0.90)
            // {
            //     aDrill.Enabled = false;
            // }
            int aLength = 60;
            int aIndicator = (int) (aFillPercent * aLength);
            string aGauge = "  [ " + new string('!',aIndicator) + new string('.',aLength-aIndicator)+"]";

            aResult += String.Format("Drill: {0}\n  Status: {1}\n{2}\n",
                        aDrill.CustomName,
                        aStatus,
                        aGauge);
        }
    }
    return aResult;
}


// TODO: maybe use this to rename the components as well later
public void Debug()
{
    Echo("Connector: "+myShipConnector.CustomName);
    Echo("Cockpit: "+myCockpit.CustomName);
    Echo("Remote: "+myRemoteControl.CustomName);
    Echo("Oxygen Generator: "+myGasGenerator.CustomName);
    Echo("Medium Container: "+myContainer.CustomName);
    Echo("Ore Detetector: "+myOreDetector.CustomName);

    Echo("Batteries: "+myBatteries.Count);
    Echo("LCD Panels: "+myLCDPanels.Count);
    Echo("Drills: "+myDrills.Count);
    Echo("Ejectors: "+myEjectors.Count);
    Echo("Cameras: "+myCameras.Count);
    Echo("Code Blocks: "+myCodeBlocks.Count);
    Echo("Gyros: "+myGyros.Count);
    Echo("Sorters: "+mySorters.Count);
    Echo("Thrusters: "+myThrusters.Count);
    Echo("Spot Lights: "+mySpotLights.Count);
}
