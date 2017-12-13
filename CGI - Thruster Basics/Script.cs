/**
 *      Author: Casegard
 *    Program: CGI - Thruster Basics
 *
 *    Version:
 *              v0.10 - just the very basic thruster data, forces, classify the ship thrusters by the main direction
 *              v0.11 - Classify all thrusters by its direction compared to the remote control - sum all forces and calculate the accelleration
 *              v0.12 - forces are now calculated as EffectiveForce as well - accounts for gravity related thrust adjustments
 *              v0.13 - changed the remote controller interface to shipController
 */

Dictionary<Base6Directions.Direction,List<IMyThrust>> myThrusters = new Dictionary<Base6Directions.Direction,List<IMyThrust>>();
IMyShipController  myControl = null;
IMyTextPanel myPanel = null;

int mTick = 0;


static string THRUST_PANEL_NAME = "CGI - Kowari Panel 01";


// TEST: 

public Program() 
{ 
    Runtime.UpdateFrequency  = UpdateFrequency.Update10;
    List<IMyShipController> aControlList = new List<IMyShipController>();
    GridTerminalSystem.GetBlocksOfType<IMyShipController>(aControlList);
    
    if (aControlList.Count == 0) 
    {
        Echo("ERROR: no remote control block!");
        return;
    }
   
    // TODO: this is a bit ugly - it finds the RemoteControl Block but it should get it from the CustomData instead 
    // to account for the right one if you have multiple 
    myControl = aControlList[0];
    Dictionary<Base6Directions.Direction,Vector3D> aVectorReference = new Dictionary<Base6Directions.Direction,Vector3D>();
    aVectorReference[Base6Directions.Direction.Forward] = myControl.WorldMatrix.Forward;
    aVectorReference[Base6Directions.Direction.Backward] = myControl.WorldMatrix.Backward;
    aVectorReference[Base6Directions.Direction.Left] = myControl.WorldMatrix.Left; 
    aVectorReference[Base6Directions.Direction.Right] = myControl.WorldMatrix.Right; 
    aVectorReference[Base6Directions.Direction.Up] = myControl.WorldMatrix.Up;
    aVectorReference[Base6Directions.Direction.Down] = myControl.WorldMatrix.Down;

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


    string aOutput = "";
    MyShipMass aMass = myControl.CalculateShipMass();
    Vector3D aGravity = myControl.GetNaturalGravity();
    aOutput  = aOutput + String.Format("{0}: \n {1} / {2} \n Gravity: {3} \n\n",
                myControl.CustomName,aMass.BaseMass,aMass.TotalMass,aGravity.Length().ToString("0.000"));

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

    myPanel.WritePublicText(aOutput,false);
} 
