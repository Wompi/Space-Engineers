IMyTextPanel mPanel = null;
List<CaseSorterParameters> myCaseSorters = new List<CaseSorterParameters>();


static string BASE_NAME = "CGI - Zeta Mars ";
static string PANEL_NAME =  BASE_NAME +"Text Panel (Debug)";

public Program() 
{ 
    mPanel = GridTerminalSystem.GetBlockWithName(PANEL_NAME) as IMyTextPanel;
    List<IMyConveyorSorter> mySorters = new List<IMyConveyorSorter>();
    GridTerminalSystem.GetBlocksOfType<IMyConveyorSorter>(mySorters);

    foreach (IMyConveyorSorter aSorter in mySorters)
    {
        CaseSorterParameters aSorterParam = ParseCustomData(aSorter.CustomData);
        if (aSorterParam != null)
        {
            aSorterParam.mSorter = aSorter;
            myCaseSorters.Add(aSorterParam);

            Echo(aSorterParam.ToString());
        }
    }
     
} 
 
public void Save() {} 

public void Main(string argument) 
{ 
    // string[] myArguments = argument.Split(':');
    // Echo(myArguments[0]);
    // Echo(myArguments[1]);




    string aOutput = "";

    mPanel.WritePublicText(aOutput);    
} 


public CaseSorterParameters ParseCustomData(string pCustomData)
{
    if (pCustomData.Length == 0)
    {
        //Echo("No CustomData!");
        return null;
    }

    string[] myPairs =  pCustomData.Split(new char[] {'\r','\n'}, StringSplitOptions.RemoveEmptyEntries);
    
    // TODO: this whole approach to the custom data is very rude - because I would not bother to look up 
    // the string parse functions for CSharp right now 
    // It will fail if other sorters have a different custom data declaration - fix this ASAP

    CaseSorterParameters aResult = new CaseSorterParameters();
    string[] aEntityPair = myPairs[0].Split('=');
    aResult.mEntityID =  aEntityPair[1];

    string[] aFlowPair = myPairs[1].Split('='); 
    aResult.mFlow =  aFlowPair[1]; 

    return aResult;
}


public class CaseSorterParameters
{
    public IMyConveyorSorter mSorter { get; set;}
    public string mEntityID  { get; set; }
    public string mFlow { get; set;}

    public override string ToString()
    {
        return String.Format("Entity: {0} Flow: {1} Name: {2}",mEntityID,mFlow,mSorter.DisplayNameText);
    }
}




