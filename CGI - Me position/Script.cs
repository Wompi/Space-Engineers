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
    string aOutput = "";

    string aPosition = Me.GetPosition().ToString();
    Echo(aPosition);
    aOutput = aOutput + aPosition.ToString() + "\n";


    Quaternion aQuaternion = new Quaternion(0,0,0,0);
    Me.Orientation.GetQuaternion(out aQuaternion);
    Echo(aQuaternion.ToString());
    aOutput = aOutput + aQuaternion.ToString() + "\n";


    Me.CustomData = aOutput;; 
} 
