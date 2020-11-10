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
using ActUtlTypeLib;

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
        DanhSachDaiLy _dsDaiLy = new DanhSachDaiLy();

        ReceviedDataFromCamera _dataCamera = new ReceviedDataFromCamera();
        private int _soluongPass, _soluongFail;
        private RunMode _mode;
        Connection _cameraMV;

        private bool _flagDatSoLuong;

        ActUtlType Fx1s = new ActUtlType();
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

            Fx1s.ActLogicalStationNumber = 1;
            Fx1s.Open();
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
            //getFileNames();
            //_cameraMV = new Connection();
            //_cameraMV.ConnectionEventCallback += _cameraMV_ConnectionEventCallback;

            cbbCheDoChay.Items.Add(RunMode.mode.NORMAL);
            cbbCheDoChay.Items.Add(RunMode.mode.CHECK);

            cbbCheDoChay.SelectedIndex = 0;

            cbbTenDL.DataSource = SQLite.Instance().LayToanBoBangDanhSachDaiLy()[1];
            _dsDaiLy = SQLite.Instance().LayDaiLy(cbbTenDL.SelectedItem.ToString());
            dvDSDaiLy.DataSource = SQLite.Instance().TaoBang("SELECT *FROM DanhSachDaiLy");

            txtThuMucLuuAnh.Text = Properties.Settings.Default.PathLuuAnh;

            short data1, data2;
            Fx1s.ReadDeviceBlock2("D1", 1, out data1);
            txtTriggerDelay.Text = data1.ToString();

            Fx1s.ReadDeviceBlock2("D3", 1, out data2);
            txtRejectDelay.Text = data2.ToString();
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
                        bufferView1.Buffer = report.Images[0];
                        //Update Tool Data
                        foreach (Visionscape.Communications.InspectionReportValue result in report.Results)
                        {
                            try
                            {
                                if (result.NameSym == "Snapshot1.OCRX1.OutStr")
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
                            if (_mode.RunModeCurrent == RunMode.mode.NORMAL)
                            {
                                _soluongPass++;
                                _dataCamera.CounterPASS = _soluongPass;

                                lblPassFail.Text = ReceviedDataFromCamera.Result.PASS.ToString();
                                lblPassFail.BackColor = Color.Green;

                                lblCounterPass.Text = _dataCamera.CounterPASS.ToString();
                                lblCounter.Text = _dataCamera.CounterPASS.ToString();

                                SQLite.Instance().ThemDuLieuSanPham(DateTime.Now.ToString("ddMMyyHHmmss"), DateTime.Now.ToString("dd/MM/yyyy"),
                                    _dsDaiLy.MaDL, _dsDaiLy.TenDL, _dataCamera.ReceviedString);

                                if (_soluongPass == int.Parse(numericUpDown1.Value.ToString()))
                                {
                                    if (MessageBox.Show("Đã chạy đủ số lượng", "Thông báo", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                                    {
                                        btnChayDung.PerformClick();
                                    }
                                }
                            }

                            else if (_mode.RunModeCurrent == RunMode.mode.CHECK)
                            {
                                lblPassFail.Text = ReceviedDataFromCamera.Result.PASS.ToString();
                                lblPassFail.BackColor = Color.Green;

                                _dulieuSP = SQLite.Instance().LayDuLieuSanPham(_dataCamera.ReceviedString);
                                frmCheck.lblChuoiNhan.Text = _dulieuSP.MaSP;
                                frmCheck.lblMaDL.Text = _dulieuSP.MaDL;
                                frmCheck.lblTenDL.Text = _dulieuSP.TenDL;
                                frmCheck.lblNgayGioXuat.Text = _dulieuSP.Time;
                                if (frmCheck.cbbChonDaiLy.SelectedItem.ToString() == _dulieuSP.TenDL)
                                {
                                    frmCheck.lblKetQua.Text = "KHỚP";
                                    frmCheck.lblKetQua.BackColor = Color.Green;
                                    frmCheck.lblTenDL.BackColor = Color.SeaGreen;
                                    frmCheck.cbbChonDaiLy.BackColor = Color.SeaGreen;
                                }
                                else
                                {
                                    frmCheck.lblKetQua.Text = "KHÔNG KHỚP";
                                    frmCheck.lblKetQua.BackColor = Color.Red;
                                    frmCheck.lblTenDL.BackColor = Color.Red;
                                    frmCheck.cbbChonDaiLy.BackColor = Color.Red;
                                }
                            }
                        }
                        else
                        {
                            if (_mode.RunModeCurrent == RunMode.mode.NORMAL)
                            {
                                _soluongFail++;
                                _dataCamera.CounterFAIL = _soluongFail;

                                lblPassFail.Text = ReceviedDataFromCamera.Result.FAIL.ToString();
                                lblPassFail.BackColor = Color.OrangeRed;

                                lblCounterFail.Text = _dataCamera.CounterFAIL.ToString();
                                lblCounterFail.Text = _dataCamera.CounterFAIL.ToString();

                                string name = DateTime.Now.ToString("dd/MM/yy-HHmmssfff") + "_F";
                                string imagePath = Properties.Settings.Default.PathLuuAnh + "\\NORMAL\\" + name + ".bmp";
                                bufferView1.Buffer.SaveImage(imagePath, Visionscape.Steps.EnumImgFileType.ftWithGraphics);
                            }
                            else if (_mode.RunModeCurrent == RunMode.mode.CHECK)
                            {
                                lblPassFail.Text = ReceviedDataFromCamera.Result.FAIL.ToString();
                                lblPassFail.BackColor = Color.OrangeRed;

                                string name = DateTime.Now.ToString("dd/MM/yy-HHmmssfff") + "_F";
                                string imagePath = Properties.Settings.Default.PathLuuAnh + "\\CHECK\\" + name + ".bmp";
                                bufferView1.Buffer.SaveImage(imagePath, Visionscape.Steps.EnumImgFileType.ftWithGraphics);
                            }
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
                btnChayDung.PerformClick();
                frmCheck = new frmKiemTraDuLieu();
                frmCheck.SuKienThayDoiCheDoChay += FrmCheck_SuKienThayDoiCheDoChay;
                frmCheck.ShowDialog();
            }
        }

        private void FrmCheck_SuKienThayDoiCheDoChay()
        {
            cbbCheDoChay.SelectedIndex = 0;
            btnChayDung.PerformClick();
            //cbbCheDoChay_SelectedIndexChanged(null, null);
        }

        bool _flagChayDung;
        private void btnChayDung_Click(object sender, EventArgs e)
        {
            if (!_flagChayDung)
            {
                if (_mode.RunModeCurrent == RunMode.mode.NORMAL)
                {
                    if (int.Parse(numericUpDown1.Value.ToString()) == 0 || _flagDatSoLuong == false)
                    {
                        MessageBox.Show("Chưa đặt số lượng");
                        numericUpDown1.Focus();
                        return;
                    }
                }
                _cameraMV.ConnectCamera(VisionDevice);
                btnChayDung.Text = CameraState.STOP.ToString();
                btnChayDung.BackColor = Color.OrangeRed;
                _flagChayDung = true;
                tabControl1.Enabled = false;
                _dsDaiLy = SQLite.Instance().LayDaiLy(cbbTenDL.SelectedItem.ToString());
            }
            else
            {
                _cameraMV.DisconnectAll();
                btnChayDung.Text = CameraState.RUN.ToString();
                btnChayDung.BackColor = Color.Green;
                _flagChayDung = false;
                _soluongPass = 0;
                _soluongFail = 0;
                _flagDatSoLuong = false;
                tabControl1.Enabled = true;
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            SQLite.Instance().CheckConnect();
        }

        private void btnLuu_Click(object sender, EventArgs e)
        {
            if(txtMaDL.Text.Trim() == "" || txtTenDL.Text.Trim()=="")
            {
                MessageBox.Show("Không được để trống");
                return;
            }
            SQLite.Instance().ThemDaiLy(txtMaDL.Text.Trim(), txtTenDL.Text.Trim());
            txtMaDL.Text = "";
            txtTenDL.Text = "";
            txtMaDL.Focus();
            dvDSDaiLy.DataSource = SQLite.Instance().TaoBang("SELECT *FROM DanhSachDaiLy");

        }
        private void btnDatSoLuong_Click(object sender, EventArgs e)
        {
            if(int.Parse(numericUpDown1.Value.ToString())==0)
            {
                MessageBox.Show("Số lượng đặt không thể bằng 0");
                return;
            }
            SQLite.Instance().ThemDuLieuSoLuongDat(DateTime.Now.ToString("ddMMyyHHmmss"), DateTime.Now.ToString("dd/MM/yyyy"),
                _dsDaiLy.MaDL, _dsDaiLy.TenDL, int.Parse(numericUpDown1.Value.ToString()));
            _flagDatSoLuong = true;
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
            if (frmCheck.cbbChonDaiLy.SelectedItem.ToString() == _dulieuSP.TenDL)
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

        private void btnLuuTriggerDelay_Click(object sender, EventArgs e)
        {
            short data = short.Parse(txtTriggerDelay.Text.Trim());
            int result = Fx1s.SetDevice2("D1", data);
            if (result == 0) MessageBox.Show("OK");
        }

        private void btnLuuRejectDelay_Click(object sender, EventArgs e)
        {
            short data = (short)(short.Parse(txtRejectDelay.Text.Trim()) / 10);
            int result = Fx1s.SetDevice2("D3", data);
            if (result == 0) MessageBox.Show("OK");
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Fx1s.Close();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            frmDuLieu fm = new frmDuLieu();
            fm.ShowDialog();
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            int id = int.Parse(dvDSDaiLy.CurrentRow.Cells[0].FormattedValue.ToString());
            SQLite.Instance().XoaTheoID("DELETE FROM DanhSachDaiLy WHERE ID='" + id + "'");
            dvDSDaiLy.DataSource = SQLite.Instance().TaoBang("SELECT *FROM DanhSachDaiLy");
        }

        private void cbbTenDL_SelectedIndexChanged(object sender, EventArgs e)
        {
            _dsDaiLy = SQLite.Instance().LayDaiLy(cbbTenDL.SelectedItem.ToString());
        }

        private void button8_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.PathLuuAnh = txtThuMucLuuAnh.Text.Trim();
            Properties.Settings.Default.Save();
            System.Diagnostics.Process.Start("Explorer.exe", Properties.Settings.Default.PathLuuAnh);
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
