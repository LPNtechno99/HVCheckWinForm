using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Visionscape;
using Visionscape.Communications;
using Visionscape.Devices;
using Visionscape.Steps;
using Visionscape.Display.Image;
using Visionscape.Display.Setup;

namespace Visionscape.Extension
{
    public enum Enum_ConnectionEvent
    {
        DISCOVERED_NEW_CAMERA,
        DISCOVERED_CAMERA,
        CONNECTED_JOB,
        DOWNLOADING_JOB,
        DOWNLOADED_JOB,
        CONNECTED_REPORT,
        RECEIVED_REPORT,
        RECEIVED_IMAGE,
        CONNECTED_IO,
        TRIGGERED_IO,
        STATECHANGED_IO,
        DISCONNECTED_DEVICE,
        DISCONNECTED_JOB,
        DISCONNECTED_REPORT,
        DISCONNECTED_IO,
        DISCONNECTED_ALL,
        ERROR
    }

    public struct Event_Progress
    {
        public double progress;
        public string message;
    }

    public class Connection
    {

        public VsCoordinator vsCoordinator;
        public JobStep jobStep;
        public VsDevice vsDevice;
        public ReportConnection allReport;
        public ReportConnection dataReport;
        public ReportConnection imageReport;

        public ReportConnection[] allMultipleReport;

        public IOConnection ioConnection;
        public SetupManager setupManager;

        private string _cameraName;
        private string _jobPath;
        private double _downloadProgress;

        public delegate void ConnectionEvent(Enum_ConnectionEvent e, object obj);
        public event ConnectionEvent ConnectionEventCallback;

        public void ConnectCamera(string cameraName)
        {
            if (_cameraName == cameraName || cameraName == "None")
            {
                return;
            }
            _cameraName = cameraName;

            // If camera already found
            foreach (VsDevice dev in vsCoordinator.Devices)
            {
                if (dev.Name == cameraName && vsDevice != dev)
                {
                    vsDevice = dev;
                    if (ConnectionEventCallback != null)
                    {
                        ConnectionEventCallback.Invoke(Enum_ConnectionEvent.DISCOVERED_CAMERA, dev);
                    }
                    return;
                }
            }
        }
        public void ConnectJob(string jobPath)
        {
            _jobPath = jobPath;


            DisconnectJob();

            if (jobStep == null)
            {
                jobStep = new JobStep();
            }

            jobStep.Load(_jobPath);

            if (ConnectionEventCallback != null)
            {
                ConnectionEventCallback.Invoke(Enum_ConnectionEvent.CONNECTED_JOB, jobStep);
            }
        }
        public void ConnectReport()
        {
            try
            {
                if (allReport == null)
                {
                    allReport = new ReportConnection();
                    allReport.NewReport += receivedReport_NewReport;
                }
                allReport.Connect(vsDevice);

                allReport.DropWhenBusy = false;
                allReport.FreezeMode = ReportConnection.FreezeModeOptions.SHOW_ALL;

                allReport.DataRecordAdd("Snapshot1.BufOut");

                if (ConnectionEventCallback != null)
                {
                    ConnectionEventCallback.Invoke(Enum_ConnectionEvent.CONNECTED_REPORT, allReport);
                }
            }
            catch (Exception ex)
            {
                throw new NotImplementedException(ex.Message);
            }
        }
        public void ConnectMultipleReport()
        {
            IAvpCollection snapshotStepCol = jobStep.FindByType("Step.Snapshot.1");
            for (int i = 0; i < snapshotStepCol.Count; i++)
            {
                if (allMultipleReport == null)
                {
                    allMultipleReport = new ReportConnection[4];
                }
                if (allMultipleReport[i] == null)
                {
                    allMultipleReport[i] = new ReportConnection();
                    allMultipleReport[i].NewReport += receivedReport_NewReport;
                }

                allMultipleReport[i].Connect(vsDevice, i+1);
                allMultipleReport[i].DropWhenBusy = false;
                allMultipleReport[i].FreezeMode = ReportConnection.FreezeModeOptions.SHOW_ALL;

                allMultipleReport[i].DataRecordAdd("Snapshot1.BufOut");

                if (ConnectionEventCallback != null)
                {
                    ConnectionEventCallback.Invoke(Enum_ConnectionEvent.CONNECTED_REPORT, allMultipleReport[i]);
                }
            }
        }

        public void ConnectIO()
        {
            if (ioConnection == null)
            {
                ioConnection = new IOConnection();
                ioConnection.IOTransition += ioConnection_IOTransition;
            }
            ioConnection.Connect(vsDevice);
            
            // STATECHANGED_IO
            if (ConnectionEventCallback != null)
            {
                ConnectionEventCallback.Invoke(Enum_ConnectionEvent.CONNECTED_IO, ioConnection);
            }
        }

