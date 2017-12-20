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

public int PANEL_CAMERA_INDEX = 2;
public int PANEL_SCAN_INDEX = 3;

public Program()
{
    Runtime.UpdateFrequency  = UpdateFrequency.Update10;
    string aOut = myCameraManager.LoadEntities(GridTerminalSystem, b => b.CubeGrid == Me.CubeGrid);
    GridTerminalSystem.GetBlocksOfType(myLCDPanels, b => b.CubeGrid == Me.CubeGrid);

    // Note: This leads to a game grash if another script set this as well
    // foreach( IMyTextPanel aPanel in myLCDPanels)
    // {
    //     aPanel.FontSize = 1f;
    //     aPanel.Font = "MonoSpace";
    //     aPanel.ShowPublicTextOnScreen();
    // }
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
        aOut += myCameraManager.StatisticsForCurrentCamera();
    }

    myLCDPanels[PANEL_SCAN_INDEX].WritePublicText(myCameraManager.GetCurrentScanResult(),false);
    myLCDPanels[PANEL_CAMERA_INDEX].WritePublicText(aOut,false);
}


public class CGI_CameraManager
{
        private List<IMyCameraBlock> mCameras = new List<IMyCameraBlock>();
        private int mCurrentCameraIndex = 0;
        private Dictionary<string, Func<string,bool>> mArguments = new Dictionary<string, Func<string,bool>>();

        private Dictionary<long,MyDetectedEntityInfo> mScanResults = new Dictionary<long,MyDetectedEntityInfo>();
        private List<long> mScanIDs = new List<long>();
        private int mCurrentScanIndex = 0;

        double DEFAULT_SCAN_RANGE = 60000;

        public CGI_CameraManager()
        {
            mArguments["scan"] = CommandSingleScan;
            mArguments["NextCamera"] = CommandNext;
            mArguments["PreviousCamera"] = CommandPrevious;
            mArguments["NextScan"] = CommandNextScan;
            mArguments["PreviousScan"] = CommandPreviousScan;
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
            IMyCameraBlock aCamera = mCameras[mCurrentCameraIndex];
            aResult += String.Format("Camera Index: {0:00}/{1:00}\n Name: {2}\n CanScan [{3:0.0} km]: {4}\n ScanRange: {5:0.0} km\n Mass: {6}\n\n",
                mCurrentCameraIndex+1,
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

        public string GetCurrentScanResult()
        {
            string aResult = "";
            if (mScanIDs.Count != 0)
            {
                long aID = mScanIDs[mCurrentScanIndex];
                MyDetectedEntityInfo aScan = mScanResults[aID];

                TimeSpan aTime = new TimeSpan(aScan.TimeStamp);
                string aName = aScan.Name;
                MyDetectedEntityType aType = aScan.Type;
                Vector3D aPos = aScan.Position;
                Vector3D aVelo = aScan.Velocity;
                Nullable<Vector3D> aHitPosition = aScan.HitPosition;

                // TODO: the camera should not thr first one in the list
                IMyCameraBlock aCamera = mCameras[mCurrentCameraIndex];
                double aDistance = Vector3D.Distance(aPos,aCamera.GetPosition());

                string aVeloString = "Velocity: static";
                if (aVelo != Vector3D.Zero)
                {
                    aVeloString = String.Format("Velocity:\n     {0:00.00}\n     {1:00.00}\n     {2:00.00}",
                        aVelo.X,
                        aVelo.Y,
                        aVelo.Z);
                }

                string aHitString = "HitPosition: undefined";
                if (aHitPosition.HasValue)
                {
                    aHitString = String.Format("HitPosition:\n     {0:0.00}\n     {1:0.00}\n     {2:0.00}",
                        aHitPosition.Value.X,
                        aHitPosition.Value.Y,
                        aHitPosition.Value.Z);
                }

                aResult += String.Format("Scan Index: {0:00}/{1:00}\n ID: {2}\n Name: {3}\n Type: {4}\n Time: {5:hh\\:mm\\:ss}\n Distance: {6:0.0} km\n{7}\n {8}\n",
                    mCurrentScanIndex+1,
                    mScanIDs.Count,
                    aID,
                    aName,
                    aType,
                    aTime,
                    aDistance / 1000,
                    aVeloString,
                    aHitString);

            }
            return aResult;
        }


        private bool CommandSingleScan(string pCommand)
        {
            bool aResult = false;
            IMyCameraBlock aCamera = mCameras[mCurrentCameraIndex];
            if (aCamera.CanScan(DEFAULT_SCAN_RANGE))
            {
                MyDetectedEntityInfo aScan = aCamera.Raycast(DEFAULT_SCAN_RANGE);
                if (!aScan.IsEmpty())
                {
                    long aID = aScan.EntityId;
                    mScanResults[aID] = aScan;
                    mScanIDs = mScanResults.Keys.ToList();
                    mScanIDs.Sort();
                    mCurrentScanIndex = mScanIDs.IndexOf(aID);
                    aResult = true;
                }
            }
            return aResult;
        }

        private bool CommandNext(string pCommand)
        {
            mCurrentCameraIndex++;
            if (mCurrentCameraIndex > (mCameras.Count-1)) mCurrentCameraIndex = 0;
            return true;
        }

        private bool CommandPrevious(string pCommand)
        {
            mCurrentCameraIndex--;
            if (mCurrentCameraIndex < 0) mCurrentCameraIndex = mCameras.Count-1;
            return true;
        }

        private bool CommandNextScan(string pCommand)
        {
            mCurrentScanIndex++;
            if (mCurrentScanIndex > (mScanIDs.Count-1)) mCurrentScanIndex = 0;
            return true;
        }

        private bool CommandPreviousScan(string pCommand)
        {
            mCurrentScanIndex--;
            if (mCurrentScanIndex < 0) mCurrentScanIndex = mScanIDs.Count-1;
            return true;
        }
}
