    ﻿/**
 *    Author: Casegard
 *    Program: CGI - Kowari Control
 *
 *    Version:
 *              v0.10   - all the good stuff to make live easier on a small welding ship
 *                          - batteries will be recharge if connected to another grid
 *                          - tool will be switched of if jumping out of the cockpit
 *                          - reactor switched of when connected to another grid
 *              v0.12   - only grab the blocks on the grid - nice adjustment to the 'GetFirstBlockOfType' generic
 *              v0.13   - lets see if we can make a better status display - batteries / power and all the good stuff
 *                         around the landing gear and connector
 *              v0.14   - ship connectors can be a couple more than just one - so it needs a list approach
 *              v0.15   - GasTanks fill amount is now handled
 *
 */

IMyLandingGear myLandingGear = null;
IMyReactor myReactor = null;
IMyGyro myGyro = null;
IMyCockpit myCockpit = null;
IMyRemoteControl myRemoteControl = null;
IMyGasGenerator myGasGenerator = null;

IMyShipToolBase myTool = null;
IMyRadioAntenna myAntenna = null;

List<IMyShipConnector> myShipConnectors = new List<IMyShipConnector>();
List<IMyCargoContainer> myContainers = new List<IMyCargoContainer>();
List<IMyBatteryBlock> myBatteries = new List<IMyBatteryBlock>();
List<IMySolarPanel> mySolarPanels = new List<IMySolarPanel>();
List<IMyTextPanel> myLCDPanels = new List<IMyTextPanel>();
List<IMyGasTank> myGasTanks = new List<IMyGasTank>();


double mLastO2 = 0;

const float RECTOR_DEFAULT_LOAD = 50;
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

public Program()
{
    Runtime.UpdateFrequency  = UpdateFrequency.Update10;

    Func<IMyTerminalBlock, bool> aCheck = b => b.CubeGrid == Me.CubeGrid;

    GetFirstBlockOfType(ref myLandingGear, aCheck);
    GetFirstBlockOfType(ref myReactor, aCheck);
    GetFirstBlockOfType(ref myGyro, aCheck);
    GetFirstBlockOfType(ref myCockpit, aCheck);
    GetFirstBlockOfType(ref myRemoteControl, aCheck);
    GetFirstBlockOfType(ref myGasGenerator, aCheck);
    GetFirstBlockOfType(ref myTool, aCheck);
    GetFirstBlockOfType(ref myAntenna, aCheck);

    GridTerminalSystem.GetBlocksOfType(myBatteries, aCheck);
    GridTerminalSystem.GetBlocksOfType(mySolarPanels, aCheck);
    GridTerminalSystem.GetBlocksOfType(myLCDPanels, aCheck);
    GridTerminalSystem.GetBlocksOfType(myShipConnectors, aCheck);
    GridTerminalSystem.GetBlocksOfType(myContainers, aCheck);
    GridTerminalSystem.GetBlocksOfType(myGasTanks,aCheck);

    foreach( IMyTextPanel aPanel in myLCDPanels)
    {
        aPanel.FontSize = 1f;
        aPanel.Font = "Monospace";
        aPanel.ShowPublicTextOnScreen();
    }
}

public void Save() {}

public void Main(string argument, UpdateType updateSource)
{
    string aOut = "";
    // Branch out the functionallity if we are connected or if we are in space
    bool isConnected = IsShipConnected();

    aOut += HandleBatteries(isConnected);
    //aOut += HandleReactor(isConnected);
    //aOut += HandleTool(isConnected);
    //aOut += HandleSolarpanels(isConnected);
    //aOut += HandleAntenna(isConnected);
    aOut += HandleGasTanks(isConnected);

    //Debug();

    myLCDPanels[0].WritePublicText(aOut,false);
}

/**
 *  NOTE: Gives back the status true if the ship is at least with one connector connected to something
 *
 */
