/**
 *    Program: CGI - Echidna Control
 *    Version: v0.2
 *
 */

List<IMyShipDrill> mDrills = new List<IMyShipDrill>();
List<IMyThrust> myThrusters = new List<IMyThrust>();
IMyTextPanel mPanelLeft = null;
IMyTextPanel mPanelRight = null; 
IMyShipController mShipController = null;
List<IMyGyro> mGyros = new List<IMyGyro>();


string mLog = "";
int mTick = 0;

string PANEL_LEFT_NAME = "CGI - Echidna Panel Left";
string PANEL_RIGHT_NAME = "CGI - Echidna Panel Right";
string SHIP_CONTROLLER_NAME = "CGI - Echidna Cockpit";
//string SHIP_CONTROLLER_NAME = "CGI - Echidna Remote Control";

public Program() 
{ 
   GridTerminalSystem.GetBlocksOfType<IMyShipDrill>(mDrills);
   GridTerminalSystem.GetBlocksOfType<IMyThrust>(myThrusters); 
   mPanelLeft = GridTerminalSystem.GetBlockWithName(PANEL_LEFT_NAME) as IMyTextPanel;
   mPanelRight = GridTerminalSystem.GetBlockWithName(PANEL_RIGHT_NAME) as IMyTextPanel; 
   mShipController = GridTerminalSystem.GetBlockWithName(SHIP_CONTROLLER_NAME) as IMyShipController;
   GridTerminalSystem.GetBlocksOfType<IMyGyro>(mGyros);
} 
 
public void Save() 
{
    Storage = Storage + "\n["+mTick.ToString()+"] " + mLog;
} 
 
public void Main(string argument) 
{ 
    mTick++;
    // TEST STUFF
    // GetMyGridDamage();
    //return;
    // END

    if (mPanelLeft == null) 
    {
        Echo("No Left Panel Found!");
        return;
    }
    if (mPanelRight == null)  
    { 
        Echo("No Right Panel Found!"); 
        return; 
    }

   // Echo("Drills: "+mDrills.Count);

    string aOutput = "";
    
    foreach ( IMyShipDrill aDrill in mDrills)
    {
        IMyInventory aInventory = aDrill.GetInventory(0);
        float aFillPercent = (float) aInventory.CurrentVolume / (float) aInventory.MaxVolume;
       // Echo(aFillPercent.ToString()); 
       

      // aOutput = aOutput + "  " + aDrill.DisplayNameText +" "+aFillPercent+ "%\n" + aInventory.MaxVolume + "/" + aCurrentVolume + "\n\n";
      aOutput = aOutput +"  "+ aDrill.DisplayNameText +" "+aFillPercent+ "%\n" + GetFillGauge(aFillPercent) + "\n\n"; 
    }    
    mPanelLeft.WritePublicText(aOutput);


    // NOTE: gather some shipcontroller statistics to get a general feel for the math behind it
    string aStatistic = "";
    MyShipMass aMass = mShipController.CalculateShipMass();
    aStatistic = aStatistic + String.Format("Mass Base: {0} Total: {1} \n",aMass.BaseMass,aMass.TotalMass);

    Vector3D aPlanetPosition = new Vector3D(0,0,0);
    if (mShipController.TryGetPlanetPosition(out aPlanetPosition))
    {
          aStatistic = aStatistic + String.Format("PlanetPosition: {0} \n",aPlanetPosition.ToString());
    }
    else
    {
        aStatistic = aStatistic + "PlanetPosition: unknown \n";
    }

    double aElevation = 0;
    if (mShipController.TryGetPlanetElevation(MyPlanetElevation.Surface, out aElevation))
    {
        aStatistic = aStatistic + String.Format("PlanetElevation: {0} (survace) \n",aElevation.ToString("0.0"));
    }
    else
    {
        aStatistic = aStatistic + "PlanetElevation: unknown \n";
    }


    Vector3D aNaturalGravity = mShipController.GetNaturalGravity();
    double aLength = aNaturalGravity.Normalize();    // the same as the original length aka the gravity value in [m/s^2] (8.8 on mars)
    aStatistic = aStatistic + 
                String.Format("NaturalGravity: {0} G: {1}  \n", 
                                aNaturalGravity.ToString("0.00"),
                                aLength.ToString("0.00"));
                       

    // NOTE: this is very interesting for control of a VTOL style approach
    //  Directions for Echidna:      
    //                                              Z = -1 (forward)      1 (backward)
    //                                              X = -1 (left)              1 (right)
    //                                              Y = -1 (down)               1 (up)

    Vector3D aMoveIndicator = mShipController.MoveIndicator;
    aStatistic = aStatistic + String.Format("MoveIndicator: {0} \n",aMoveIndicator.ToString("0.00"));

    // NOTE: also very intersting for a manual control
    // Directions for Echidna: (key input - mouse input varies to the current mouse position)
    //                                            x (zenit) =     9 (nose up)                     -9 (nose down)  
    //                                            y (azimut) =  -9 (counter clockwise)    9 (clockwise)
    // TODO: figure out what the values mean - angle? inertia?
 
    Vector2 aRotationIndicator = mShipController.RotationIndicator;
    aStatistic = aStatistic + String.Format("RotationIndicator: x={0} y={1} \n",aRotationIndicator.X,aRotationIndicator.Y); 

    float aRollIndicator = mShipController.RollIndicator;
    aStatistic = aStatistic + String.Format("RollIndicator: {0} \n",aRollIndicator);


    // NOTE: lets have at the gyros
    foreach (IMyGyro aGyro in mGyros)
    {
        aStatistic = aStatistic + String.Format(" Y={0} P={1} R={2} \n",
                        aGyro.Yaw.ToString("0.00"),
                        aGyro.Pitch.ToString("0.00"),
                        aGyro.Roll.ToString("0.00"));
    }


    mPanelRight.WritePublicText(aStatistic);
    Echo("Tick: "+mTick.ToString());

    // Debug/Test thingemajiggies
    LogThrusters(myThrusters); 


    // END: Debug
}


public string GetFillGauge(double aPercent)
{
    
    int aLength = 60;
    int aIndicator = (int) (aPercent * aLength);
    
    //Echo("Derp" + aIndicator+" "+aPercent);
    string aResult = "  [ " + new string('!',aIndicator) + new string('.',aLength-aIndicator)+"]";
    //Echo("Derp" + aResult);
    return aResult;
}


public void LogThrusters(List<IMyThrust> pThrusters)
{
    if (pThrusters == null) return;
    if (pThrusters.Count == 0) return;

    string aLog = ""; 
    foreach(IMyThrust aThrust in pThrusters) 
    { 
        aLog = aLog + String.Format("{0};{1};{2}\n",aThrust.DisplayNameText,aThrust.MaxThrust,aThrust.CurrentThrust); 
    } 
    Me.CustomData = aLog;

    mLog = aLog;
    Save();
}


/**
/*
public void GetMyGridDamage()
{
        IMyCubeGrid aGrid = Me.CubeGrid;
        
        List<IMySlimBlock> myDerpBlocks = new List<IMySlimBlock>();

        Func<IMySlimBlock, bool> MyUnfinishedBlocks = delegate (IMySlimBlock pBlock) 
        { 
            Dictionary<string,int> aMissingParts = new Dictionary<string,int>();
            pBlock.GetMissingComponents(aMissingParts);
            return aMissingParts.Count > 0;
        };
        aGrid.GetBlocks(myDerpBlocks);


        string aOutput = "";
        aOutput = aOutput + String.Format("Unfinished: {0} \n",myDerpBlocks.Count);
        mPanelRight.WritePublicText(aOutput);
}
**/

