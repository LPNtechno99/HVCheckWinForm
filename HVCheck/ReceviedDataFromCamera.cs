using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HVCheck
{
    public class ReceviedDataFromCamera
    {
        public enum Result { PASS, FAIL}
        public Result InspectionResult { get; set; }

        private string _receviedstring;
        public string ReceviedString
        {
            get { return _receviedstring; }
            set
            {
                _receviedstring = value;
            }
        }
        public int CounterPASS { get; set; }
        public int CounterFAIL { get; set; }
    }
}
