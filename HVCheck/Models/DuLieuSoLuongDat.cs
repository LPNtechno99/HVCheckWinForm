using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HVCheck.Models
{
    public class DuLieuSoLuongDat
    {
        private static DuLieuSoLuongDat _instance = null;
        public static DuLieuSoLuongDat Instance()
        {
            if (_instance == null) _instance = new DuLieuSoLuongDat();
            return _instance;
        }
        public int ID { get; set; }
        public string TimeStamp { get; set; }
        public string Time { get; set; }
        public string MaDL { get; set; }
        public string TenDL { get; set; }
        public int SoLuongDat { get; set; }
    }
}
