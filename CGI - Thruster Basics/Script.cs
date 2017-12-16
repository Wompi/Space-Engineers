/**
 *    Author: Casegard
 *    Program: CGI - Thruster Basics
 *
 *    Version:
 *      v0.10 - just the very basic thruster data, forces, classify the ship thrusters by the main direction
 *      v0.11 - Classify all thrusters by its direction compared to the remote control - sum all forces and calculate the accelleration
 *      v0.12 - forces are now calculated as EffectiveForce as well - accounts for gravity related thrust adjustments
 *      v0.13 - changed the remote controller interface to shipController
 *      v0.20 - change to a manager based design to make it a bit more usable
 *      V0.21 - the usual bugfixes for a blind commit
 */

public CGI_ThrustManager myThrustManager = new CGI_ThrustManager();

public List<IMyShipController> myShipControllers = new List<IMyShipController>();
public List<IMyTextPanel> myLCDPanels = new List<IMyTextPanel>();

public Program()
{
    Runtime.UpdateFrequency  = UpdateFrequency.Update10;
    GridTerminalSystem.GetBlocksOfType(myShipControllers);
    GridTerminalSystem.GetBlocksOfType(myLCDPanels);
    myThrustManager.LoadEntities(GridTerminalSystem);
}

public void Save() {}

public void Main(string argument, UpdateType updateSource)
{
    string aOut = "";




    bool isThrustDirectionSet = myThrustManager.SetDirections();
    if (isThrustDirectionSet)
    {
        IMyShipController aCurrentController = GetControlledController();
        MyShipMass aMass = aCurrentController.CalculateShipMass();
        Vector3D aGravity = aCurrentController.GetNaturalGravity();
        myThrustManager.ProcessCalculations(aCurrentController.CalculateShipMass().PhysicalMass);

        aOut = aOut + String.Format("{0}: \n Base: {1}\n  Total: {2}\n Physical: {3}\n Gravity: {4} \n\n",
                    aCurrentController.CustomName,aMass.BaseMass,aMass.TotalMass,aMass.PhysicalMass,aGravity.Length().ToString("0.000"));

        aOut = aOut + myThrustManager.Statistics("CurrentForce");
    }

    myLCDPanels[1].WritePublicText(aOut,false);
}

public IMyShipController GetControlledController()
{
    IMyShipController aResult = null;
    foreach( IMyShipController aController in myShipControllers)
    {
        if (aController.IsUnderControl)
        {
            aResult = aController;
            break;
        }
    }
    return aResult;
}


public class CGI_ThrustManager
{
    private List<IMyThrust> mThrusters = new List<IMyThrust>();
    private Dictionary<Base6Directions.Direction,List<IMyThrust>> myThrustDirections = new Dictionary<Base6Directions.Direction,List<IMyThrust>>();
    private List<CGI_ThrusterDirectionStats> mDirectionStatsList = null;


    public string LoadEntities(IMyGridTerminalSystem pGTS)
    {
        string aOut = "";
        pGTS.GetBlocksOfType(mThrusters);
        return aOut;
    }

    // TODO: this could use a cache check - so that the calculations not run every cycle
    // because the thrusters don't change the directions
    public bool SetDirections()
    {
        bool aResult = false;

        myThrustDirections[Base6Directions.Direction.Forward] = new List<IMyThrust>();
        myThrustDirections[Base6Directions.Direction.Backward] = new List<IMyThrust>();
        myThrustDirections[Base6Directions.Direction.Left] = new List<IMyThrust>();
        myThrustDirections[Base6Directions.Direction.Right] = new List<IMyThrust>();
        myThrustDirections[Base6Directions.Direction.Up] = new List<IMyThrust>();
        myThrustDirections[Base6Directions.Direction.Down] = new List<IMyThrust>();

        foreach( IMyThrust aThruster in mThrusters)
        {
            Vector3D aThrustDirection = aThruster.GridThrustDirection;
            myThrustDirections[Base6Directions.GetDirection(aThrustDirection)].Add(aThruster);
        }

        // Note: not sure if this is a good test but for now it will do
        // When the ship is not under control by a player or a remote control the 'GridThrustDirection' for
        // all thrusters is always 'Forward'
        // TODO: this will not work for thrusters not mounted on the main grid - a thruster on a rotor will
        // not give any usefull results I guess
        if (myThrustDirections[Base6Directions.Direction.Forward].Count != mThrusters.Count)
        {
            aResult = true;
        }
        return aResult;
    }

    public void ProcessCalculations(double pPhysicalMass)
    {
        mDirectionStatsList = new List<CGI_ThrusterDirectionStats>();

        foreach(KeyValuePair<Base6Directions.Direction,List<IMyThrust>> aPair in myThrustDirections)
        {
            CGI_ThrusterDirectionStats aDirectionStats = new CGI_ThrusterDirectionStats();

            Base6Directions.Direction aKey = aPair.Key;
            List<IMyThrust> aValue = aPair.Value;

            aDirectionStats.mDirection = aKey;

            foreach(IMyThrust aThruster in aValue)
            {
                if (aThruster.Enabled && aThruster.IsFunctional)
                {
                    aDirectionStats.mDirectionForceCurrent += aThruster.CurrentThrust;
                    aDirectionStats.mDirectionForceEffective += aThruster.MaxEffectiveThrust;
                    aDirectionStats.mDirectionForceMax += aThruster.MaxThrust;
                    aDirectionStats.mThrusters += 1;

                }
            }

            aDirectionStats.mAccelerationMax = aDirectionStats.mDirectionForceMax / pPhysicalMass;
            aDirectionStats.mAccelerationEffective = aDirectionStats.mDirectionForceEffective / pPhysicalMass;
            aDirectionStats.mAccelerationCurrent = aDirectionStats.mDirectionForceCurrent / pPhysicalMass;

            aDirectionStats.mEfficiency = aDirectionStats.mDirectionForceCurrent / aDirectionStats.mDirectionForceMax;

            mDirectionStatsList.Add(aDirectionStats);
        }
    }

    public string Statistics(string pArgument)
    {
        string aOut = "";
        if (pArgument.Equals("CurrentForce"))
        {
            aOut = aOut + " Current Force: \n";
            foreach (CGI_ThrusterDirectionStats aStats in mDirectionStatsList)
            {
                aOut = aOut + String.Format("  {0}|{1}|{2}|[{3}][{4}]\n",
                    aStats.mAccelerationCurrent.ToString("000"),
                    aStats.mDirectionForceCurrent.ToString("0000000"),
                    aStats.mEfficiency.ToString("0.00"),
                    aStats.mDirection.ToString()[0],
                    aStats.mThrusters.ToString("00"));
            }
        }
        return aOut;
    }


    struct CGI_ThrusterDirectionStats
    {
        public Base6Directions.Direction mDirection;
        public int mThrusters;

        public double mDirectionForceMax;
        public double mDirectionForceEffective;
        public double mDirectionForceCurrent;

        public double mAccelerationMax;
        public double mAccelerationEffective;
        public double mAccelerationCurrent;
        public double mEfficiency;
    }

}
