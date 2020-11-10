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
    public partial class frmDuLieu : Form
    {
        public frmDuLieu()
        {
            InitializeComponent();
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            dvDuLieuSanPham.DataSource = SQLite.Instance().TaoBang("SELECT *FROM DuLieuSanPham WHERE Time='" + dateTimePicker1.Value.ToString("dd/MM/yyyy") + "'");
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            dvDuLieuSanPham.DataSource = SQLite.Instance().TaoBang("SELECT *FROM DuLieuSanPham WHERE TenDL='" + cbbDaiLy.SelectedItem.ToString() + "'");
        }

        private void frmDuLieu_Load(object sender, EventArgs e)
        {
            cbbDaiLy.DataSource = SQLite.Instance().LayDanhSachDaiLy();
            dvDuLieuSanPham.DataSource = SQLite.Instance().TaoBang("SELECT *FROM DuLieuSanPham WHERE Time='" + DateTime.Now.ToString("dd/MM/yyyy") + "'");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            dvDuLieuSanPham.DataSource = SQLite.Instance().TaoBang("SELECT *FROM DuLieuSanPham WHERE Time='" + dateTimePicker1.Value.ToString("dd/MM/yyyy") + "'" + " AND TenDL='" + cbbDaiLy.SelectedItem.ToString() + "'");
        }

        private void tabControl1_VisibleChanged(object sender, EventArgs e)
        {
            dvSoLuongDat.DataSource = SQLite.Instance().TaoBang("SELECT *FROM DuLieuSoLuongDat");
            dvDuLieuSanPham.DataSource = SQLite.Instance().TaoBang("SELECT *FROM DuLieuSanPham WHERE Time='" + DateTime.Now.ToString("dd/MM/yyyy") + "'");
        }

        private void dateTimePicker2_ValueChanged(object sender, EventArgs e)
        {
            dvSoLuongDat.DataSource = SQLite.Instance().TaoBang("SELECT *FROM DuLieuSoLuongDat WHERE Time='"+dateTimePicker2.Value.ToString("dd/MM/yyyy")+"'");
        }
    }
}
