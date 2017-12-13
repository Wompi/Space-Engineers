public Program() {
    // The constructor, called only once every session and
    // always before any other method is called. Use it to
    // initialize your script. 
    //     
    // The constructor is optional and can be removed if not
    // needed.
}

public void Save() {
    // Called when the program needs to save its state. Use
    // this method to save your state to the Storage field
    // or some other means. 
    // 
    // This method is optional and can be removed if not
    // needed.
}

public void Main(string argument) 
{
    List<IMyShipDrill> myDrills = new List<IMyShipDrill>();
    GridTerminalSystem.GetBlocksOfType<IMyShipDrill>(myDrills);

    List<IMyTextPanel> myScreens = new List<IMyTextPanel>();
    GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(myScreens);


    myScreens[0].WritePublicText(myDrills[0].GetInventory[0],MaxVolume);
    myScreens[1].WritePublicText(myDrills[1].GetInventory[0],MaxVolume); 
    

}
