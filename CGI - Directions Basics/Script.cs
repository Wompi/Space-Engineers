int mTick = 0;

List<IMyShipController> myShipControllers = new List<IMyShipController>();


public Program() 
{
    GridTerminalSystem.GetBlocksOfType<IMyShipController>(myShipControllers);
} 
 
public void Save() 
{ 
    Echo("Save: "+ mTick.ToString());
} 
 
public void Main(string argument) 
{ 
    mTick++;


    string aCSV = "";
    
    Dictionary<string,Vector3D> myDirections = createDirectionDictionary(Me);
    aCSV = aCSV + createCSVString(myDirections);

    foreach(IMyShipController aControl in myShipControllers)
    {
        Dictionary<string,Vector3D> aDictionary = createDirectionDictionary(aControl); 
        aCSV = aCSV + createCSVString(aDictionary); 
    }

    Me.CustomData = aCSV;
    Echo("Iteration: "+mTick);
    Echo("Note: check custom data!");
} 

public string createCSVString(Dictionary<string,Vector3D> pDictionary)
{
    string aResult = "";
    foreach( var aPair in pDictionary) 
    { 
        string aKey = aPair.Key; 
        Vector3D aValue = aPair.Value; 
 
        aResult = aResult + String.Format("{0};{1};{2};{3} \n",aKey,aValue.X,aValue.Y,aValue.Z); 
    }
    return aResult;
}

public Dictionary<string,Vector3D> createDirectionDictionary( IMyCubeBlock pBlock)
{
    if (pBlock == null) return null;


    Dictionary<string,Vector3D> aResult = new Dictionary<string,Vector3D>();
   
    aResult["Position"] = pBlock.GetPosition();  
    
    aResult["Forward"] = pBlock.WorldMatrix.Forward; 
    aResult["Backward"] = pBlock.WorldMatrix.Backward;  
    aResult["Left"] = pBlock.WorldMatrix.Left; 
    aResult["Right"] = pBlock.WorldMatrix.Right; 
    aResult["Up"] = pBlock.WorldMatrix.Up; 
    aResult["Down"] = pBlock.WorldMatrix.Down;  
   
    aResult["FxU"] = pBlock.WorldMatrix.Forward.Cross(pBlock.WorldMatrix.Up);   
  
    // TODO: put this in external functions for a better structure .. but for now it should be good enough
    if ( pBlock is IMyShipController)
    {
        Vector3D aGravity = ((IMyShipController) pBlock).GetNaturalGravity();
        if (aGravity != null)
        {
            aResult["NaturalGravity"] = aGravity;
        }
    }

    // END: TODO



    return aResult;
}
