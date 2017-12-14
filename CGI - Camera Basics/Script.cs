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
 */


public CGI_CameraManager myCameraManager = new CGI_CameraManager();

public IMyTextPanel mPanel = null;
public static string PANEL_NAME = "CGI - Kowari Panel 02";

public Program()
{
    Runtime.UpdateFrequency  = UpdateFrequency.Update10;
    string aOut = myCameraManager.LoadEntities(GridTerminalSystem);
    mPanel = GridTerminalSystem.GetBlockWithName(PANEL_NAME) as IMyTextPanel;
}

public void Save() {}

public void Main(string argument, UpdateType updateSource)
{
    string aOut = myCameraManager.Statistics();

    if (!argument.Equals(String.Empty))
    {
        myCameraManager.ProcessCommand(argument);
    }

    aOut = aOut + myCameraManager.GetLastScanResult();
    mPanel.WritePublicText(aOut,false);
}


public class CGI_CameraManager
{
        private List<IMyCameraBlock> mCameras = new List<IMyCameraBlock>();
        private Dictionary<string, Func<string,bool>> mArguments = new Dictionary<string, Func<string,bool>>();

        private Dictionary<long,MyDetectedEntityInfo> mScanResults = new Dictionary<long,MyDetectedEntityInfo>();

        double DEFAULT_SCAN_RANGE = 60000;

        public CGI_CameraManager()
        {
            mArguments["scan"] = CommandScan;
        }

        public string LoadEntities(IMyGridTerminalSystem aGTS)
        {
                string aOut = "";
                aGTS.GetBlocksOfType(mCameras);

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

                bOut = bOut + String.Format("GPS: Scan {0}:{1}:{2}:{3}:\n",aName,aPos.X,aPos.Y,aPos.Z);

                if (aVelo != Vector3D.Zero)
                {
                    aOut = aOut + String.Format(" Velocity: {0:0.00} {1:0.00} {2:0.00}",aVelo.Value.X,aVelo.Value.Y,aVelo.Value.Z);
                }

                if (aHitPosition.HasValue)
                {
                    aOut = aOut + String.Format(" Hit: {0:0.00} {1:0.00} {2:0.00}",aHitPosition.Value.X,aHitPosition.Value.Y,aHitPosition.Value.Z);
                }

            }

            return aOut + "\n\n" + bOut;
        }


        public bool CommandScan(string pCommand)
        {
            bool aResult = false;
            foreach (IMyCameraBlock aCamera in mCameras)
            {
                if (aCamera.CanScan(DEFAULT_SCAN_RANGE))
                {
                    MyDetectedEntityInfo aScan = aCamera.Raycast(DEFAULT_SCAN_RANGE);
                    if (!aScan.IsEmpty())
                    {
                        long aID = aScan.EntityId;
                        mScanResults[aID] = aScan;
                    }
                }
            }
            return aResult;
        }
}
