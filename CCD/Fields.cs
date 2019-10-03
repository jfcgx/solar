using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCD
{

    public class Fields
    {
        int _v;          // 25920
        int _vS;
        int _vM;
        float _dM;
        int _vPV;        // 2470
        int _pPV;     // 0
        int _i;          // 0
        int _iL;
        string _lOAD;    // ON
        string _t;
        int _p;
        int _cE;
        float _sOC;
        int _tTG;
        string _alarm;
        string _relay;
        string _aR;

        int _h1;
        int _h2;
        int _h3;
        int _h4;
        int _h5;
        int _h6;
        int _h7;
        int _h8;
        int _h9;
        int _h10;
        int _h11;
        int _h12;
        int _h13;
        int _h14;
        int _h15;
        int _h16;
        float _h17;
        float _h18;
        float _h19;     // 1656
        float _h20;     // 180
        int _h21;     // 431
        float _h22;     // 118
        int _h23;     // 473
        string _bMV;
        string _fW;      // 123
        string _pID;     // 0xA04B
        string _sER;     // #  HQ1647UYS8I
        string _hSDS;    // 13
        string _mODE;
        float _aC_OUT_V;
        float _aC_OUT_I;
        string _wARN;
        string _checksum;  //      ‡

        DateTime _date;

        public string PID
        {
            get
            {
                if (_pID == null)
                    _pID = string.Empty;
                return _pID;
            }

            set
            {
                _pID = value;
            }
        }



        public DateTime Date
        {
            get
            {
                return _date;
            }

            set
            {
                _date = value;
            }
        }

        public int V
        {
            get
            {
                return _v;
            }

            set
            {
                _v = value;
            }
        }

        public int VS
        {
            get
            {
                return _vS;
            }

            set
            {
                _vS = value;
            }
        }

        public int VM
        {
            get
            {
                return _vM;
            }

            set
            {
                _vM = value;
            }
        }

        public float DM
        {
            get
            {
                return _dM;
            }

            set
            {
                _dM = value;
            }
        }

        public int VPV
        {
            get
            {
                return _vPV;
            }

            set
            {
                _vPV = value;
            }
        }

        public int PPV
        {
            get
            {
                return _pPV;
            }

            set
            {
                _pPV = value;
            }
        }

        public int I
        {
            get
            {
                return _i;
            }

            set
            {
                _i = value;
            }
        }

        public int IL
        {
            get
            {
                return _iL;
            }

            set
            {
                _iL = value;
            }
        }

        public string LOAD
        {
            get
            {
                return _lOAD;
            }

            set
            {
                _lOAD = value;
            }
        }

        public string T
        {
            get
            {
                return _t;
            }

            set
            {
                _t = value;
            }
        }

        public int P
        {
            get
            {
                return _p;
            }

            set
            {
                _p = value;
            }
        }

        public int CE
        {
            get
            {
                return _cE;
            }

            set
            {
                _cE = value;
            }
        }

        public float SOC
        {
            get
            {
                return _sOC;
            }

            set
            {
                _sOC = value;
            }
        }

        public int TTG
        {
            get
            {
                return _tTG;
            }

            set
            {
                _tTG = value;
            }
        }

        public string Alarm
        {
            get
            {
                return _alarm;
            }

            set
            {
                _alarm = value;
            }
        }

        public string Relay
        {
            get
            {
                return _relay;
            }

            set
            {
                _relay = value;
            }
        }

        public string AR
        {
            get
            {
                return _aR;
            }

            set
            {
                _aR = value;
            }
        }

        public int H1
        {
            get
            {
                return _h1;
            }

            set
            {
                _h1 = value;
            }
        }

        public int H2
        {
            get
            {
                return _h2;
            }

            set
            {
                _h2 = value;
            }
        }

        public int H3
        {
            get
            {
                return _h3;
            }

            set
            {
                _h3 = value;
            }
        }

        public int H4
        {
            get
            {
                return _h4;
            }

            set
            {
                _h4 = value;
            }
        }

        public int H5
        {
            get
            {
                return _h5;
            }

            set
            {
                _h5 = value;
            }
        }

        public int H6
        {
            get
            {
                return _h6;
            }

            set
            {
                _h6 = value;
            }
        }

        public int H7
        {
            get
            {
                return _h7;
            }

            set
            {
                _h7 = value;
            }
        }

        public int H8
        {
            get
            {
                return _h8;
            }

            set
            {
                _h8 = value;
            }
        }

        public int H9
        {
            get
            {
                return _h9;
            }

            set
            {
                _h9 = value;
            }
        }

        public int H10
        {
            get
            {
                return _h10;
            }

            set
            {
                _h10 = value;
            }
        }

        public int H11
        {
            get
            {
                return _h11;
            }

            set
            {
                _h11 = value;
            }
        }

        public int H12
        {
            get
            {
                return _h12;
            }

            set
            {
                _h12 = value;
            }
        }

        public int H13
        {
            get
            {
                return _h13;
            }

            set
            {
                _h13 = value;
            }
        }

        public int H14
        {
            get
            {
                return _h14;
            }

            set
            {
                _h14 = value;
            }
        }

        public int H15
        {
            get
            {
                return _h15;
            }

            set
            {
                _h15 = value;
            }
        }

        public int H16
        {
            get
            {
                return _h16;
            }

            set
            {
                _h16 = value;
            }
        }

        public float H17
        {
            get
            {
                return _h17;
            }

            set
            {
                _h17 = value;
            }
        }

        public float H18
        {
            get
            {
                return _h18;
            }

            set
            {
                _h18 = value;
            }
        }

        public float H19
        {
            get
            {
                return _h19;
            }

            set
            {
                _h19 = value;
            }
        }

        public float H20
        {
            get
            {
                return _h20;
            }

            set
            {
                _h20 = value;
            }
        }

        public int H21
        {
            get
            {
                return _h21;
            }

            set
            {
                _h21 = value;
            }
        }

        public float H22
        {
            get
            {
                return _h22;
            }

            set
            {
                _h22 = value;
            }
        }

        public int H23
        {
            get
            {
                return _h23;
            }

            set
            {
                _h23 = value;
            }
        }

        public string BMV
        {
            get
            {
                return _bMV;
            }

            set
            {
                _bMV = value;
            }
        }

        public string FW
        {
            get
            {
                return _fW;
            }

            set
            {
                _fW = value;
            }
        }

        public string SER
        {
            get
            {
                return _sER;
            }

            set
            {
                _sER = value;
            }
        }

        public string HSDS
        {
            get
            {
                return _hSDS;
            }

            set
            {
                _hSDS = value;
            }
        }

        public string MODE
        {
            get
            {
                return _mODE;
            }

            set
            {
                _mODE = value;
            }
        }

        public float AC_OUT_V
        {
            get
            {
                return _aC_OUT_V;
            }

            set
            {
                _aC_OUT_V = value;
            }
        }

        public float AC_OUT_I
        {
            get
            {
                return _aC_OUT_I;
            }

            set
            {
                _aC_OUT_I = value;
            }
        }

        public string WARN
        {
            get
            {
                return _wARN;
            }

            set
            {
                _wARN = value;
            }
        }

        public string Checksum
        {
            get
            {
                return _checksum;
            }

            set
            {
                _checksum = value;
            }
        }

        public Fields()
        {
            _v = 0;          // 25920
            _vS = 0;
            _vM = 0;
            _dM = 0;
            _vPV = 0;        // 2470
            _pPV = 0;     // 0
            _i = 0;          // 0
            _iL = 0;
            _lOAD = string.Empty;    // ON
            _t = string.Empty;
            _p = 0;
            _cE = 0;
            _sOC = 0;
            _tTG = 0;
            _alarm = string.Empty;
            _relay = string.Empty;
            _aR = string.Empty;

            _h1 = 0;
            _h2 = 0;
            _h3 = 0;
            _h4 = 0;
            _h5 = 0;
            _h6 = 0;
            _h7 = 0;
            _h8 = 0;
            _h9 = 0;
            _h10 = 0;
            _h11 = 0;
            _h12 = 0;
            _h13 = 0;
            _h14 = 0;
            _h15 = 0;
            _h16 = 0;
            _h17 = 0;
            _h18 = 0;
            _h19 = 0;     // 1656
            _h20 = 0;     // 180
            _h21 = 0;     // 431
            _h22 = 0;     // 118
            _h23 = 0;     // 473
            _bMV = string.Empty;
            _fW = string.Empty;      // 123
            _pID = string.Empty;     // 0xA04B
            _sER = string.Empty;     // #  HQ1647UYS8I
            _hSDS = string.Empty;    // 13
            _mODE = string.Empty;
            _aC_OUT_V = 0;
            _aC_OUT_I = 0;
            _wARN = string.Empty;
            _checksum = string.Empty;  //      ‡

            _date = DateTime.Now; ;
        }

        public bool ComparaEstado(Fields nuevo)
        {
            bool igual = true;
            if (PID != nuevo.PID) igual = false;    // 0xA04B
            if (P != nuevo.P) igual = false;    // 0xA04B
            if (DM != nuevo.DM) igual = false;    // 0xA04B
            if (VM != nuevo.VM) igual = false;    // 0xA04B
            if (FW != nuevo.FW) igual = false;      // 123
            if (SER != nuevo.SER) igual = false;     //   HQ1647UYS8I
            if (V != nuevo.V) igual = false;       // 25920
            if (I != nuevo.I) igual = false;       // 0
            if (VPV != nuevo.VPV)
                if ((VPV + 50 < nuevo.VPV) || (VPV > nuevo.VPV + 50))
                    igual = false;     // 2470
            if (PPV != nuevo.PPV) igual = false;     // 0
            if (LOAD != nuevo.LOAD) igual = false;    // ON
            if (H19 != nuevo.H19) igual = false;     // 1656
            if (H20 != nuevo.H20) igual = false;     // 180
            if (H21 != nuevo.H21) igual = false;     // 431
            if (H22 != nuevo.H22) igual = false;     // 118
            if (H23 != nuevo.H23) igual = false;     // 473
            if (HSDS != nuevo.HSDS) igual = false;    // 13
            return igual;
        }

        public override string ToString()
        {
            return string.Format("{0}|V:{1}| I:{2}| VPV:{3}| PPV:{4}", Date, V, I, VPV, PPV);
        }
    }
}
