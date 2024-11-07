/*
 * SerialMon
 * Program settings class
 * Visutronik/Hamann
 * 30.10.2024 
 */

using System;
using System.ComponentModel;			// wichtig für Benutzung PropertyGrid!
using System.IO;
using System.Diagnostics;
using Visutronik.Commons;               // XmlAppSettings

namespace Visutronik.SerialMon
{
	/// <summary>
	/// Singleton-Klasse für benutzerdefinierte Programmeinstellungen
	/// </summary>
	public sealed class ProgSettings
	{
		#region ----- constants ------

		private const string XML_SETTINGSFILE = "config.xml";
		//private const string SQL_CONNECTFILE  = "DatabaseParameter.dat";

		#endregion

		#region ----- properties -----

		/// <summary>
		/// Verzeichnis für Logdateien (Programm, DB, Profildateien)
		/// </summary>
		[Description("Logdateiverzeichnis"), Category("Dateien")]
		public string LogFolder { get; set; } = @"D:\Temp\SerialMon";

        // --- Einstellungsmodus ---

        [Description("Mode"), Category("")]
        public int Mode { get; set; } = 0;

		// --- Window size and location ---

		//[Description("Fullscreenmodus"), Category("Allgemein")]
		//public bool Fullscreen { get; set; } = false;

		//[Description("WindowSize"), Category("Allgemein")]
		//public int WindowSizeX { get; set; }
		//public int WindowSizeY { get; set; }

		//[Description("WindowLocation"), Category("Allgemein")]
		//public int WindowLocX { get; set; }
		//public int WindowLocY { get; set; }

		// --- Hardware ------------------------------

		/// <summary>
		/// COM-Port 
		/// </summary>
		public string SerialPort { get; set; } = "COM1";

		/// <summary>
		/// Baudrate
		/// </summary>
		public int SerialBaudRate { get; set; } = 115200;

		#endregion

		#region ----- internal vars -----------

		public string strMessage = "";
        private string AppSettingsPath = "";
		private string XmlSettingsFile = "";


		// Instantiierung
		private static ProgSettings _instance = new ProgSettings();
		public static ProgSettings Instance { get { return _instance; } }

		#endregion

		#region ----- methods --------

		/// <summary>
		/// Dateipfad setzen
		/// </summary>
		/// <param name="path">Path to settings file or empty for default path</param>
		public bool SetSettingsPath(string path = "")
		{
			//if (path.Length == 0)	AppSettingsPath = DEFAULT_PATH;
			//else					AppSettingsPath = path;
			AppSettingsPath = path;
			Debug.WriteLine("  settings path = " + AppSettingsPath);

			XmlSettingsFile = Path.Combine(AppSettingsPath, XML_SETTINGSFILE);
			return true;
		}


		/// <summary>
		/// Programmeinstellungen aus Datei laden
		/// </summary>
		/// <returns><c>true</c> bei Erfolg</returns>
		public bool LoadSettings()
		{
			Debug.WriteLine("--- ProgSettings.LoadSettings() " + XmlSettingsFile + " ---");

			XmlAppSettings xmlset = null;
			bool result = true;

			try
			{
				xmlset = new XmlAppSettings(XmlSettingsFile, false);
				LogFolder		= xmlset.Read("Log-Verzeichnis", LogFolder);
                Mode = xmlset.Read("Mode", Mode);
                SerialPort = xmlset.Read("SerialPort", SerialPort);
				SerialBaudRate = xmlset.Read("BaudRate", SerialBaudRate);
                // Window               
                //Fullscreen = xmlset.Read("Fullscreen", this.Fullscreen);
                //WindowSizeX 	= xmlset.Read("WindowSizeX", this.WindowSizeX);
                //WindowSizeY 	= xmlset.Read("WindowSizeY", this.WindowSizeY);
                //WindowLocX 		= xmlset.Read("WindowPosX", this.WindowLocX);
                //WindowLocY 		= xmlset.Read("WindowPosY", this.WindowLocY);
            }
			catch (System.IO.FileNotFoundException fnfex)
            {
                this.strMessage = fnfex.Message;
				result = false;
			}
			catch (Exception ex)
			{
				this.strMessage = ex.Message;
				result = false;
			}
			return result;
		}


		/// <summary>
		/// Programmeinstellungen in Datei speichern
		/// </summary>
		/// <returns><c>true</c> bei Erfolg</returns>
		public bool SaveSettings()
		{
			Debug.WriteLine("--- ProgSettings.Save() " + XmlSettingsFile + " ---");
			bool result = true;
			XmlAppSettings xmlset = null;

			try
			{
				xmlset = new XmlAppSettings(XmlSettingsFile, false);

				// Dateipfade	
				xmlset.Write("Log-Verzeichnis", this.LogFolder);

                xmlset.Write("Mode", this.Mode);
                xmlset.Write("SerialPort", SerialPort);
                xmlset.Write("BaudRate", SerialBaudRate);

                //Window
                //xmlset.Write("Fullscreen", this.Fullscreen);
                //xmlset.Write("WindowSizeX", this.WindowSizeX);
                //xmlset.Write("WindowSizeY", this.WindowSizeY);
                //xmlset.Write("WindowPosX", this.WindowLocX);
                //xmlset.Write("WindowPosY", this.WindowLocY);

                xmlset.Save();
			}
			catch (System.IO.FileNotFoundException fnfex)
			{
				this.strMessage = fnfex.Message;
				result = false;
			}
			catch (Exception ex)
			{
				this.strMessage = ex.Message;
				result = false;
			}
			return result;
		}

		
		/// <summary>
		/// Programmeinstellungen im Debugfenster ausgeben:
		/// </summary>
        public void DebugSettings()
        {
			Debug.WriteLine("--- DEBUG Benutzereinstellungen ---");

			Debug.WriteLine("Log-Verzeichnis: " + LogFolder);
            Debug.WriteLine("Mode       : " + this.Mode);
            Debug.WriteLine("SerialPort : " + SerialPort);
            Debug.WriteLine("BaudRate   : " + SerialBaudRate);

            //Debug.WriteLine("Fullscreen : " + this.Fullscreen);
            //Debug.WriteLine("WindowSizeX: " + this.WindowSizeX);
            //Debug.WriteLine("WindowSizeY: " + this.WindowSizeY);
            //Debug.WriteLine("WindowPosX : " + this.WindowLocX);
            //Debug.WriteLine("WindowPosY : " + this.WindowLocY);
            Debug.WriteLine("--------------------------------");
		}

		#endregion

		#region ----- private methods -----

		/// <summary>
		/// Konstruktion / setzt defaults
		/// </summary>
		private ProgSettings()
		{
			// Defaults setzen
			// Window
			//this.WindowSizeX = 1200;
			//this.WindowSizeY = 800;
			//this.WindowLocX = 10;
			//this.WindowLocY = 10;
		}

		#endregion

	}
}
