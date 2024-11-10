//======================================================================================
// SerialMon - Empfang über seriellen Port protokollieren
// Projekt Arduino-Tools
// (c) 2023 visutronik GmbH
// 13.09.2023   V1.0 aus WinForm IceFighter V1.6 
// 30.10.2024   V1.1 add settings 
// 05.11.2024   V1.2 add Send + Print 
// 05.11.2024   V1.3 add EscPosHelper class
// 07.11-2024   V1.4 add PT_220 class
//======================================================================================
// unter .NET >= 5.0 Verweis auf Assembly "System.Io.Ports" zum Projekt hinzufügen!

using System;
using System.Threading;
using System.IO.Ports;
using System.Windows.Forms;
using System.Diagnostics;
using Visutronik.Printers;

namespace Visutronik.SerialMon
{
    public partial class Form1 : Form
    {
        const string PROGVER = "SerialMon V1.4 (08.11.2024)";
        const string STR_DATE_TIME_FORMAT = "yyyy-MM-dd_hh-mm";

        private bool NoPrinter = false;

        readonly SerialPort sp = new SerialPort();
        readonly ProgSettings settings = ProgSettings.Instance;     // Singleton class

        string[] ports;
        string port = "";
        int baudrate = 9600;
        bool running = false;

        #region --- Main form ----------------------------------------------------------------------------------

        /// <summary>
        /// ctor
        /// </summary>
        public Form1()
        {
            InitializeComponent();
            this.Text = PROGVER;
            btnStart.Text = "Start";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLoad(object sender, EventArgs e)
        {
            settings.SetSettingsPath("");   // im Programmverzeichnis
            settings.LoadSettings();

            chkCRLF.Checked = settings.CRLF > 0;
            chkZeit.Checked = settings.Mode > 0;

            int idx = 0;
            ports = SerialPort.GetPortNames();
            if (ports.Length == 0)
            {
                port = "COM1";
                cbxPort.Items.Add(port);
            }
            else
            {
                foreach (string port in ports)
                {
                    Debug.WriteLine(port);
                    cbxPort.Items.Add(port);
                    idx++;
                }
            }
            //cbxPort.SelectedIndex = 0;
            cbxPort.SelectedItem = settings.SerialPort;

            string[] baudrates = { "9600", "115200" };
            cbxBaud.Items.AddRange(baudrates);
            //cbxBaud.SelectedIndex = 0;
            cbxBaud.SelectedItem = settings.SerialBaudRate.ToString();

            ShowStatus("Please select serial port!");

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            Debug.WriteLine("OnFormClosing");
            Thread.Sleep(500);
            if ((sp != null) && sp.IsOpen)
            {
                Debug.WriteLine(" - close serial port");
                try
                {
                    //sp.DataReceived -= sp_DataReceived;
                    if (running)
                        sp.Close();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
            System.Threading.Thread.Sleep(500);
            running = false;

            //Debug.WriteLine(" - save settings");
            settings.SerialPort = sp.PortName;
            settings.SaveSettings();
            Debug.WriteLine(" - close window");
        }

        #endregion

        #region --- Main form events -------------------------------------------------------------------------

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonClose_Click(object sender, EventArgs e)
        {
            Debug.WriteLine("... exit ...");
            this.Close();
        }

        /// <summary>
        /// selektiert Port
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbxPort_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.port = (string)cbxPort.SelectedItem;
            Debug.WriteLine($"... {port} ...");
        }

        /// <summary>
        /// selektiert Baudrate
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbxBaud_SelectedIndexChanged(object sender, EventArgs e)
        {
            baudrate = Convert.ToInt32(cbxBaud.SelectedItem);
            Debug.WriteLine($"... {baudrate} ...");
        }
        /// <summary>
        /// activate timestamp in output
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Zeit_CheckedChanged(object sender, EventArgs e)
        {
            settings.Mode = chkZeit.Checked ? 1 : 0;
        }

        /// <summary>
        /// switch CRLF / LF at end of line in SerialPort.ReadLine / WriteLine
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CRLF_CheckedChanged(object sender, EventArgs e)
        {
            if (sender == chkCRLF)
            {
                if (chkCRLF.Checked)
                    sp.NewLine = "\r\n";
                else
                    sp.NewLine = "\n";

                settings.CRLF = chkCRLF.Checked ? 1 : 0;
            }
        }

        /// <summary>
        /// Setze Flag zum Speichern empfangener Daten
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Start_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            {
                if (running)
                {
                    ClosePort();
                    running = false;
                    btnStart.Text = "Start";
                }
                else
                {
                    running = OpenPort(port, baudrate);
                    if (running)
                    {
                        btnStart.Text = "Stopp";

                        settings.SerialPort = port;
                        settings.SerialBaudRate = baudrate;
                    }
                    else
                    {

                    }
                }
            }
            Cursor = Cursors.Default;
        }

        /// <summary>
        /// Empfangene Daten sichern
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Speichern_Click(object sender, EventArgs e)
        {
            Sichern();
        }

        /// <summary>
        /// send the content of input box to serial port
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Send_Click(object sender, EventArgs e)
        {
            Senden(textBox1.Text);
        }

        #endregion

        #region ----- Helpers --------------------------

        #region --- Statusbar ---

        delegate void StatusCallback(string s);

        void ShowStatus(string msg)
        {
            if (labelStatus.InvokeRequired)
            {
                labelStatus.BeginInvoke(new StatusCallback(ShowStatus), msg);
            }
            else
            {
                this.labelStatus.Text = msg;
            }
        }

        #endregion

        #region --- Listbox ---

        delegate void OutputCallback(string s);

        void Output(string s)
        {
            if (listBox1.InvokeRequired)
            {
                listBox1.BeginInvoke(new OutputCallback(Output), s);
            }
            else
            {
                Debug.WriteLine(s);
                if (settings.Mode == 1)
                {
                    string time = DateTime.Now.ToShortTimeString();
                    // am Ende einfügen und den letzten Eintrag sichtbar machen:
                    listBox1.Items.Add($"{time}: {s}");
                }
                else
                {
                    listBox1.Items.Add(s);
                }
                // Länge der Liste begrenzen - ältesten Eintrag löschen
                if (listBox1.Items.Count > 500)
                    listBox1.Items.RemoveAt(0);
                // Cursor auf letzten Eintrag setzen
                listBox1.SelectedIndex = listBox1.Items.Count - 1;
                // und sofort neu zeichnen
                listBox1.Update();
            }
        }

        #endregion

        #endregion

        #region ----- COM-Port -------------------------

        /// <summary>
        /// Initialisiert und öffnet den seriellen Port
        /// </summary>
        /// <param name="comport">Portname</param>
        /// <returns>true bei Erfolg</returns>
        private bool OpenPort(string comport = "COM1", int baud = 9600)
        {
            Debug.WriteLine($"OpenPort({comport}, {baud})");
            bool bOk = true;
            try
            {
                sp.PortName = comport;
                sp.BaudRate = baud;
                sp.NewLine = "\r\n";    // IceFighterData.EOT;  statt wie beim Senden "\r\n";
                //sp.NewLine = "\n";    // Arduino Druckmesser
                sp.ReadTimeout = 500;
                sp.WriteTimeout = 200;
                // TODO sp.DataReceived aktivieren, wenn kein BT-Printer am Port hängt
                if (NoPrinter)
                {
                    sp.DataReceived += OnSerialportDataReceived;
                }
                sp.PinChanged += sp_PinChanged;
                sp.ErrorReceived += sp_ErrorReceived;
                sp.Open();
                ShowStatus($"{comport} ({baud}): geöffnet, bereit zum Datenempfang ...");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                ShowStatus(String.Format("{0}: {1}", comport, ex.Message));
                bOk = false;
            }
            return bOk;
        }

        /// <summary>
        /// 
        /// </summary>
        private void ClosePort()
        {
            if (sp != null)
            {
                if (sp.IsOpen)
                {
                    Debug.WriteLine($" - {sp.PortName} closing");
                    sp.Close();
                }
                ShowStatus("Port geschlossen");
            }
        }

        /// <summary>
        /// Event: serielle Daten empfangen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnSerialportDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                Thread.Sleep(10);
                Debug.WriteLine($" rcvd {sp.BytesToRead} bytes");
                string s = sp.ReadLine();

                // wenn Zeilenende CRLF enthält ---> entfernen
                if (s.Contains("\n\r"))
                {
                    Debug.WriteLine("Remove(CRLF)");
                    //s = s.Replace("\n\r", "");
                }

                //ShowStatus(s);

                // Empfangene Daten in Datei schreiben
                //if (saveIceData)
                //{
                //    this.Save(s);
                //}

                //Debug.WriteLine("-----");
                //byte[] bytes = Encoding.ASCII.GetBytes(s);
                //foreach (byte b in bytes)
                //    Debug.WriteLine($"0x{b:X}");
                //string sUTF8 = Encoding.UTF8.GetString(bytes);
                //Output(sUTF8);

                Output(s);

                //string[] parts = s.Split(';');
                //foreach (string p in parts)
                //{
                //    Debug.WriteLine(p);
                //    Output(p);
                //}
            }
            catch (Exception ex)
            {
                Debug.WriteLine("OnSerialportDataReceived: " + ex.Message);
            }
        }

