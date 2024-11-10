//===========================================================================
// Class for Labelprinter NETUM G5
// data preparation for printing
// Visutronik / Lutz
// 21.02.2023
//===========================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using SkiaSharp;

namespace Visutronik.Printers
{
    /// <summary>
    /// 
    /// </summary>
    internal class NetumG5
    {
        /// <summary>
        /// store current printer name
        /// </summary>
        public string PrinterName { get; set; } = PRINTER;

        // Printing resolution, dots per inch
        public const int DPI = 203;

        /// <summary>
        /// 
        /// </summary>
        public string LastError { get; internal set; } = "";

        // printer name as bluetooth device includes serial number
        private const string PRINTER = "G5-21150170"; // DeviceAddress = B85044050C6C


        const int MAX_PRINTBYTES_PER_DOTLINE = 48;

        /// <summary>
        /// ctor
        /// </summary>
        public NetumG5()
        {

        }

        // Block 1 Jobanfang oder ResetPrinter ???
        // keine Nutzdaten
        public byte[] StartJob()
        {
            // 1f 20 00 88   = Block 1 Jobanfang oder Reset???
            return new byte[] { 0x1f, 0x20, 0x00, 0x88 };
        }

        // letzter Block - Heizung aus? Jobende?
        // keine Nutzdaten
        public byte[] EndJob()
        {
            // 1f 28 00 88 = letzter Block
            return new byte[] { 0x1f, 0x28, 0x00, 0x88 };
        }

        // 1f 27 01 30 88 = Block 2 nach Jobstart - Druckereinstellungen = 0x30 im vorletzten Byte???
        // 1 byte Nutzdaten: 0x30 = 0011 0000
        // siehe LineOffset(...) default
        public byte[] AfterStartJob()
        {
            return new byte[] { 0x1f, 0x27, 0x01, 0x30, 0x88 };
        }

        /// <summary>
        /// Move paper for cutting
        /// identisch mit DotLineFeed(63) 
        /// </summary>
        /// <returns>command bytes</returns>
        public byte[] CutPaperPosition()
        {
            // 1f 22 01 3f 88 = vorletzter Block - Zeilenvorschub zum Abreißen???
            return new byte[] { 0x1f, 0x22, 0x01, 0x3f, 0x88 };
        }

        /// <summary>
        /// DotLineFeed 
        /// </summary>
        /// <param name="n">repeat count</param>
        /// <returns>command bytes</returns>
        public byte[] DotLineFeed(int n = 12)
        {
            // "1f 22 01 12 88" => Zeilenvorschub 12 Zeilen
            byte[] bytes = new byte[] { 0x1f, 0x22, 0x01, 0x12, 0x88 };
            if ((n > 0) && (n < 256))
            {
                bytes[3] = Convert.ToByte(n);
            }
            return bytes;
        }


        /// <summary>
        /// Zeilenanfangsposition von rechts
        /// n = 0x08, 0x10, 0x18, 0x20, 0x28, 0x30
        ///        8,   16,   24,   32,   40,   48
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public byte[] LineOffset(int n = 0x30)
        {
            // "1f27013088" Zeilenoffset 0x30 - wenn zu klein, böses Geräusch!
            byte[] bytes = new byte[] { 0x1f, 0x27, 0x01, 0x30, 0x88 };
            if ((n > 0) && (n < 256))
            {
                bytes[3] = Convert.ToByte(n);
            }
            return bytes;
        }


        // 1f2502010088 - Block 3: Dimension1f25 des folgenden Print-Blockes ???
        public byte[] Dimension1f25(int n = 1)
        {
            // "1f 25 02 01 00 88" - 1x1, 1x2, 1x3, ... 1x8
            // "1f 25 02 01 01 88" - 1x64 
            // "1f 25 02 02 00 88" - 2x2
            // "1f 25 02 04 00 88" - 4x4
            // "1f 25 02 40 02 88" - 64x1
            byte[] bytes = new byte[] { 0x1f, 0x25, 0x02, 0x00, 0x00, 0x88 };
            if ((n > 0) && (n < 256))
            {
                bytes[3] = Convert.ToByte(n);
            }
            // bytes[4] > 0 wenn n >= 256
            return bytes;
        }


        /// <summary>
        /// Print a horizontal fixed line
        /// </summary>
        /// <param name="repeat">add more equal lines</param>
        /// <returns>command byte buffer for printing</returns>
        public byte[] PrintHorizontalLine(byte repeat = 0)
        {
            List<byte> horline = new List<byte>();
            for (int i = 0; i < 48; i++) horline.Add(0xFF);
            horline.AddRange(PrintLine(horline.ToArray(), repeat, 0));
            return horline.ToArray();
        }


