List<IMyGasTank> myTanks = new List<IMyGasTank>();

IMyTextPanel myPanel = null;


static string PANEL_NAME = "CGI - Kangeroo Panel 01";

public Program()
{
    Runtime.UpdateFrequency = UpdateFrequency.Update100;
    GridTerminalSystem.GetBlocksOfType(myTanks,  b => b.CubeGrid == Me.CubeGrid);

    myPanel = GridTerminalSystem.GetBlockWithName(PANEL_NAME) as IMyTextPanel;
}

public void Save()
{
  
}

public void Main(string argument, UpdateType updateSource)
{
    string aOut = "";

    aOut = aOut + String.Format("Tanks: {0} \n\n",myTanks.Count);

    double aCapacity = 0;
    double aFillRatio = 0;
    foreach(IMyGasTank aTank in myTanks)
    {
        aCapacity += aTank.Capacity;
        aOut = aOut + String.Format("Fill: {0} \n",aTank.FilledRatio);
        aFillRatio += aTank.FilledRatio * aTank.Capacity;
    }

    aOut = aOut + String.Format("TotalCpacity: {0} \n Filled: {1} \n Ratio: {2}",aCapacity,aFillRatio,aFillRatio/aCapacity);


    myPanel.WritePublicText(aOut,false);
}
