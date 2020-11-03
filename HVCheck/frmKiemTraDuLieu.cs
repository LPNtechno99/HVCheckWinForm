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
    public partial class frmKiemTraDuLieu : Form
    {
        public delegate void XulysukienThayDoiMode();
        public event XulysukienThayDoiMode SuKienThayDoiCheDoChay;

        public frmKiemTraDuLieu()
        {
            InitializeComponent();
        }

        private void frmKiemTraDuLieu_Load(object sender, EventArgs e)
        {
            
        }

        private void frmKiemTraDuLieu_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (SuKienThayDoiCheDoChay != null)
                SuKienThayDoiCheDoChay.Invoke();
        }
    }
}
