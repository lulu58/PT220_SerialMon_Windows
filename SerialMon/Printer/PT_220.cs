//===========================================================================================
// PT-210 / PT-220 / MTP-2 / MTP-II 58mm Bluetooth Printer
// https://github.com/1rfsNet/GOOJPRT-Printer-Driver
//===========================================================================================
// 08.11.2024   add PrintTest(), using EscPosHelper class

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading;

namespace Visutronik.Printers
{
    public class PT_220
    {
        public string PrinterName { get; set; } = PRINTER;

        public string VirtualSerialPort { get; set; } = "";

        public string[] ServiceGuids { get; set; } = {SPP_GUID};

        public string LastError { get; internal set; } = "";

        public int PrinterStatus { get; internal set; } = 0;
        public int PaperStatus { get; internal set; } = 0;

        private const string PRINTER = "PT-220";	// Bluetooth device name
        private const string SPP_GUID = "00001101-0000-1000-8000-00805f9b34fb";
        // Lulus privater Drucker
        private const Int64 DeviceAddress = 0x86677A445B54;

        private readonly SerialPort serialPort = new SerialPort();

        //ctor
        public PT_220()
        {
            //ServiceGuids.Append(SPP_GUID);    // SPP
        }

        public bool PrintTest(SerialPort sp)
        {
            Debug.WriteLine("TODO: PT_220.PrintTest");

            bool result = true;

            if ((sp != null) && (sp.IsOpen))
            {
                EscPosHelper ep = EscPosHelper.Instance;

                // give ep instance the connected port
                ep.SetSerialPort(sp);
                sp.DataReceived -= null;

                try
                {
                    // some ESC/POS output
                    ep.PrintCmd(EscPosHelper.EPCmd.EP_INIT);
                    
                    // ask for printer status
                    ep.PrintCmd(EscPosHelper.EPCmd.EP_TRM1);
                    Thread.Sleep(100);
                    while (sp.BytesToRead > 0)
                    {
                        PrinterStatus = sp.ReadChar();
                        Debug.WriteLine("Printer status: " + PrinterStatus);    // 22
                        // 
                    }

                    // ask for paper status
                    ep.PrintCmd(EscPosHelper.EPCmd.EP_TRM4);
                    Thread.Sleep(100);
                    while (sp.BytesToRead > 0)
                    {
                        PaperStatus = sp.ReadChar();
                        Debug.WriteLine("Paper status: " + PaperStatus);        // 18
                        // 18 - ok, 114 - Papierende oder offen
                    }

                    ep.PrintCmd(EscPosHelper.EPCmd.EP_CENTER);
                    ep.PrintLine("Visutronik GmbH");

                    //ep.PrintLine("12345678901234567890123456789012\n" +
                    //             "********************************");
                    
                    //ep.PrintCmd(EscPosHelper.EPCmd.EP_UL1); ep.PrintLine("underline 1");         // ok
                    //ep.PrintCmd(EscPosHelper.EPCmd.EP_UL2); ep.PrintLine("underline 2");         // ok
                    //ep.PrintCmd(EscPosHelper.EPCmd.EP_UL0); ep.PrintLine("Good morning!");       // ok

                    //ep.PrintCmd(EscPosHelper.EPCmd.EP_RIGHT); ep.PrintLine("rechts");            // ok
                    //ep.PrintCmd(EscPosHelper.EPCmd.EP_LEFT);  ep.PrintLine("links");             // ok
                    //ep.PrintCmd(EscPosHelper.EPCmd.EP_CENTER); ep.PrintLine("zentriert");        // ok

                    ep.PrintCmd(EscPosHelper.EPCmd.EP_INIT);
                    ep.PrintLine("... now 1 empty lines ...");
                    ep.PrintEscPos(EscPosHelper.EPCmd.EP_LF, 1);
                    //ep.PrintCmd(EscPosHelper.EPCmd.EP_LF5);                          // ok
                    //ep.PrintLine("Don't worry, be happy!");
                    //M2P("ESCd\x02");    // feed 2 lines
                    //ep.PrintCmd(EscPosHelper.EPCmd.EP_STATUS);

                    List<char>bufferlist = new List<char>();
                    // barcode height
                    bufferlist.Add('\x1D'); bufferlist.Add('\x68'); bufferlist.Add('\x30');
                    // barcode
                    bufferlist.Add('\x1D'); bufferlist.Add('\x6B'); 
                    bufferlist.Add('\x03'); // EAN8
                    bufferlist.Add('0'); bufferlist.Add('1'); bufferlist.Add('2'); bufferlist.Add('3');
                    bufferlist.Add('4'); bufferlist.Add('5'); bufferlist.Add('6'); bufferlist.Add('7');
                    bufferlist.Add('\x00'); // NUL

                    // barcode
                    bufferlist.Add('\x1D'); bufferlist.Add('\x6B');
                    bufferlist.Add('\x02'); // EAN13
                    bufferlist.Add('0'); bufferlist.Add('1'); bufferlist.Add('2'); bufferlist.Add('3');
                    bufferlist.Add('4'); bufferlist.Add('5'); bufferlist.Add('6'); bufferlist.Add('7');
                    bufferlist.Add('8'); bufferlist.Add('9'); bufferlist.Add('0'); bufferlist.Add('1'); bufferlist.Add('2');
                    bufferlist.Add('\x00'); // NUL

                    ep.PrintBuffer(bufferlist.ToArray(), bufferlist.Count);

                    ep.PrintLine("bye...");
                    ep.PrintCmd(EscPosHelper.EPCmd.EP_LF2);                          // ok

                    while (sp.BytesToRead > 0)
                    {
                        Debug.WriteLine(sp.ReadChar());
                    }

                    LastError = "Print ok";
                }
                catch (Exception e)
                {
                    LastError = "Error: " + e.Message;
                }

            }
            else
            {
                LastError = "Printer not connected!";
                result = false;
            }
            return result;
        }


    }
}

/*
Im Gerätemanager für "Standardmäßige Seriell-über-Bluetooth-Verbindung (COM5)":
Das Gerät "BTHENUM\{00001101-0000-1000-8000-00805f9b34fb}_LOCALMFG&000a\7&1467683d&0&DC0D30531EDE_C00000000"
erfordert weitere Installationen.

Im Gerätemanager für "Standardmäßige Seriell-über-Bluetooth-Verbindung (COM6)":
Das Gerät "BTHENUM\{00001101-0000-1000-8000-00805f9b34fb}_LOCALMFG&0000\7&1467683d&0&000000000000_00000008" 
erfordert weitere Installationen.

Im Gerätemanager für "Standardmäßige Seriell-über-Bluetooth-Verbindung (COM7)":
 
BTHENUM\{00001124-0000-1000-8000-00805F9B34FB}_VID&000205AC_PID&0239 
 = Apple Keyboard human interface device 
Serieller Port kann geöffnet werden!
  
Gerätemanager - Druckwarteschlange - POS58 Printer:
-> Systemsteuerung\Hardware und Sound\Geräte und Drucker
-> PT-220 
    an COM7

 */

