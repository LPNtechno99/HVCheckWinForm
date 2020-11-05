using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Visionscape.Extension;
using HookKeyboard;
using HVCheck.Models;

namespace HVCheck
{
    public enum CameraState { RUN, STOP }
    public partial class Form1 : Form
    {
        private string job_path = @"C:\Microscan\Vscape\Jobs\";
        private string VisionDevice = "";
        public static LowLevelKeyboardListener _listener;
        frmKiemTraDuLieu frmCheck;

        public static List<DanhSachDaiLy> _listDanhSachDaiLy;
        public static List<DuLieuSanPham> _listDuLieuSanPham;
        public static List<DuLieuSoLuongDat> _listDuLieuSoLuongDat;

        DuLieuSanPham _dulieuSP = new DuLieuSanPham();

        ReceviedDataFromCamera _dataCamera = new ReceviedDataFromCamera();
        private int _soluongPass, _soluongFail;
        private RunMode _mode;
        Connection _cameraMV;
        public Form1()
        {
            InitializeComponent();

            _listDanhSachDaiLy = new List<DanhSachDaiLy>();
            _listDuLieuSanPham = new List<DuLieuSanPham>();
            _listDuLieuSoLuongDat = new List<DuLieuSoLuongDat>();

            _mode = new RunMode();
            _listener = new LowLevelKeyboardListener();
            _listener.OnKeyPressed += _listener_OnKeyPressed;
            _listener.HookKeyboard();
        }

        private void _listener_OnKeyPressed(object sender, KeyPressedArgs e)
        {
            if (e.KeyPressed == System.Windows.Input.Key.F1)
            {
                if (cbbCheDoChay.SelectedIndex == 0)
                {
                    cbbCheDoChay.SelectedIndex = 1;
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            getFileNames();
            //_cameraMV = new Connection();
            //_cameraMV.ConnectionEventCallback += _cameraMV_ConnectionEventCallback;

            cbbCheDoChay.Items.Add(RunMode.mode.NORMAL);
            cbbCheDoChay.Items.Add(RunMode.mode.CHECK);

            cbbCheDoChay.SelectedIndex = 0;

            cbbTenDL.DataSource = SQLite.Instance().LayToanBoBangDanhSachDaiLy()[1];
            
        }

        private void _cameraMV_ConnectionEventCallback(Enum_ConnectionEvent e, object obj)
        {
            switch (e)
            {
                case Enum_ConnectionEvent.DISCOVERED_NEW_CAMERA:
                    this.Invoke(new Action(() =>
                    {
                        Visionscape.Devices.VsDeviceClass dev = obj as Visionscape.Devices.VsDeviceClass;
                        if (dev.Name != null)
                        {
                            VisionDevice = dev.Name;
                        }
                    }));
                    break;
                case Enum_ConnectionEvent.DISCOVERED_CAMERA:
                    _cameraMV.ConnectJob(job_path + cbbCongViec.SelectedItem.ToString());
                    break;
                case Enum_ConnectionEvent.CONNECTED_JOB:
                    _cameraMV.DownloadJob();
                    break;
                case Enum_ConnectionEvent.DOWNLOADING_JOB:
                    break;
                case Enum_ConnectionEvent.DOWNLOADED_JOB:
                    _cameraMV.ConnectIO();
                    _cameraMV.ConnectReport();
                    break;
                case Enum_ConnectionEvent.CONNECTED_REPORT:
                    break;
                case Enum_ConnectionEvent.RECEIVED_REPORT:
                    break;
                case Enum_ConnectionEvent.RECEIVED_IMAGE:
                    Visionscape.Communications.InspectionReport report = obj as Visionscape.Communications.InspectionReport;

                    this.Invoke(new Action(() =>
                    {
                        //Update Image
                        // bufferView1.Buffer = report.Images[0];

                        //Update Tool Data
                        foreach (Visionscape.Communications.InspectionReportValue result in report.Results)
                        {
                            try
                            {
                                if (result.NameSym == "Snapshot1.HiLevelTool1.OCRX1.OutStr")
                                {
                                    _dataCamera.ReceviedString = result.AsString;
                                    lblChuoiNhan.Text = _dataCamera.ReceviedString;
                                }

                            }
                            catch
                            {

                            }
                        }

                        //Pass - Fail Processing
                        if (report.InspectionStats.Passed)
                        {

                        }
                        else
                        {

                        }

                        //Save Image

                    }));
                    break;
                case Enum_ConnectionEvent.CONNECTED_IO:
                    break;
                case Enum_ConnectionEvent.TRIGGERED_IO:
                    break;
                case Enum_ConnectionEvent.STATECHANGED_IO:
                    break;
                case Enum_ConnectionEvent.DISCONNECTED_DEVICE:
                    break;
                case Enum_ConnectionEvent.DISCONNECTED_JOB:
                    break;
                case Enum_ConnectionEvent.DISCONNECTED_REPORT:
                    break;
                case Enum_ConnectionEvent.DISCONNECTED_IO:
                    break;
                case Enum_ConnectionEvent.DISCONNECTED_ALL:
                    break;
                case Enum_ConnectionEvent.ERROR:
                    break;
                default:
                    break;
            }
        }

        private void getFileNames()
        {
            DirectoryInfo di = new DirectoryInfo(job_path);
            FileSystemInfo[] smFiles = di.GetFiles("*.avp");
            int i = 0;
            string fileName = "";
            cbbCongViec.Items.Clear();
            while (i < smFiles.Length)
            {
                fileName = smFiles[i].ToString();
                this.cbbCongViec.Items.Add(fileName.Substring(0, fileName.Length));
                i++;
            }
            cbbCongViec.Text = smFiles[0].ToString().Substring(0, smFiles[0].ToString().Length);
        }

        private void cbbCheDoChay_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbbCheDoChay.SelectedIndex == 0)
            {
                _mode.RunModeCurrent = RunMode.mode.NORMAL;
                lblCheDoChay.Text = "Chế độ chạy " + _mode.RunModeCurrent;
                lblCheDoChay.BackColor = Color.LimeGreen;
                lblF1.BackColor = Color.LimeGreen;
            }
            else if (cbbCheDoChay.SelectedIndex == 1)
            {
                _mode.RunModeCurrent = RunMode.mode.CHECK;
                lblCheDoChay.Text = "Chế độ chạy " + _mode.RunModeCurrent;
                lblCheDoChay.BackColor = Color.DeepSkyBlue;
                lblF1.BackColor = Color.DeepSkyBlue;
                frmCheck = new frmKiemTraDuLieu();
                frmCheck.SuKienThayDoiCheDoChay += FrmCheck_SuKienThayDoiCheDoChay;
                frmCheck.Show();
            }
        }

