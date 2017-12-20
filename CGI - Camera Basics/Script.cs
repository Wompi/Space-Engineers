/**
 *      Author: Casegard
 *    Program: CGI - Camera Basics
 *
 *    Version:
 *              v0.10 - just the very basic camera data -  see what this RayCast is all about
 *              v0.11  -  no idea
 *              v0.12 - EnableRayCast so we finally can check the raycast  functions
 *              v0.13 - save the results to the camera - todo: split with small/large grids for velocity and position
 *              v0.20 - make it a class with all the functioons nessessary
 *              v0.21 - lets try some menu selection to handle more cameras
 */


public CGI_CameraManager myCameraManager = new CGI_CameraManager();
public List<IMyTextPanel> myLCDPanels = new List<IMyTextPanel>();

public int PANEL_INDEX = 0;

public Program()
{
    Runtime.UpdateFrequency  = UpdateFrequency.Update10;
    string aOut = myCameraManager.LoadEntities(GridTerminalSystem, b => b.CubeGrid == Me.CubeGrid);
    GridTerminalSystem.GetBlocksOfType(myLCDPanels);

    foreach( IMyTextPanel aPanel in myLCDPanels)
    {
        aPanel.FontSize = 1f;
        aPanel.Font = "MonoSpace";
        aPanel.ShowPublicTextOnScreen();
    }
}

public void Save() {}

public void Main(string argument, UpdateType updateSource)
{
    string aOut = "";

    if (!argument.Equals(String.Empty))
    {
        myCameraManager.ProcessCommand(argument);
        aOut += myCameraManager.StatisticsForCurrentCamera();
    }
    else
    {
        aOut += myCameraManager.Statistics();
    }

    aOut = aOut + myCameraManager.GetLastScanResult();
    myLCDPanels[PANEL_INDEX].WritePublicText(aOut,false);
}


public class CGI_CameraManager
{
        private List<IMyCameraBlock> mCameras = new List<IMyCameraBlock>();
        private int mCurrentIndex = 0;
        private Dictionary<string, Func<string,bool>> mArguments = new Dictionary<string, Func<string,bool>>();

        private Dictionary<long,MyDetectedEntityInfo> mScanResults = new Dictionary<long,MyDetectedEntityInfo>();

        double DEFAULT_SCAN_RANGE = 60000;

        public CGI_CameraManager()
        {
            mArguments["scan"] = CommandSingleScan;
            mArguments["NextCamera"] = CommandNext;
            mArguments["PreviousCamera"] = CommandPrevious;

        }

        public string LoadEntities(IMyGridTerminalSystem aGTS, Func<IMyTerminalBlock, bool> pCheck = null)
        {
                string aOut = "";
                aGTS.GetBlocksOfType(mCameras, pCheck);

                // TODO: give every camera a CustomData and set there if you want to be raycast enabled or not
                //             this would make it possible to exclude cameras
                foreach(IMyCameraBlock aCamera in mCameras)
                {
                    aCamera.EnableRaycast = true;
                }
                return aOut;
        }

        public bool ProcessCommand(string pArgument)
        {
            return mArguments[pArgument](pArgument);
        }

        public string StatisticsForCurrentCamera()
        {
            string aResult = "";
            IMyCameraBlock aCamera = mCameras[mCurrentIndex];
            aResult += String.Format("Index: {0:00}/{1:00}\n Name: {2}\n CanScan [{3:0.0} km]: {4}\n ScanRange: {5:0.0} km\n Mass: {6}\n\n",
                mCurrentIndex,
                mCameras.Count,
                aCamera.CustomName,
                DEFAULT_SCAN_RANGE/1000,
                aCamera.CanScan(DEFAULT_SCAN_RANGE) ? 'Y' : 'N',
                aCamera.AvailableScanRange / 1000,
                aCamera.Mass
                );

            return aResult;
        }

