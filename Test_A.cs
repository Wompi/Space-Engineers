void Main(string argument)
{
    var block = GridTerminalSystem.GetBlockWithName("Beacon");

    // Verbose way:
    block.GetActionWithName("OnOff").Apply(block);

    // Better way:
    block.ApplyAction("OnOff");
}
