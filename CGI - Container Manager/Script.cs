/**
 *      Author: Casegard
 *      Program: CGI - Container Manager
 *
 *      Version:
 *          v0.10 - just the basic construct for the containers
 */

CGI_ContainerManager mContainerManager = new CGI_ContainerManager();

IMyTextPanel mPanel = null;

static string PANEL_NAME = "CGI - Mars Space Panel 01";
static string CONATINER_TYPE = "LargeBlockLargeContainer";

public Program()
{
    Runtime.UpdateFrequency = UpdateFrequency.Update100;

    mContainerManager.LoadEntities(GridTerminalSystem);

    mPanel = GridTerminalSystem.GetBlockWithName(PANEL_NAME) as IMyTextPanel;
}

public void Save() {}

public void Main(string argument, UpdateType updateSource)
{
    string aOut = mContainerManager.Statistics();

    if (mPanel == null) Echo(aOut);
    else mPanel.WritePublicText(aOut,false);
}

public class CGI_ContainerManager
{
    private List<IMyCargoContainer> mContainers = new List<IMyCargoContainer>();

    public void LoadEntities(IMyGridTerminalSystem pGTS)
    {
        pGTS.GetBlocksOfType(mContainers);
    }    

    public string Statistics()
    {
        string aOut = "";
        
        aOut = aOut + String.Format("Containers: {0}\n",mContainers.Count);

        foreach(IMyCargoContainer aContainer in mContainers)
        {
            if (aContainer.BlockDefinition.SubtypeId.Equals(CONATINER_TYPE))
            {
                 aOut = aOut + String.Format("   ID: {0}\n",aContainer.BlockDefinition);
                 List<IMyInventoryItem> aItemList = aContainer.GetInventory(0).GetItems();

                foreach(IMyInventoryItem aItem in aItemList)
                {
                    VRage.ObjectBuilders.MyObjectBuilder_Base aBase = aItem.Content;
//                    if (aBase is VRage.ObjectBuilders.MyObjectBuilder_Ore)
 //                   {
   //                       Echo("Derp: " + ((VRage.Game.ObjectBuilders.MyObjectBuilder_Ore)aBase).GetMaterialName());
     //               }

                    aOut = aOut + String.Format("Item: {0}\n",aBase.GetType());
                }
    
            }
        }
    


        return aOut;
    }
}