        public string Statistics()
        {
            string aOut = "";

            bool aCollect = false;
            double aMass = -1;
            double aDistanceLimit = -1;

            foreach(IMyCameraBlock aCamera in mCameras)
            {
                if (!aCollect)
                {
                    aMass = aCamera.Mass;
                    aDistanceLimit = aCamera.RaycastDistanceLimit;
                }
                string aName = aCamera.CustomName;
                char aCanScan = aCamera.CanScan(DEFAULT_SCAN_RANGE) ? 'Y' : 'N';
                TimeSpan aNewScan = new TimeSpan(aCamera.TimeUntilScan(DEFAULT_SCAN_RANGE));
                double aAvailableScanRange = aCamera.AvailableScanRange / 1000;


                aOut = aOut + String.Format("{0:hh\\:mm\\:ss} - {1} - {2} - {3}\n",aNewScan,aAvailableScanRange.ToString("0000000"),aCanScan,aName);
            }

            string aLimitString = aDistanceLimit == -1 ? "no limit" : aDistanceLimit.ToString("0");
            aOut = aOut + String.Format("\n\n Range: {0}\n Mass: {1}\n Limit: {2}\n\n",DEFAULT_SCAN_RANGE,aMass, aLimitString);

            // if (!mCamera.CustomData.Equals(String.Empty))
            // {
            //     string[] aScans = mCamera.CustomData.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            //     aOut = aOut + String.Format("Camera: has {0} new scan data to copy!",aScans.Length);
            // }

            return aOut;
        }

        public string GetLastScanResult()
        {
            string aOut = "";
            string bOut = "";

            foreach( MyDetectedEntityInfo aScan in mScanResults.Values )
            {
                long aID = aScan.EntityId;
                TimeSpan aTime = new TimeSpan(aScan.TimeStamp);
                string aName = aScan.Name;
                MyDetectedEntityType aType = aScan.Type;
                Vector3D aPos = aScan.Position;
                Vector3D aVelo = aScan.Velocity;
                Nullable<Vector3D> aHitPosition = aScan.HitPosition;

                // TODO: the camera should not thr first one in the list
                double aDistance = Vector3D.Distance(aPos,mCameras[0].GetPosition()) / 1000;

                aOut = aOut + String.Format("  {0:000000} - {1} - {2}\n",aDistance,aType,aName);

                bOut = bOut + String.Format("GPS: Scan Position {0}:{1}:{2}:{3}:\n",aName,aPos.X,aPos.Y,aPos.Z);

                if (aVelo != Vector3D.Zero)
                {
                    aOut = aOut + String.Format(" Velocity: {0:0.00} {1:0.00} {2:0.00}",aVelo.X,aVelo.Y,aVelo.Z);
                }

                if (aHitPosition.HasValue)
                {
                    aOut = aOut + String.Format(" Hit: {0:0.00} {1:0.00} {2:0.00}",aHitPosition.Value.X,aHitPosition.Value.Y,aHitPosition.Value.Z);
                    bOut = bOut + String.Format("GPS: Scan Hit {0}:{1}:{2}:{3}:\n",aName,aHitPosition.Value.X,aHitPosition.Value.Y,aHitPosition.Value.Z);
                }

            }

            return aOut + "\n\n" + bOut;
        }


        private bool CommandSingleScan(string pCommand)
        {
            bool aResult = false;
            IMyCameraBlock aCamera = mCameras[mCurrentIndex];
            if (aCamera.CanScan(DEFAULT_SCAN_RANGE))
            {
                MyDetectedEntityInfo aScan = aCamera.Raycast(DEFAULT_SCAN_RANGE);
                if (!aScan.IsEmpty())
                {
                    long aID = aScan.EntityId;
                    mScanResults[aID] = aScan;
                    aResult = true;
                }
            }
            return aResult;
        }

        private bool CommandNext(string pCommand)
        {
            mCurrentIndex++;
            if (mCurrentIndex > (mCameras.Count-1)) mCurrentIndex = 0;
            return true;
        }

        private bool CommandPrevious(string pCommand)
        {
            mCurrentIndex--;
            if (mCurrentIndex < 0) mCurrentIndex = mCameras.Count-1;
            return true;
        }
}
