using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HVCheck
{
    public class RunMode
    {
        public enum mode { NORMAL, CHECK }

        private mode _runmodecurrent = mode.NORMAL;
        public mode RunModeCurrent
        {
            get { return _runmodecurrent; }
            set
            {
                _runmodecurrent = value;
            }
        }
    }
}