        void sp_PinChanged(object sender, SerialPinChangedEventArgs e)
        {
            //throw new NotImplementedException();
            Output("sp_PinChanged!");
        }

        void sp_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            //throw new NotImplementedException();
            Output("sp_ErrorReceived!");
        }

        #endregion


        /// <summary>
        /// Speichern eines Strings in Textdatei im Programmverzeichnis
        /// </summary>
        /// <returns>true bei Erfolg</returns>
        public bool Sichern()
        {
            DateTime dt = DateTime.Now;
            string filename = String.Format("Serial_{0}.log", dt.ToString(STR_DATE_TIME_FORMAT));
            System.IO.StreamWriter writer;
            try
            {
                writer = new System.IO.StreamWriter(filename, false, System.Text.Encoding.UTF8);

                foreach (var message in listBox1.Items)
                {
                    writer.WriteLine(message);
                }
                writer.Flush();
                writer.Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Sichern() Exception: " + ex.Message);
                Output("Fehler beim Speichern!");
                ShowStatus(ex.Message);
                return false;
            }
            return true;
        }

        /// <summary>
        /// send msg as line to serial port
        /// </summary>
        /// <param name="msg"></param>
        private void Senden(string msg)
        {
            if (msg == null)
            {
                ShowStatus("Keine Eingabe!");
                return;
            }

            if (sp.IsOpen)
            {
                sp.WriteLine(msg);
            }
            else
            {
                ShowStatus("Port nicht geöffnet!");
            }
        }

        /// <summary>
        /// make a test print to connected bluetooth label printer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Print_Click(object sender, EventArgs e)
        {
            Output($"Printing with PT-220 Thermal Printer");
            PT_220 pt_220 = new PT_220();
            pt_220.VirtualSerialPort = sp.PortName;
            pt_220.PrintTest(sp);

            Output($"Papierstatus: {pt_220.PaperStatus}");
        }

    }
}

