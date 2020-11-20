using HVCheck.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HVCheck
{
    public partial class frmNhapDuLieuBangTay : Form
    {
        public DanhSachDaiLy _dsDaiLy = new DanhSachDaiLy();
        public frmNhapDuLieuBangTay()
        {
            InitializeComponent();
            txtMaSP.Focus();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (txtMaSP.Text.Trim().Length != 10)
            {
                MessageBox.Show("Mã phải gồm 10 kí tự! Kiểm tra lại");
                txtMaSP.Focus();
                return;
            }
            SQLite.Instance().ThemDuLieuSanPham(DateTime.Now.ToString("ddMMyyyyHHmmss"), DateTime.Now.ToString("dd/MM/yyyy"),
                                           _dsDaiLy.MaDL,_dsDaiLy.TenDL, txtMaSP.Text.Trim());
            MessageBox.Show("OK");
            txtMaSP.Clear();
            txtMaSP.Focus();
        }

        private void frmNhapDuLieuBangTay_Load(object sender, EventArgs e)
        {
            cbbDSDaiLy.DataSource = SQLite.Instance().LayToanBoBangDanhSachDaiLy()[1];
            _dsDaiLy = Form1._dsDaiLy;
            cbbDSDaiLy.SelectedItem = _dsDaiLy.TenDL;
            if (_dsDaiLy == null)
                MessageBox.Show("Chưa nhập tên Đại Lý");
        }

        private void txtMaSP_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnSave_Click(null, null);
            }
        }

        private void cbbDSDaiLy_SelectedIndexChanged(object sender, EventArgs e)
        {
            _dsDaiLy = SQLite.Instance().LayDaiLy(cbbDSDaiLy.SelectedItem.ToString());
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            txtMaSP.Clear();
            txtMaSP.Focus();
        }
    }
}