public bool IsShipConnected()
{
    bool aResult = false;
    foreach(IMyShipConnector aConnector in myShipConnectors)
    {
        if (aConnector.Status == MyShipConnectorStatus.Connected)
        {
            aResult = true;
            break;
        }
    }
    return aResult;
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
        aResult += aStatusString + String.Format(" <{0}{1}{2}> ",C_RED,aSpan.ToString("d\\d\\ hh\\:mm\\:ss"),C_RED);
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

    return aResult + "\n";
}

/**
 *   NOTE: currently this is just a on/off switch when connected or not.
 *
 *   TODO:  - think about more advanced stuff - like switching the reactor off in dependence of the battery state
 *          - also landing gear handling
 *
 */
public string HandleReactor(bool pIsConnected)
{
    string aResult = "";

    string aAction = "OnOff_On";
    if (pIsConnected)
    {
        aAction = "OnOff_Off";
    }
    myReactor.ApplyAction(aAction);
    string aStatusString = myReactor.Enabled ? C_GREEN + " (on)" : C_RED + " (off)";

    aResult += String.Format("Reactor: {0} - {1:0.000} kw\n",
                aStatusString,
                myReactor.CurrentOutput);

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
public string HandleTool(bool pIsConnected)
{
    string aResult = "";
    char aStatus = C_RED;

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
        else
        {
            aStatus = myTool.Enabled ? C_GREEN : C_YELLOW;
        }
    }
    aResult += String.Format("Tool: {0} {1}\n",aStatus,myTool.CustomName);
    return aResult;
}

public string HandleSolarpanels(bool pIsConnected)
{
    string aResult = "";
    double aSolarOutput = 0;
    foreach ( IMySolarPanel aPanel in mySolarPanels)
    {
        aSolarOutput += aPanel.CurrentOutput;
    }

    aResult += String.Format("Solar: {0} - {1:0.000} kw\n",
                    mySolarPanels.Count,
                    aSolarOutput);
    return aResult;
}

public string HandleAntenna(bool pIsConnected)
{
    string aResult = "";

    char aStatus = C_RED;
    // When the ship is actively controlled the antenna can be switched off
    if (myCockpit.IsUnderControl)
    {
        pIsConnected = true;
        aStatus = C_YELLOW;
    }

    double aRadius =  myAntenna.Radius;

    if (pIsConnected)
    {
        myAntenna.Enabled = false;
    }
    else
    {
        myAntenna.Enabled = true;
        aStatus = C_GREEN;
    }
    aResult += String.Format("Antenna: {0} {1} m  {2} kw\n",
                aStatus,
                aRadius,
                (aRadius * ANTENNA_ENERGY_FACTOR));
    return aResult;
}


// TODO: split for oxygen and hydrogen
public string HandleGasTanks(bool pIsConnected)
{
    string aResult = "";
    if (myGasTanks.Count == 0) return "No Gas Tanks\n";

    double aGas = 0;
    float aCapacity = 0;

    foreach( IMyGasTank aTank in myGasTanks)
    {
        aGas += aTank.Capacity * aTank.FilledRatio;
        aCapacity += aTank.Capacity;
    }

    double aDelta = mLastO2 - aGas;

    aResult += String.Format("Tanks: {0:0}/{1}  \nDelta: {2}",aGas,aCapacity,aDelta);

    mLastO2 = aGas;

    return aResult;
}

/**
 *  NOTE: Helper function to get the fillstatus for a terminalblock that has an aInventory
 *
 *  TODO: for now only the first inventory is used - beware that blocks can have multiple inventories
 */
public float GetInventoryFillPercentage(IMyTerminalBlock pTerminalBlock)
{
    float aResult = -1;
    if (pTerminalBlock.HasInventory)
    {
        IMyInventory aInventory = pTerminalBlock.GetInventory(0);
        aResult = (float) aInventory.CurrentVolume / (float) aInventory.MaxVolume;
    }
    return aResult;
}