        public void DownloadJob()
        {

            if (vsDevice == null || jobStep == null)
            {
                return;
            }

            if (vsDevice.IsAnyInspectionRunning)
            {
                vsDevice.StopAll();
            }

            if (vsDevice.DeviceClass == tagDEVCLASS.DEVCLASS_GIGE_VISION_SYSTEM || vsDevice.DeviceClass == tagDEVCLASS.DEVCLASS_SOFTWARE_EMULATED)
            {
                if (ConnectionEventCallback != null)
                {
                    vsDevice.Download(jobStep, true);

                    Event_Progress ep = new Event_Progress();
                    ep.progress = 1;
                    ep.message = "Download Completed";
                    vsDevice.StartAll();
                    ConnectionEventCallback.Invoke(Enum_ConnectionEvent.DOWNLOADED_JOB, ep);
                }
            } 
            else 
            {
                try
                {
                    _downloadProgress = 0;
                    vsDevice.OnXferProgress += vsDevice_OnXferProgress;
                    vsDevice.OnDownloadComplete += vsDevice_OnDownloadComplete;

                    vsDevice.Download(jobStep, false);
                }
                catch (Exception e)
                {
                    if (ConnectionEventCallback != null)
                    {
                        ConnectionEventCallback.Invoke(Enum_ConnectionEvent.ERROR, e.Message);
                    }
                }

            }
        }

        public void ConnectSetup(Step rootStep) {
            if (vsDevice == null || jobStep == null)
            {
                if (ConnectionEventCallback != null)
                {
                    ConnectionEventCallback.Invoke(Enum_ConnectionEvent.ERROR, "ERROR: Device or Job not found!");
                }
                return;
            }
            if (setupManager == null)
            {
                setupManager = new SetupManager();
            }

            if (vsDevice.IsAnyInspectionRunning)
            {
                vsDevice.StopAll();
            }

            if (rootStep == null)
            {
                IAvpCollection inspStepCol = jobStep.FindByType("Step.Inspection.1");
                setupManager.RootStep = inspStepCol[1] as Step;
            }
            else 
            {
                setupManager.RootStep = rootStep;
            }
            
            setupManager.OptionLayoutSet(SetupManagerLayoutOptions.ShowEverything, true);
            setupManager.OptionLayoutSet(SetupManagerLayoutOptions.ShowDatumGrid, false);
            setupManager.OptionLayoutSet(SetupManagerLayoutOptions.ShowStepTree, false);
            setupManager.ZoomAuto();
            setupManager.Acquire();


        }
        public void ConnectLive(bool startLive) {
            if (startLive)
            {
                IAvpCollection inspStepCol = jobStep.FindByType("Step.Inspection.1");
                Step rootStep = inspStepCol[1] as Step;
                ConnectSetup(rootStep);
                setupManager.OptionLayoutSet(SetupManagerLayoutOptions.ShowEverything, false);
                setupManager.OptionLayoutSet(SetupManagerLayoutOptions.ShowView, true);
                setupManager.ZoomAuto();
                setupManager.LiveVideoStart();
            }
            else
            {
                setupManager.LiveVideoStop();
                setupManager.RootStep = null;
                if (!vsDevice.IsAnyInspectionRunning)
                {
                    vsDevice.StartAll();
                }
            }
        }

