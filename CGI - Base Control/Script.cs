/**
 *    Author: Casegard
 *    Program: CGI - 
 *
 *    Version:Base Control
 *              v0.10   -  lets start with batteries
 *
 */

List<IMyBatteryBlock> myBatteries = new List<IMyBatteryBlock>();
List<IMySolarPanel> mySolarPanels = new List<IMySolarPanel>();
List<IMyTextPanel> myLCDPanels = new List<IMyTextPanel>();



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

   Func<IMyTerminalBlock, bool> aCheck = b => true;


    GridTerminalSystem.GetBlocksOfType(myBatteries, aCheck);
    GridTerminalSystem.GetBlocksOfType(mySolarPanels, aCheck);
    GridTerminalSystem.GetBlocksOfType(myLCDPanels, aCheck);
}

public void Save() {}

public void Main(string argument, UpdateType updateSource)
{
    string aOut = "";
  
    aOut += HandleBatteries(false);
    aOut += HandleSolarpanels(false);

    //Debug();

    myLCDPanels[0].WritePublicText(aOut,false);
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
        aResult += aStatusString + String.Format("\n <{0}{1}{2}> ",C_RED,aSpan.ToString("hh\\:mm\\:ss"),C_RED);
    }
    else if (aCurrentUse < 0)
    {
        TimeSpan aSpan = new TimeSpan((long)((aBatteryStore - aBatteryMaxStore) / aCurrentUse * 60 * 60 * 10000000));
        aResult += aStatusString + String.Format(" \n>{0}{1}{2}< ",C_GREEN,aSpan.ToString("hh\\:mm\\:ss"),C_GREEN);
    }
    else
    {
        aResult += aStatusString + " \n --:--:--  ";
    }

    //Echo(String.Format("   IN: {0:0.000} - Store: {1:0.000} Recharge: {2}",aBatteryInput,aBatteryStore, isRecharge));
    //Echo(String.Format("   Livetime: {0:0.000}",aBatteryTime));

    return aResult + "\n";
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