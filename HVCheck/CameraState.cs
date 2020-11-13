using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HVCheck
{
    public class CameraState
    {
        private static CameraState _instance;
        public static CameraState Instance()
        {
            if (_instance == null)
                _instance = new CameraState();
            return _instance;
        }
        public enum CameraStatus { RUN, STOP }
        private CameraStatus _currentState = CameraStatus.STOP;
        public CameraStatus CurrentState
        {
            get { return _currentState; }
            set
            {
                _currentState = value;
            }
        }
    }
}
