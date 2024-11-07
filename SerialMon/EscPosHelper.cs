//=================================================================================
// Einige ESC/POS Druckbefehle
// https://medium.com/@osamainayat4999/esc-pos-commands-f0ab0c3b22cc
//=================================================================================
// 07.11.2024   initial


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SerialMon
{

    public class EscPosHelper
    {
        // ESC_POS_Command
        public enum EPCmd
        {
            EP_INIT,
            EP_LEFT, EP_CENTER, EP_RIGHT,
            EP_LF, EP_LF2,
            EP_DOWN
        }


        // 'ESC' = "\x1B"

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
            string s = "";
            switch (cmd)
            {
                // set printing defaults
                case EPCmd.EP_INIT:   s = "\x1B@"; break;
                // Justification
                case EPCmd.EP_LEFT:   s = "\x1Ba\x00"; break;
                case EPCmd.EP_CENTER: s = "\x1Ba\x01"; break;
                case EPCmd.EP_RIGHT:  s = "\x1Ba\x02"; break;
                case EPCmd.EP_LF2:    s = "\x1Bd\x02"; break;      // "ESCd\x02"
            }
            return s;
        }

        // 
        public string GetEscPos(EPCmd cmd, int value)
        {
            string s = "";
            if ((value > 0) && (value < 256))
            {
                switch (cmd)
                {
                    case EPCmd.EP_LF: s = "\x1Bd" + (byte)value; break;
                    default: s = "unknown ESC/POS!\n";  break;
                }
            }
            return s;
        }

    }
}
