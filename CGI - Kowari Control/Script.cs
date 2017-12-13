/**
 *    Author: Casegard
 *    Program: CGI - Kowari Control
 *
 *    Version:
 *              v0.10   - all the good stuff to make live easier on a small welding ship
 *                          - batteries will be recharge if connected to another grid 
 *                          - tool will be switched of if jumping out of the cockpit
 *                          - reactor switched of when connected to another grid 
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



Dictionary<Base6Directions.Direction,List<IMyThrust>> myThrusters = new Dictionary<Base6Directions.Direction,List<IMyThrust>>();
IMyTextPanel myPanel = null;

int mTick = 0;


static string THRUST_PANEL_NAME = "CGI - Kowari Panel 01";

public T GetFirstBlockOfType<T>(T pBlock) where T : class
{
    List<T> aList = new List<T>();
    GridTerminalSystem.GetBlocksOfType(aList);

    T aResult = null;
    if (aList.Count > 0) aResult = aList[0];

    return aResult;
}

public Program() 
{ 
    Runtime.UpdateFrequency  = UpdateFrequency.Update10;

    myShipConnector = GetFirstBlockOfType(myShipConnector);
    myLandingGear = GetFirstBlockOfType(myLandingGear);
    myReactor = GetFirstBlockOfType(myReactor); 
    myGyro = GetFirstBlockOfType(myGyro); 
    myCockpit = GetFirstBlockOfType(myCockpit);  
    myRemoteControl = GetFirstBlockOfType(myRemoteControl);
    myGasGenerator = GetFirstBlockOfType(myGasGenerator); 
    //myWelder = GetFirstBlockOfType(myWelder);  
    //myGrinder = GetFirstBlockOfType(myGrinder); 
    myTool =  GetFirstBlockOfType(myTool);
    myAntenna = GetFirstBlockOfType(myAntenna);   
      
    
    GridTerminalSystem.GetBlocksOfType(myBatteries); 
    GridTerminalSystem.GetBlocksOfType(mySolarPanels);  
    GridTerminalSystem.GetBlocksOfType(myTextPanels);   
        
    // TODO: this is a bit ugly - it finds the RemoteControl Block but it should get it from the CustomData instead 
    // to account for the right one if you have multiple 
    Dictionary<Base6Directions.Direction,Vector3D> aVectorReference = new Dictionary<Base6Directions.Direction,Vector3D>();
    aVectorReference[Base6Directions.Direction.Forward] = myRemoteControl.WorldMatrix.Forward;
    aVectorReference[Base6Directions.Direction.Backward] = myRemoteControl.WorldMatrix.Backward;
    aVectorReference[Base6Directions.Direction.Left] = myRemoteControl.WorldMatrix.Left; 
    aVectorReference[Base6Directions.Direction.Right] = myRemoteControl.WorldMatrix.Right; 
    aVectorReference[Base6Directions.Direction.Up] = myRemoteControl.WorldMatrix.Up;
    aVectorReference[Base6Directions.Direction.Down] = myRemoteControl.WorldMatrix.Down;

    List<IMyThrust> aThrusterList = new List<IMyThrust>();
    GridTerminalSystem.GetBlocksOfType<IMyThrust>(aThrusterList);

    foreach( IMyThrust aThruster in aThrusterList)
    {
        Vector3D aForward = aThruster.WorldMatrix.Backward;   // thrusters point in the opposite direction

        foreach( KeyValuePair<Base6Directions.Direction,Vector3D> aVectorPair in aVectorReference)
        {
                Base6Directions.Direction aKey  = aVectorPair.Key;
                Vector3D aVector = aVectorPair.Value;
            
                if (aForward.Equals(aVector,0.00001))
                {
                     if (!myThrusters.ContainsKey(aKey)) 
                    { 
                        myThrusters.Add(aKey,new List<IMyThrust>()); 
                    }         
                    myThrusters[aKey].Add(aThruster);  

                    // DEBUG: 
                    Echo(aKey.ToString() + " " + aThruster.CustomName); 
                    // DEBUG: end
                    break;
                }
        }
    } 
    

    // TODO: the panel handling should be done with the CustomData as well
    myPanel = GridTerminalSystem.GetBlockWithName(THRUST_PANEL_NAME) as IMyTextPanel;


} 
 
public void Save() {} 
 
public void Main(string argument, UpdateType updateSource) 
{ 
    mTick++;

    // DEBUG:
    foreach(KeyValuePair<Base6Directions.Direction,List<IMyThrust>> aPair in myThrusters)
    {
        Echo("Update["+mTick+"]  "+updateSource.ToString() + " " + aPair.Key.ToString() + " : " + aPair.Value.Count);
    }
    // DEBUG: end

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

    foreach(KeyValuePair<Base6Directions.Direction,List<IMyThrust>> aPair in myThrusters) 
    {
        Base6Directions.Direction aKey = aPair.Key;
        List<IMyThrust> aValue = aPair.Value;

        double aDirectionForce = 0;
        double aMaxDirectionForce = 0;
        double aEffectiveDirectionForce = 0;
        foreach(IMyThrust aThruster in aValue)
        {
            double aForce = aThruster.CurrentThrust;
            double aMaxForce = aThruster.MaxThrust;
            aDirectionForce += aForce;
            aMaxDirectionForce += aMaxForce;
            aEffectiveDirectionForce += aThruster.MaxEffectiveThrust;
        }
        double aAcceleration = aDirectionForce/aMass.TotalMass;
        double aEfficiency = aDirectionForce/aMaxDirectionForce;
        double aMaxDirectionAcceleration = aEffectiveDirectionForce/aMass.TotalMass;
        //aOutput = aOutput + String.Format("  {0}|{1}|{2} [{3}] \n",
        //            aAcceleration.ToString("0.000"),aDirectionForce.ToString("000000"),aKey.ToString()[0],aValue.Count.ToString("00"));
        aOutput = aOutput + String.Format("  {0}|{1}|{2}|{3} [{4}] \n", 
                    aAcceleration.ToString("00.00"),aEfficiency.ToString("00.00"),aMaxDirectionAcceleration.ToString("00.00"),aKey.ToString()[0],aValue.Count.ToString("00")); 
           
    }

    aOutput = aOutput + aIndicator + "\n";

    myPanel.WritePublicText(aOutput,false);
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
        myTool = GetFirstBlockOfType(myTool);
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
