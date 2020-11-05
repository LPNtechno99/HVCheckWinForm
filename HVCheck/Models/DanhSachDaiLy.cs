using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HVCheck.Models
{
    public class DanhSachDaiLy
    {
        private static DanhSachDaiLy _instance = null;
        public static DanhSachDaiLy Instance()
        {
            if (_instance == null) _instance = new DanhSachDaiLy();
            return _instance;
        }
        public int ID { get; set; }
        public string MaDL { get; set; }
        public string TenDL { get; set; }
    }
}