        private void FrmCheck_SuKienThayDoiCheDoChay()
        {
            cbbCheDoChay.SelectedIndex = 0;
            //cbbCheDoChay_SelectedIndexChanged(null, null);
        }

        bool _flagChayDung;
        private void btnChayDung_Click(object sender, EventArgs e)
        {
            if (!_flagChayDung)
            {
                //_cameraMV.ConnectCamera(VisionDevice);
                btnChayDung.Text = CameraState.STOP.ToString();
                btnChayDung.BackColor = Color.OrangeRed;
                _flagChayDung = true;
            }
            else
            {
                //_cameraMV.DisconnectAll();
                btnChayDung.Text = CameraState.RUN.ToString();
                btnChayDung.BackColor = Color.Green;
                _flagChayDung = false;
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            SQLite.Instance().CheckConnect();
        }

        private void btnLuu_Click(object sender, EventArgs e)
        {

            SQLite.Instance().ThemDaiLy(txtMaDL.Text.Trim(), txtTenDL.Text.Trim());

        }
        private void button6_Click(object sender, EventArgs e)
        {
            _soluongPass++;
            _dataCamera.CounterPASS = _soluongPass;
            _dataCamera.ReceviedString = "NamDepzai";

            lblPassFail.Text = ReceviedDataFromCamera.Result.PASS.ToString();
            lblPassFail.BackColor = Color.Green;
            lblChuoiNhan.Text = _dataCamera.ReceviedString;

            lblCounterPass.Text = _dataCamera.CounterPASS.ToString();
            lblCounter.Text = _dataCamera.CounterPASS.ToString();

            _dulieuSP = SQLite.Instance().LayDuLieuSanPham("12345676");
            frmCheck.lblChuoiNhan.Text = _dulieuSP.MaSP;
            frmCheck.lblMaDL.Text = _dulieuSP.MaDL;
            frmCheck.lblTenDL.Text = _dulieuSP.TenDL;
            frmCheck.lblNgayGioXuat.Text = _dulieuSP.Time;
            if(frmCheck.cbbChonDaiLy.SelectedItem.ToString() == _dulieuSP.TenDL)
            {
                frmCheck.lblKetQua.Text = "KHỚP";
                frmCheck.lblKetQua.BackColor = Color.Green;
            }
            else
            {
                frmCheck.lblKetQua.Text = "KHÔNG KHỚP";
                frmCheck.lblKetQua.BackColor = Color.Red;
            }
        }
        private void button7_Click(object sender, EventArgs e)
        {
            _soluongFail++;
            _dataCamera.CounterFAIL = _soluongFail;
            _dataCamera.ReceviedString = "MarineXinhGai";

            lblPassFail.Text = ReceviedDataFromCamera.Result.FAIL.ToString();
            lblPassFail.BackColor = Color.OrangeRed;
            lblChuoiNhan.Text = _dataCamera.ReceviedString;

            lblCounterFail.Text = _dataCamera.CounterFAIL.ToString();
            lblCounterFail.Text = _dataCamera.CounterFAIL.ToString();
        }
    }
}
