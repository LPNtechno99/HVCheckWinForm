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
    }
}