        private void vsDevice_OnDownloadComplete(int nStatus)
        {

            if (!vsDevice.IsInspectionRunning())
            {
                vsDevice.StartAll();
            }
            if (ConnectionEventCallback != null)
            {
                Event_Progress ep = new Event_Progress();
                ep.progress = 1;
                ep.message = "Download Completed";
                ConnectionEventCallback.Invoke(Enum_ConnectionEvent.DOWNLOADED_JOB, ep);
            }

            vsDevice.OnXferProgress -= vsDevice_OnXferProgress;
            vsDevice.OnDownloadComplete -= vsDevice_OnDownloadComplete;
        }
        private void vsDevice_OnXferProgress(int nState, int nStatus, string msg)
        {
            /*
                Download Started to VisionHawk1580EB
                FTP Connection established
                Copying files to device...
                Copying _visionhawkcm_4002_752x480_CMOS.cam...
                Job streamed to memory
                RAM Drive Created
                Avp downloaded to Device
                Loading job on device...
                Job Loaded
                Download Complete to VisionHawk1580EB
             */

            _downloadProgress += 0.1;

            if (ConnectionEventCallback != null)
            {
                Event_Progress ep = new Event_Progress();
                ep.progress = _downloadProgress;
                ep.message = msg;
                ConnectionEventCallback.Invoke(Enum_ConnectionEvent.DOWNLOADING_JOB, ep);
            }

        }
        private void receivedReport_NewReport(object sender, ReportConnectionEventArgs e)
        {
            if (ConnectionEventCallback != null)
            {
                if (e.Report.Images.Count > 0)
                {
                    ConnectionEventCallback.Invoke(Enum_ConnectionEvent.RECEIVED_IMAGE, e.Report);
                }
                ConnectionEventCallback.Invoke(Enum_ConnectionEvent.RECEIVED_REPORT, e.Report);
            }
            GC.Collect();
        }
        private void vsCoordinator_OnDeviceDiscovered(VsDevice newDevice)
        {
            // When new camera detected
            if (newDevice.Name == _cameraName && vsDevice != newDevice)
            {
                vsDevice = newDevice;
                if (ConnectionEventCallback != null)
                {
                    ConnectionEventCallback.Invoke(Enum_ConnectionEvent.DISCOVERED_CAMERA, newDevice);
                }
            }
            if (ConnectionEventCallback != null)
            {
                ConnectionEventCallback.Invoke(Enum_ConnectionEvent.DISCOVERED_NEW_CAMERA, newDevice);
            }
        }
        private void ioConnection_IOTransition(object sender, IOConnectionEventArgs e)
        {
            if (ConnectionEventCallback != null)
	        {
                ConnectionEventCallback.Invoke(Enum_ConnectionEvent.STATECHANGED_IO, e);
	        }
        }

        public Connection()
        {
            vsCoordinator = new VsCoordinator();
            vsCoordinator.OnDeviceDiscovered += vsCoordinator_OnDeviceDiscovered;
        }

        public void Trigger(int virtualIO) {
            if (ioConnection == null)
            {
                ioConnection = new IOConnection();
            }
            if (!ioConnection.IsConnected())
            {
                ioConnection.Connect(vsDevice);
            }

            ioConnection.PointWrite(virtualIO, true);
            ioConnection.PointWrite(virtualIO, false);

            if (ConnectionEventCallback != null)
            {
                ConnectionEventCallback.Invoke(Enum_ConnectionEvent.TRIGGERED_IO, virtualIO);
            }
        }
        public void SaveJob() {
            if (jobStep != null && _jobPath.Length > 5)
            {
                jobStep.SaveAll(_jobPath);
            }
        }        

        public void DisconnectAll()
        {
            DisconnectJob();
            DisconnectDevice();
            if (ConnectionEventCallback != null)
            {
                ConnectionEventCallback.Invoke(Enum_ConnectionEvent.DISCONNECTED_ALL, null);
            }
        }
        private void DisconnectDevice() {
            if (vsDevice != null)
            {
                if (vsDevice.IsAnyInspectionRunning)
                {
                    vsDevice.StopAll();
                }
                if (vsDevice.IsConnected)
                {
                    vsDevice.Disconnect();
                }
            }

            vsDevice = null;
            _cameraName = null;

            if (ConnectionEventCallback != null)
            {
                ConnectionEventCallback.Invoke(Enum_ConnectionEvent.DISCONNECTED_DEVICE, null);
            }
        }
        private void DisconnectJob()
        {
            try
            {
                if (vsDevice != null)
                {
                    if (vsDevice.IsAnyInspectionRunning)
                        vsDevice.StopAll();
                }

                if (jobStep != null)
                {
                    while (jobStep.Count > 0)
                    {
                        jobStep.Remove(1);
                    }
                    jobStep = null;
                }
                if (ConnectionEventCallback != null)
                {
                    ConnectionEventCallback.Invoke(Enum_ConnectionEvent.DISCONNECTED_JOB, null);
                }
            }
            catch
            {
                throw new NotImplementedException();
            }
        }
        private void DisconnectReport()
        {
            if (allReport != null)
            {
                allReport.Disconnect();
                allReport.NewReport -= receivedReport_NewReport;
                allReport = null;
            }
            if (allMultipleReport != null)
            {
                foreach (ReportConnection report in allMultipleReport)
                {
                    report.Disconnect();
                    report.NewReport -= receivedReport_NewReport;
                }
                allMultipleReport = null;
            }
            if (ConnectionEventCallback != null)
            {
                ConnectionEventCallback.Invoke(Enum_ConnectionEvent.CONNECTED_REPORT, null);
            }
        }
        private void DisconnectIO()
        {
            if (ioConnection != null)
            {
                ioConnection.Disconnect();
                ioConnection = null;
            }
            if (ConnectionEventCallback != null)
            {
                ConnectionEventCallback.Invoke(Enum_ConnectionEvent.DISCONNECTED_IO, null);
            }
        }

    }
}
