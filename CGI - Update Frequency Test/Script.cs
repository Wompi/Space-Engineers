IMyTextPanel mPanel = null;
IMyMotorStator mRotor = null;

public Program()
{
    Runtime.UpdateFrequency = UpdateFrequency.Update1;
    mPanel = GridTerminalSystem.GetBlockWithName("Debug") as IMyTextPanel;
    mRotor = GridTerminalSystem.GetBlockWithName("Rotor") as IMyMotorStator;
}

TimeSpan mLast = new TimeSpan();
float mLastAngle = 0;

public void Main(string argument, UpdateType updateSource)
{
    TimeSpan aNow = Runtime.TimeSinceLastRun; 

    float aAngle = mRotor.Angle;
    double aDelta = (aAngle - mLastAngle) * 180.0f / Math.PI;

    if (!mLast.Equals(TimeSpan.Zero))
    {
        mPanel.WritePublicText(String.Format("{0}\n{1}\n{2}\n",aNow.TotalMilliseconds,aNow.Ticks,aDelta),false);
    }

    mLast = aNow;
    mLastAngle = aAngle;
}
