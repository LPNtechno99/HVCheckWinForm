using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HVCheck.Models
{
    public class DuLieuSanPham
    {
        private static DuLieuSanPham _instance = null;
        public static DuLieuSanPham Instance()
        {
            if (_instance == null) _instance = new DuLieuSanPham();
            return _instance;
        }
        public int ID { get; set; }
        public string TimeStamp { get; set; }
        public string Time { get; set; }
        public string MaDL { get; set; }
        public string TenDL { get; set; }
        public string MaSP { get; set; }
    }
}
