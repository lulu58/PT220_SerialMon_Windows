//=================================================================================
// Einige ESC/POS Druckbefehle
// https://medium.com/@osamainayat4999/esc-pos-commands-f0ab0c3b22cc
// https://tabshop.smartlab.at/help-topics/help-esc-pos-codes.html
//=================================================================================
// 07.11.2024   initial


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Visutronik.SerialMon;

namespace Visutronik.Printers
{

    public sealed class EscPosHelper
    {
        #region ----- internal vars -----------

        // Instantiierung
        private static EscPosHelper _instance = new EscPosHelper();
        public static EscPosHelper Instance { get { return _instance; } }

        #endregion



        // ESC_POS_Command
        public enum EPCmd
        {
            EP_INIT, EP_STATUS,
            EP_LEFT, EP_CENTER, EP_RIGHT,
            EP_LF, EP_LF2, EP_LF5,
            EP_UL0, EP_UL1, EP_UL2,         // Underline off - 1pt - 2pt
            EP_TRM1, EP_TRM4                // Transmit status
        }

        public bool Diag { get; set; } = true;

        private SerialPort _sp = null;

        public EscPosHelper(SerialPort serialPort = null)
        {
            _sp = serialPort;
        }

        public void SetSerialPort(SerialPort serialPort)
        {
            _sp = serialPort;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        public void PrintLine(string text)
        {
            if (_sp != null)
            {
                _sp.WriteLine(text);
                Debug.WriteLineIf(Diag, text);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        public void PrintText(string text)
        {
            if (_sp != null)
            {
                _sp.Write(text);
                Debug.WriteLineIf(Diag, text);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="bytes"></param>
        public void PrintBuffer(char[] buffer, int bytes)
        {
            _sp.Write(buffer, 0, bytes);
        }


        public void PrintCmd(EPCmd cmd)
        {
            if (_sp != null)
            {
                _sp.Write(GetEscPos(cmd));
            }
        }

        public void PrintEscPos(EPCmd cmd, int value)
        {
            if (_sp != null)
            {
                _sp.Write(GetEscPosN(cmd, value));
            }
        }


        // 'ESC' = "\x1B"
        // 'GS'  = "\x1D" 	GS 	group separator

        // Justification ASCII      Code(Hex)
        // Left         ESC a 0 	0x1B 0x61 0x00
        // Center       ESC a 1 	0x1B 0x61 0x01
        // Right        ESC a 2 	0x1B 0x61 0x02

        /// <summary>
        /// Feste Kommandos
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public string GetEscPos(EPCmd cmd)
        {
            string s = string.Empty;
            switch (cmd)
            {
                // set printing defaults
                case EPCmd.EP_INIT:   s = "\x1B@"; break;

                //case EPCmd.EP_STATUS: s = "\x1BV"; break;
                // Justification ESC a  n
                case EPCmd.EP_LEFT:   s = "\x1B\x61\x00"; break;
                case EPCmd.EP_CENTER: s = "\x1B\x61\x01"; break;
                case EPCmd.EP_RIGHT:  s = "\x1B\x61\x02"; break;
                // Underline ESC -  n
                case EPCmd.EP_UL0:    s = "\x1B-0"; break;             // no underline 
                case EPCmd.EP_UL1:    s = "\x1B-1"; break;             // underline 1pt
                case EPCmd.EP_UL2:    s = "\x1B-2"; break;             // underline 2pt
                // multiple line feed
                case EPCmd.EP_LF2:    s = "\x1B\x64\x02"; break;       // "ESCd\x02"
                case EPCmd.EP_LF5:    s = "\x1B\x64\x05"; break;       // "ESCd\x05"
                // Transmit printer / paper status
                case EPCmd.EP_TRM1:   s = "\x10\x04\x01"; break;       // DLE EOT n=1   Printer status  
                case EPCmd.EP_TRM4:   s = "\x10\x04\x04"; break;       // DLE EOT n=4   Paper sensor status
                default: s = "unknown ESC/POS!\n"; break;
            }
            if (Diag)
            {
                Debug.WriteLine("------------------");
                foreach (char c in s)
                {
                    byte b = (byte)c;
                    Debug.Write($"0x{b:X2} ");
                }
                Debug.WriteLine("\n------------------");
            }
            return s;
        }

        /// <summary>
        /// Kommandos mit n Wiederholungen
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public string GetEscPosN(EPCmd cmd, int n)
        {
            string s = string.Empty;
            if ((n > 0) && (n < 256))
            {
                switch (cmd)
                {
                    case EPCmd.EP_LF: s = "\x1B\x64" + (char)n; break;
                    default: s = "unknown ESC/POS!\n";  break;
                }
            }
            if (Diag)
            {
                Debug.WriteLine("------------------");
                foreach (char c in s)
                {
                    byte b = (byte)c;
                    Debug.Write($"0x{b:X2} ");
                }
                Debug.WriteLine("\n------------------");
            }
            return s;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        private void M2P(string msg)
        {
            while (msg.Contains("ESC"))
            {
                msg = msg.Replace("ESC", "\x1B");   // \e = 27 wird noch nicht unterstützt...
            }
            Debug.WriteLine("print: " + msg);
            _sp.WriteLine(msg);
        }


    }
}
