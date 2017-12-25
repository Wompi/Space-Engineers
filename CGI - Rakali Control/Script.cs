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
List<IMyTextPanel> myTextPanels = new List<IMyTextPanel>();             // The ship can have as many as it wants
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


static double ANTENNA_ENERGY_FACTOR = 0.004;    // NOTE: could be changed in future versions for now the energy input is linear (4W)

public void GetFirstBlockOfType<T>( ref T pBlock, Func<IMyTerminalBlock, bool> pCheck = null ) where T : class
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
    GridTerminalSystem.GetBlocksOfType(myTextPanels, aCheck);
    GridTerminalSystem.GetBlocksOfType(myCodeBlocks, aCheck);
    GridTerminalSystem.GetBlocksOfType(myGattlingGuns, aCheck);
    GridTerminalSystem.GetBlocksOfType(myGyros, aCheck);
    GridTerminalSystem.GetBlocksOfType(myReactors, aCheck);
    GridTerminalSystem.GetBlocksOfType(mySorters, aCheck);


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
    Debug();
}


public void Debug()
{
    Echo("Connector: "+myShipConnector.CustomName);
    Echo("Cockpit: "+myCockpit.CustomName);
    Echo("Remote: "+myRemoteControl.CustomName);
    Echo("Oxygen Generator: "+myGasGenerator.CustomName);
    Echo("Medium Container: "+myContainer.CustomName);
    Echo("Ore Detetector: "+myOreDetector.CustomName);

    Echo("Batteries: "+myBatteries.Count);
    Echo("LCD Panels: "+myTextPanels.Count);
    Echo("Drills: "+myDrills.Count);
    Echo("Ejectors: "+myEjectors.Count);
    Echo("Cameras: "+myCameras.Count);
    Echo("Code Blocks: "+myCodeBlocks.Count);
    Echo("Gyros: "+myGyros.Count);
    Echo("Sorters: "+mySorters.Count);
    Echo("Thrusters: "+myThrusters.Count);
    Echo("Spot Lights: "+mySpotLights.Count);
}
