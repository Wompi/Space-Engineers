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
 *      v0.21 - the usual bugfixes for a blind commit
 *      v0.22 - a little bit shuffling for the display function arguments can be 'ForceMax,ForceEffective,ForceCurrent'
 *      v0.30 - lets see if we can calculate the local velocities and the current break distances - I highly doubt it :)
 */

public CGI_ThrustManager myThrustManager = new CGI_ThrustManager();

public List<IMyShipController> myShipControllers = new List<IMyShipController>();
public List<IMyTextPanel> myLCDPanels = new List<IMyTextPanel>();

public Program()
{
    Runtime.UpdateFrequency  = UpdateFrequency.Update10;
    GridTerminalSystem.GetBlocksOfType(myShipControllers, b => b.CubeGrid == Me.CubeGrid);
    GridTerminalSystem.GetBlocksOfType(myLCDPanels, b => b.CubeGrid == Me.CubeGrid);
    myThrustManager.LoadEntities(GridTerminalSystem, b => b.CubeGrid == Me.CubeGrid);
}

public void Save() {}

public void Main(string argument, UpdateType updateSource)
{
    string aOut = "";

    bool isThrustDirectionSet = myThrustManager.SetDirections();
    if (isThrustDirectionSet)
    {
        IMyShipController aCurrentController = GetControlledController();
        if (aCurrentController != null)
        {
            MyShipMass aMass = aCurrentController.CalculateShipMass();
            Vector3D aGravity = aCurrentController.GetNaturalGravity();
            double aGravityForce = aGravity.Length() * aMass.PhysicalMass;

            myThrustManager.ProcessCalculations(aCurrentController);

            aOut += String.Format("{0}: \n Base: {1}\n  Total: {2}\n Physical: {3}\n Gravity: {4}\n GForce: {5}\n\n",
                    aCurrentController.CustomName,
                    aMass.BaseMass,
                    aMass.TotalMass,
                    aMass.PhysicalMass,
                    aGravity.Length().ToString("0.000"),
                    aGravityForce.ToString("0000000"));

            if (!argument.Equals(String.Empty))
            {
                myThrustManager.ProcessCommand(argument);
            }
            aOut += myThrustManager.Statistics(argument);
        }
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

    private Dictionary<string, Func<string,bool>> mArguments = new Dictionary<string, Func<string,bool>>();
    private Func<string,bool> mArgumentFunction = null;
    private string mStatisticsString = "";

    private Vector3D mLocalVelocity = Vector3D.Zero;
    private Vector3D mLocalBreakDistance = Vector3D.Zero;

    public CGI_ThrustManager()
    {
        mArguments["ForceMax"] = CommandForceMax;
        mArguments["ForceCurrent"] = CommandForceCurrent;
        mArguments["ForceEffective"] = CommandForceEffective;
        mArguments["LocalVelocities"] = CommandLoacalVelocities;
    }

    public string LoadEntities(IMyGridTerminalSystem pGTS, Func<IMyTerminalBlock,bool> pCheck = null)
    {
        string aOut = "";
        pGTS.GetBlocksOfType(mThrusters, pCheck);
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

    public void ProcessCalculations(IMyShipController pController)
    {
        mDirectionStatsList = new List<CGI_ThrusterDirectionStats>();
        double aPhysicalMass = pController.CalculateShipMass().PhysicalMass;

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

            aDirectionStats.mAccelerationMax = aDirectionStats.mDirectionForceMax / aPhysicalMass;
            aDirectionStats.mAccelerationEffective = aDirectionStats.mDirectionForceEffective / aPhysicalMass;
            aDirectionStats.mAccelerationCurrent = aDirectionStats.mDirectionForceCurrent / aPhysicalMass;

            aDirectionStats.mEfficiency = aDirectionStats.mDirectionForceCurrent / aDirectionStats.mDirectionForceMax;

            mDirectionStatsList.Add(aDirectionStats);
        }

        ProcessVelocities(pController);

    }

    public string Statistics(string pArgument)
    {
        if (mArgumentFunction != null)
        {
            mArgumentFunction(pArgument);
        }
        return mStatisticsString;
    }

    public bool ProcessCommand(string pArgument)
    {
        bool aResult = false;
        Func<string, bool> aArgumentFunction =  mArguments[pArgument];
        if (aArgumentFunction != null)
        {
            mArgumentFunction = aArgumentFunction;
            aResult = true;
        }
        return aResult;
    }

    private bool ProcessVelocities(IMyShipController pController)
    {
        Vector3D aLinearVelocity = pController.GetShipVelocities().LinearVelocity;
        double aSpeed = aLinearVelocity.Length();

        Vector3D aUp = pController.WorldMatrix.Up;
        Vector3D aLeft = pController.WorldMatrix.Left;
        Vector3D aForward = pController.WorldMatrix.Forward;

        mLocalVelocity.X = aLinearVelocity.Dot(aUp);        // UP
        mLocalVelocity.Y = aLinearVelocity.Dot(aForward);   // FORWARD
        mLocalVelocity.Z = aLinearVelocity.Dot(aLeft);      // LEFT

        // TODO: holly mother of programming fix this ASAP this is horrible style and very bad design
        // well - it has to do for now to see if it works
        // also - the direction checks are opposite - beware of that

        foreach(CGI_ThrusterDirectionStats aStats in mDirectionStatsList)
        {
            if (mLocalVelocity.X > 0)
            {
                if (aStats.mDirection == Base6Directions.Direction.Up)
                {
                    mLocalBreakDistance.X = -(mLocalVelocity.X*mLocalVelocity.X) / (2 * -aStats.mAccelerationEffective);
                }
            }
            else if (mLocalVelocity.X < 0)
            {
                if (aStats.mDirection == Base6Directions.Direction.Down)
                {
                    mLocalBreakDistance.X = -(mLocalVelocity.X*mLocalVelocity.X)  / (2 * -aStats.mAccelerationEffective);
                }
            }

            if (mLocalVelocity.Y > 0)
            {
                if (aStats.mDirection == Base6Directions.Direction.Forward)
                {
                    mLocalBreakDistance.Y = -(mLocalVelocity.Y*mLocalVelocity.Y)  / (2 * -aStats.mAccelerationEffective);
                }
            }
            else if (mLocalVelocity.Y < 0)
            {
                if (aStats.mDirection == Base6Directions.Direction.Backward)
                {
                    mLocalBreakDistance.Y = -(mLocalVelocity.Y*mLocalVelocity.Y)  / (2 * -aStats.mAccelerationEffective);
                }
            }

            if (mLocalVelocity.Z > 0)
            {
                if (aStats.mDirection == Base6Directions.Direction.Left)
                {
                    mLocalBreakDistance.Z = -(mLocalVelocity.Z*mLocalVelocity.Z)  / (2 * -aStats.mAccelerationEffective);
                }
            }
            else if (mLocalVelocity.Z < 0)
            {
                if (aStats.mDirection == Base6Directions.Direction.Right)
                {
                    mLocalBreakDistance.Z = -(mLocalVelocity.Z*mLocalVelocity.Z)  / (2 * -aStats.mAccelerationEffective);
                }
            }

        }
        return true;
    }


    private bool CommandForceMax(string pCommand)
    {
        mStatisticsString = " Max Force: \n";
        foreach (CGI_ThrusterDirectionStats aStats in mDirectionStatsList)
        {
            mStatisticsString += String.Format("  {0}|{1}|{2}|[{3}][{4}]\n",
                aStats.mAccelerationMax.ToString("000"),
                aStats.mDirectionForceMax.ToString("0000000"),
                aStats.mEfficiency.ToString("0.00"),
                aStats.mDirection.ToString()[0],
                aStats.mThrusters.ToString("00"));
        }
        return true;
    }

    private bool CommandForceCurrent(string pCommand)
    {
        mStatisticsString = " Current Force: \n";
        foreach (CGI_ThrusterDirectionStats aStats in mDirectionStatsList)
        {
            mStatisticsString += String.Format("  {0}|{1}|{2}|[{3}][{4}]\n",
                aStats.mAccelerationCurrent.ToString("000"),
                aStats.mDirectionForceCurrent.ToString("0000000"),
                aStats.mEfficiency.ToString("0.00"),
                aStats.mDirection.ToString()[0],
                aStats.mThrusters.ToString("00"));
        }
        return true;
    }

    private bool CommandForceEffective(string pCommand)
    {
        mStatisticsString = " Effective Force: \n";
        foreach (CGI_ThrusterDirectionStats aStats in mDirectionStatsList)
        {
            mStatisticsString += String.Format("  {0}|{1}|{2}|[{3}][{4}]\n",
                aStats.mAccelerationEffective.ToString("000"),
                aStats.mDirectionForceEffective.ToString("0000000"),
                aStats.mEfficiency.ToString("0.00"),
                aStats.mDirection.ToString()[0],
                aStats.mThrusters.ToString("00"));
        }
        return true;
    }

    private bool CommandLoacalVelocities(string pCommand)
    {
        mStatisticsString = " Local Velocities / Break Distance: \n";

        //mStatisticsString += String.Format("Speed: {0}\n [F] {1:000.00} / {2:000.00}\n [L] {3:000.00} / {4:000.00}\n [U] {5:000.00} / {6:000.00}\n",
        //            mLocalVelocity.Length(),
         //           mLocalVelocity.Y,
        //            mLocalBreakDistance.Y,
        //            mLocalVelocity.Z,
         //           mLocalBreakDistance.Z,
         //           mLocalVelocity.X,
          //          mLocalBreakDistance.X);


   mStatisticsString += String.Format("Speed: {0}\n [F] {1:000.00}\n [L] {2:000.00}\n [U] {3:000.00}\n", 
                    mLocalVelocity.Length(), 
                    mLocalBreakDistance.Y, 
                    mLocalBreakDistance.Z, 
                    mLocalBreakDistance.X);
        return true;
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