        /// <summary>
        /// Print a single line
        /// </summary>
        /// <param name="linebuf">max. 48 Byte Nutzdaten, mehr wird vom Drucker abgeschnitten</param>
        /// <param name="repeat">Anzahl der Druckwiederholungen</param>
        /// <param name="offsetBytes">Anfang der Druckzeile (1 byte = 8 Dots)</param>
        /// <returns>command byte buffer for printing</returns>
        public byte[] PrintLine(byte[] linebuf, int repeat = 0, int offsetBytes = 0)
        {
            List<byte> printlinecmd = new List<byte>() { 0x1f, 0x21 };
            int databyte_count = linebuf.Length + 2;
            printlinecmd.Add((byte)databyte_count);
            printlinecmd.Add((byte)repeat);
            printlinecmd.Add((byte)offsetBytes);
            printlinecmd.AddRange(linebuf);
            printlinecmd.Add(0x88);

            return printlinecmd.ToArray();
        }

        /// <summary>
        /// print a SkiaSharp bitmap
        /// convert color bitmaps to bw bitmap
        /// </summary>
        /// <param name="bmp">source bitmap, max width 384 Pixel</param>
        /// <param name="threshold">black-white threshold value</param>
        /// <param name="byte_offset">line offset, byte_offset * 8 dots</param>
        /// <returns>command byte buffer for printing</returns>
        public byte[] PrintSKBitmap(SKBitmap bmp, int threshold = 127, byte byte_offset = 0)
        {
            Debug.WriteLine("PrintSkiaSharpBitmap");

            List<byte> bytes = new List<byte>();

            Debug.WriteLine(" - bitmap width % 8 = " + bmp.Width % 8);
            int linebytes = bmp.Width / 8;
            Debug.WriteLine(" - linebytes = " + linebytes); // 240 -> 30

            if (MAX_PRINTBYTES_PER_DOTLINE < linebytes + byte_offset)
            {
                linebytes = MAX_PRINTBYTES_PER_DOTLINE - byte_offset;
            }

            // --- create the bytes for printing ---

            // iterate image row
            for (int y = 0; y < bmp.Height; y++)
            {
                bytes.AddRange(new byte[] { 0x1f, 0x21 });          // line print cmd
                bytes.Add((byte)(linebytes + 2));                   // count of data bytes
                bytes.AddRange(new byte[] { 0x00, byte_offset });   // repeat count + linestart offset
                // iterate image colums
                int x = 0;
                for (int lb = 0; lb < linebytes; lb++)
                {
                    int b = 0;                                      // output byte to print
                    for (int p = 0; p < 8; p++)                     // convert 8 pixels to 1 output byte
                    {
                        SKColor color = bmp.GetPixel(x, y); 
                        x++;                                        // next pixel position
                        b = b << 1;                                 // shift output byte

                        // Grayscale = 0.299R + 0.587G + 0.114B
                        // Grayscale = (2.99R + 5.87G + 1.14B.) / 10 round (3R + 6G + 1B) / 10
                        int gray = (color.Red * 3 + color.Green * 6 + color.Blue) / 11;
                        //int mask = color.Blue > 127 ? 0 : 1;
                        int mask = gray < threshold ? 1 : 0;            // dot value, 1 means black dot!!!
                        b = b | mask;                               // set the dot value
                    }
                    bytes.Add((byte)b);
                }
                bytes.Add(0x88);                                    // cmd end
            }
            return bytes.ToArray();
        }



    }
}

/* Bei Druck am Papierende empfangen (Status?):
1F 40 7 1 2 3 1E 5 0 2 8D 
1F 40 7 1 2 3 1E 5 0 0 8F 
1F 40 7 1 2 3 1E 5 0 1 8E 
1F 40 7 1 2 3 1E 5 0 0 8F 
1F 40 7 1 2 3 1E 5 0 2 8D 
1F 40 7 1 2 3 1E 5 0 0 8F 
*/

/* Java API example
private void printText(String content)
{
    // Stellen Sie eine Verbindung zum ersten Druckerobjekt des Paars her
    // 连接配对的第一个打印机对象
    api.openPrinter("");

    // Zeichenauftrag starten, Parameter übergeben (Seitenbreite, Seitenhöhe)
    // 开始绘图任务，传入参数(页面宽度, 页面高度)
    api.startJob(50, 30, 0);

    // 开始一个页面的绘制，绘制文本字符串
    api.drawText(content, 4, 5, 40, 30, 4);

    // 结束绘图任务提交打印
    api.commitJob();
}

private void printQRcode(String content)
{
    // Stellen Sie eine Verbindung zum ersten Druckerobjekt des Paars her
    // 连接配对的第一个打印机对象
    api.openPrinter("");

    // Zeichenauftrag starten, Parameter übergeben (Seitenbreite, Seitenhöhe)
    // 开始绘图任务，传入参数(页面宽度, 页面高度)
    api.startJob(40, 30, 0);

    // 开始一个页面的绘制，绘制二维码
    api.draw2DQRCode(content, 4, 5, 20);

    // 结束绘图任务提交打印
    api.commitJob();
}
*/
