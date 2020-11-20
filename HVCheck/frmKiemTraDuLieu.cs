using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HookKeyboard;
using HVCheck.Models;

namespace HVCheck
{
    public partial class frmKiemTraDuLieu : Form
    {
        public delegate void XulysukienThayDoiMode();
        public event XulysukienThayDoiMode SuKienThayDoiCheDoChay;

        public DuLieuSanPham _dulieuSP = new DuLieuSanPham();
        public frmKiemTraDuLieu()
        {
            InitializeComponent();

            Form1._listener.OnKeyPressed += _listener_OnKeyPressed1;
        }

        private void _listener_OnKeyPressed1(object sender, KeyPressedArgs e)
        {
            if (e.KeyPressed == System.Windows.Input.Key.F1)
            {
                this.Close();
            }
            else if(e.KeyPressed == System.Windows.Input.Key.F5)
            {
                if (chbNhapTay.Checked)
                    chbNhapTay.Checked = false;
                else
                    chbNhapTay.Checked = true;
            }
        }

        private void frmKiemTraDuLieu_Load(object sender, EventArgs e)
        {
            this.Activate();
            cbbChonDaiLy.DataSource = SQLite.Instance().LayDanhSachDaiLy();
        }

        private void frmKiemTraDuLieu_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (SuKienThayDoiCheDoChay != null)
                SuKienThayDoiCheDoChay.Invoke();
        }

        private void chbNhapTay_CheckedChanged(object sender, EventArgs e)
        {
            if(chbNhapTay.Checked)
            {
                txtMaNhapTay.Enabled = true;
                btnKiemTra.Enabled = true;
                txtMaNhapTay.Focus();
            }
            else
            {
                txtMaNhapTay.Enabled = false;
                btnKiemTra.Enabled = false;
            }
        }

        private void txtMaNhapTay_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
            {
                btnKiemTra_Click(null, null);
            }
        }

        private void btnKiemTra_Click(object sender, EventArgs e)
        {
            _dulieuSP = SQLite.Instance().LayDuLieuSanPham(txtMaNhapTay.Text.Trim());
            lblChuoiNhan.Text = _dulieuSP.MaSP;
            lblMaDL.Text = _dulieuSP.MaDL;
            lblTenDL.Text = _dulieuSP.TenDL;
            lblNgayGioXuat.Text = _dulieuSP.Time;
            lblKhoangThoiGian.Text = Convert.ToDateTime(DateTime.Now.ToString("dd/MM/yyyy"))
            .Subtract(Convert.ToDateTime(_dulieuSP.Time)).ToString("dd") + " ngày";
            if (cbbChonDaiLy.SelectedItem.ToString() == _dulieuSP.TenDL)
            {
                lblKetQua.Text = "KHỚP";
                lblKetQua.BackColor = Color.Green;
                lblTenDL.BackColor = Color.SeaGreen;
                cbbChonDaiLy.BackColor = Color.SeaGreen;
            }
            else
            {
                lblKetQua.Text = "KHÔNG KHỚP";
                lblKetQua.BackColor = Color.Red;
                lblTenDL.BackColor = Color.Red;
                cbbChonDaiLy.BackColor = Color.Red;
                Form1.Fx1s.SetDevice2("M2", 1);
            }
        }

        private void cbbChonDaiLy_SelectedIndexChanged(object sender, EventArgs e)
        {
            txtMaNhapTay.Focus();
        }
    }
}
