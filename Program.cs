using System;
using System.Drawing;
using System.Windows.Forms;

using System.Drawing.Imaging;
using System.Timers;
using System.Reflection;
using System.IO; 

namespace ScreenShotUtility
{
    /// <summary>
    /// Provides functions to capture the entire screen, or a particular window, and save it to a file.
    /// Added SCREEN selection via config file
    /// </summary>


    // xml file configuration for 'config.xml'
    public class Conf
    {
        public string name { get; set; }
        public string dir { get; set; }
        public double threshold { get; set; }
        public bool diff_flag { get; set; }
        public int interval { get; set; }
        public int screen { get; set; }

    }



    // xml file configuration for 'config.xml'
    public class Config
    {
        public string name { get; set; }
        public string dir { get; set; }
        public double threshold { get; set; }
        public bool diff_flag { get; set; }
        public int interval { get; set; }
        public int screen { get; set; }

    }

    class Program
    {
        static int fileCount = 0;
        static Image imgR = null, imgT = null, imgD = null;
        static bool init = true;
        static ScreenCapture sc;

        public static string DIR = "NA";
        public static double THRESHOLD = 0.002; // 0.01 => 1% 
        private static bool DIFF_FLAG = true ;
        private static int INTERVAL = 500;
        private static int SCREEN = 0; // default screen = 0

        // read xml file 'config.xml'
        public static void readConfiguration()
        {
            // Get System Path to Binary (current directory)
            string path = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\";
           
            // Now we can read the serialized book ...  
            System.Xml.Serialization.XmlSerializer reader = new System.Xml.Serialization.XmlSerializer(typeof(Config));            
            System.IO.StreamReader file = new System.IO.StreamReader(path + @"config.xml");
            Config conf = (Config)reader.Deserialize(file);
            file.Close();


            // populate vars 
            DIR = Convert.ToString(conf.dir);
            THRESHOLD = Convert.ToDouble(conf.threshold);
            DIFF_FLAG = Convert.ToBoolean(conf.diff_flag);
            INTERVAL = Convert.ToInt32(conf.interval);
            SCREEN = Convert.ToInt32(conf.screen);

            /// test read 
            //Console.WriteLine(dir);
            //Console.WriteLine(threshold);
            //Console.WriteLine(diff_flag);
            //Console.WriteLine(interval);
        }

        // write xml file 'config.xml'
        public static void writeConfiguration ()
        {
            // Get System Path to Binary (current directory)
            string path = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\";

            // First write something so that there is something to read ...  
            var b = new Config { name = "Screenshot Configuration 1.0" };
            b.interval = 500;
            b.dir = path;
            b.threshold = 0.01;
            b.diff_flag = false;
            b.screen = 0;
            

            var writer = new System.Xml.Serialization.XmlSerializer(typeof(Config));
            var wfile = new System.IO.StreamWriter(path + "config.xml");
            writer.Serialize(wfile, b);
            wfile.Close();
        }

        static void Main(string[] args)
        {

            /* http://stackoverflow.com/questions/12535722/what-is-the-best-way-to-implement-a-timer */

            var dateandtime = DateTime.Now;
            var date = dateandtime.ToString("yyyy-dd-MM_HH-mm");
            string path = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\";


            if (args.Length == 0)
                //read config file config.xml
                readConfiguration();

            else 
            {
                //generate config file config.xml
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i] == "--config")
                    {
                        writeConfiguration();
                        return;
                    }
                }
            }

            
            //// write an xml config: test write variable


            //// READ IMAGE SAVE PATH FILE 
            //string savePath = "NA";
            //string line = "";
            //try
            //{
            //    System.IO.StreamReader file = new System.IO.StreamReader(path + "imageSavePath.config");
            //    Console.Write("savePath:");
            //    line = file.ReadLine();
            //    Console.WriteLine(line);
            //    // assign sensorID to the vars
            //    savePath = line.Trim();
            //    file.Close();
            //}
            //catch (Exception ex) { }


            //// READ THRESHOLD VALUE  from "threshold" file 
            //line = "";
            //try
            //{
            //    System.IO.StreamReader file = new System.IO.StreamReader(path + "threshold.config");
            //    Console.Write("threshold:");
            //    line = file.ReadLine();
            //    Console.WriteLine(line);
            //    // assign thresholds to the vars
            //    threshold = Convert.ToDouble(line.Trim());
            //    file.Close();
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex.ToString());
            //}


            // CREATE DIR AND PATH 

            if (DIR == "NA") DIR = path + @"\\img\\saved_" + date.ToString(); // save in the bin directory 
            else DIR = DIR + @"\\saved_" + date.ToString();

            System.IO.Directory.CreateDirectory(DIR);

            Console.WriteLine("Image Save Path = " + DIR);
            Console.WriteLine("Threshold=" + THRESHOLD);

            

            // Greetings 
            Console.WriteLine("!-----------------------------Hello World!---------------------------------------------!");
            Console.WriteLine("<             This is Towshif's screen grabber utility based on T-R                    >");
            Console.WriteLine("To generate config file, open cmd in the directory, run \n> ScreenShotApp.exe --config");
            Console.WriteLine("!--------------------------------------------------------------------------------------!\n");

            // Start Runner 
            sc = new ScreenCapture();
            System.Timers.Timer aTimer = new System.Timers.Timer();
            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            aTimer.Interval = INTERVAL;
            aTimer.Enabled = true;

            Console.WriteLine("Press \'q\' to quit the sample.");
            while (Console.Read() != 'q') ;


        }

        // Method # 1: using win32 get desktop api 

        static Bitmap CaptureScreen()
        {
            Bitmap memoryImage;
            memoryImage = new Bitmap(1000, 900);
            Size s = new Size(memoryImage.Width, memoryImage.Height);

            Graphics memoryGraphics = Graphics.FromImage(memoryImage);

            memoryGraphics.CopyFromScreen(0, 0, 0, 0, s);

            return memoryImage;

        }

        // Method # 2: using win forms "screens" api

        private static Bitmap CaptureMyScreen()
        {
            return CaptureMyScreen(0);
        }

        private static Bitmap CaptureMyScreen(int screen)
        {
            Bitmap captureBitmap = null;
            try
            {
                //Creating a Rectangle object which will capture our Current Screen
                Rectangle captureRectangle = Screen.AllScreens[screen].Bounds;

                //Creating a new Bitmap object
                captureBitmap = new Bitmap(captureRectangle.Size.Width, captureRectangle.Size.Height, PixelFormat.Format32bppArgb);

                //Creating a New Graphics Object
                Graphics captureGraphics = Graphics.FromImage(captureBitmap);

                //Copying Image from The Screen
                captureGraphics.CopyFromScreen(captureRectangle.Left, captureRectangle.Top, 0, 0, captureRectangle.Size);

                //Saving the Image File (I am here Saving it in My E drive).
                captureBitmap.Save(@"G:\img\Capture.jpg", ImageFormat.Jpeg);
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return captureBitmap;
        }

        /*
         *  Main Caller functions  CaptureMyScreen() OR ScreenCapture.CaptureScreen() 
         */
        private static void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            //Console.WriteLine("Hello World!");

            // Method I: c++ windows handler for screenshot using ScreenCapture Class
            //using (var c = sc.CaptureScreen())

            // Method II: Windows forms Screen function for capture 
            using (var c = CaptureMyScreen(SCREEN))
            {

                // capture entire screen, and save it to a file
                if (init) { imgR = new Bitmap(c); init = false; }
                else { imgR.Dispose(); imgR = imgT; } // swap last target with current reference image.
                                                      //imgT = imgR;

                //if (imgT != null)
                //{
                //    Console.WriteLine("imgT not NULL!");imgT.Dispose();
                //}


                imgT = new Bitmap(c);

                //imgT = CaptureScreen();
                // display image in a Picture control named imageDisplay
                //this.imageDisplay.Image = img;
                // capture this window, and save it
                //sc.CaptureWindowToFile(this.Handle, "C:\\temp2.gif", ImageFormat.Gif);

                //imgT.Save("G:\\temp" + (fileCount++) + ".gif", ImageFormat.Gif);


                if (imgDiff(imgR, imgT))
                //if (isPixelDiff(new Bitmap (imgR), new Bitmap (imgT)))
                {
                    //imgT.Save(dir + "\\" + (fileCount++) + ".gif", ImageFormat.Gif);
                    imgT.Save(DIR + "\\" + (fileCount++) + ".png", ImageFormat.Png); // use PNG for High Res Images 
                    //imgD.Save(dir + "\\" + temp_diff" + (fileCount) + ".png", ImageFormat.png);
                }


                //imgD = convertToGray_roughalgo(imgT);
                //imgD = new Bitmap (imgT);
                //imgD.Save("G:\\temp_gray" + (fileCount++) + ".gif", ImageFormat.Gif);
                //imgD.Dispose();
                //sc.CaptureScreenToFile("G:\\temp2.gif", ImageFormat.Gif);
                c.Dispose();
                //imgT.Dispose();
                imgR.Dispose();
            }

        }

        /*  http://stackoverflow.com/questions/9367138/calculate-image-differences-in-c-sharp 
         *  https://msdn.microsoft.com/en-us/library/system.drawing.bitmap%28v=vs.100%29.aspx
         */
        static bool imgDiff(Image img1, Image img2)
        {
            //var a = (Bitmap)Image.FromFile("image1.png");
            //var b = (Bitmap)Image.FromFile("image2.png");
            //var a = (Bitmap)imgR;
            //var b = (Bitmap)imgT;
            //Image imgC = PixelDiff(a, b);
            //imgD = PixelDiff1(a, b, Color.Black);

            if (isPixelDiff(new Bitmap(imgR), new Bitmap(imgT))) return true; else return false;
        }

        
        /*************************************************
         *  Find if there is reasonable difference b/w 
         *  in image1 and image2 (towshif ali  ver.5)
         *  
         * return true / false
         * 
         * *************************************************/



        static bool isPixelDiff(Bitmap image1, Bitmap image2)
        {
            if (image1 == null | image2 == null)
                return false;

            if (image1.Height != image2.Height || image1.Width != image2.Width)
                return false;

            Bitmap diffImage = image2.Clone() as Bitmap;

            int height = image1.Height;
            int width = image1.Width;

            BitmapData data1 = image1.LockBits(new Rectangle(0, 0, width, height),
                                               ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            BitmapData data2 = image2.LockBits(new Rectangle(0, 0, width, height),
                                               ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            BitmapData diffData = diffImage.LockBits(new Rectangle(0, 0, width, height),
                                                   ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            int a1 = 0, a2 = 0, a3 = 0, count = 0;

            unsafe
            {
                byte* imagePointer1 = (byte*)data1.Scan0;
                byte* imagePointer2 = (byte*)data2.Scan0;
                byte* imagePointer3 = (byte*)diffData.Scan0;
                for (int i = 0; i < data1.Height; i++)
                {
                    for (int j = 0; j < data1.Width; j++)
                    {
                        // write the logic implementation here


                        /*  ALgo Logic START here */
                        a1 = (imagePointer1[0] + imagePointer1[1] +
                             imagePointer1[2]) / 3;
                        a2 = (imagePointer2[0] + imagePointer2[1] +
                             imagePointer2[2]) / 3;

                        a3 = Math.Abs(a1 - a2);  // < 0? 0 : Math.Abs (a1-a2);
                        //Console.Write(a1 + "|" + a2 + "|" + a3);
                        //Console.Write(imagePointer1[0]+",");
                        imagePointer3[0] = (byte)a3;
                        imagePointer3[1] = (byte)a3;
                        imagePointer3[2] = (byte)a3;
                        imagePointer3[3] = imagePointer1[3];

                        if (Math.Abs(a1 - a2) > 50) count++;

                        /*  ALgo Logic END here */

                        //4 bytes per pixel
                        imagePointer1 += 4;
                        imagePointer2 += 4;
                        imagePointer3 += 4;
                    }//end for j
                    //4 bytes per pixel
                    imagePointer1 += data1.Stride -
                                    (data1.Width * 4);
                    imagePointer2 += data2.Stride -
                                    (data2.Width * 4);
                    imagePointer3 += diffData.Stride -
                                    (diffData.Width * 4);
                }//end for i

                //Console.Write("Exiting Loop");
            }//end unsafe
            image1.UnlockBits(data1);
            image2.UnlockBits(data2);
            diffImage.UnlockBits(diffData);


            // save diff image 
            if ( DIFF_FLAG ) diffImage.Save(DIR + "\\" + "diff" + (fileCount) + ".png", ImageFormat.Png);

            // dispose and release memory 
            image1.Dispose();
            image2.Dispose();
            diffImage.Dispose();

            Console.WriteLine();
            Console.Write("[" + height + "X" + width + "] ");
            Console.Write(fileCount + "> count = " + count + " of " + (height * width) + "=%" + (100 * (double)count / (double)(height * width)));
            //Console.WriteLine();
            if (count > THRESHOLD * (height * width)) { return true; }
            else return false;

            //return diffImage;
        }//end processImage





        // unused function
        //static unsafe Bitmap PixelDiff(Bitmap a, Bitmap b)
        static Bitmap PixelDiff1(Bitmap image1, Bitmap image2, Color matchColor)
        {
            if (image1 == null | image2 == null)
                return null;

            if (image1.Height != image2.Height || image1.Width != image2.Width)
                return null;

            Bitmap diffImage = image2.Clone() as Bitmap;

            int height = image1.Height;
            int width = image1.Width;

            BitmapData data1 = image1.LockBits(new Rectangle(0, 0, width, height),
                                               ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData data2 = image2.LockBits(new Rectangle(0, 0, width, height),
                                               ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData diffData = diffImage.LockBits(new Rectangle(0, 0, width, height),
                                                   ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            unsafe
            {
                byte* data1Ptr = (byte*)data1.Scan0;
                byte* data2Ptr = (byte*)data2.Scan0;
                byte* diffPtr = (byte*)diffData.Scan0;

                byte[] swapColor = new byte[3];
                swapColor[0] = matchColor.B;
                swapColor[1] = matchColor.G;
                swapColor[2] = matchColor.R;

                int rowPadding = data1.Stride - (image1.Width * 3);

                // iterate over height (rows)
                for (int i = 0; i < height; i++)
                {
                    // iterate over width (columns)
                    for (int j = 0; j < width; j++)
                    {
                        int same = 0;

                        byte[] tmp = new byte[3];

                        // compare pixels and copy new values into temporary array
                        // all pixel comparison operation done here.
                        for (int x = 0; x < 3; x++)
                        {
                            tmp[x] = data2Ptr[0];
                            if (data1Ptr[0] == data2Ptr[0])
                            {
                                same++;
                            }
                            data1Ptr++; // advance image1 ptr
                            data2Ptr++; // advance image2 ptr
                        }

                        // swap color or add new values
                        for (int x = 0; x < 3; x++)
                        {
                            diffPtr[0] = (same == 3) ? swapColor[x] : tmp[x];
                            diffPtr++; // advance diff image ptr
                        }
                    }

                    // at the end of each column, skip extra padding
                    if (rowPadding > 0)
                    {
                        data1Ptr += rowPadding;
                        data2Ptr += rowPadding;
                        diffPtr += rowPadding;
                    }
                }
            }
            image1.UnlockBits(data1);
            image2.UnlockBits(data2);
            diffImage.UnlockBits(diffData);

            //image1.Dispose();
            //image2.Dispose();

            return diffImage;
        }

        /*************************************************
         *  Find difference Image in image1 and image2 (towshif ali  ver.2)
         *  
         * 
         * *************************************************/

        // unused function
        static Bitmap PixelDiff(Bitmap image1, Bitmap image2)
        {
            if (image1 == null | image2 == null)
                return null;

            if (image1.Height != image2.Height || image1.Width != image2.Width)
                return null;

            Bitmap diffImage = image2.Clone() as Bitmap;

            int height = image1.Height;
            int width = image1.Width;

            BitmapData data1 = image1.LockBits(new Rectangle(0, 0, width, height),
                                               ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            BitmapData data2 = image2.LockBits(new Rectangle(0, 0, width, height),
                                               ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            BitmapData diffData = diffImage.LockBits(new Rectangle(0, 0, width, height),
                                                   ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            int a1 = 0, a2 = 0, a3 = 0, count = 0;

            unsafe
            {
                byte* imagePointer1 = (byte*)data1.Scan0;
                byte* imagePointer2 = (byte*)data2.Scan0;
                byte* imagePointer3 = (byte*)diffData.Scan0;
                for (int i = 0; i < data1.Height; i++)
                {
                    for (int j = 0; j < data1.Width; j++)
                    {
                        // write the logic implementation here


                        /*  ALgo Logic START here */
                        a1 = (imagePointer1[0] + imagePointer1[1] +
                             imagePointer1[2]) / 3;
                        a2 = (imagePointer2[0] + imagePointer2[1] +
                             imagePointer2[2]) / 3;

                        a3 = Math.Abs(a1 - a2);  // < 0? 0 : Math.Abs (a1-a2);
                        //Console.Write(a1 + "|" + a2 + "|" + a3);
                        //Console.Write(imagePointer1[0]+",");
                        imagePointer3[0] = (byte)a3;
                        imagePointer3[1] = (byte)a3;
                        imagePointer3[2] = (byte)a3;
                        imagePointer3[3] = imagePointer1[3];

                        //if (Math.Abs(a1 - a2) > 50) count++;

                        /*  ALgo Logic END here */

                        //4 bytes per pixel
                        imagePointer1 += 4;
                        imagePointer2 += 4;
                        imagePointer3 += 4;
                    }//end for j
                    //4 bytes per pixel
                    imagePointer1 += data1.Stride -
                                    (data1.Width * 4);
                    imagePointer2 += data2.Stride -
                                    (data2.Width * 4);
                    imagePointer3 += diffData.Stride -
                                    (diffData.Width * 4);
                }//end for i

                //Console.Write("Exiting Loop");
            }//end unsafe
            image1.UnlockBits(data1);
            image2.UnlockBits(data2);
            diffImage.UnlockBits(diffData);

            //if (count > 0.3 * (height * width)) return true; else return false;
            diffImage.Save("G:\\temp_new_Diff" + (fileCount) + ".png", ImageFormat.Png);
            return diffImage;
        }//end processImage

        static Bitmap convertToGray_roughalgo(Image img)
        {
            Bitmap image = (Bitmap)img;
            Bitmap returnMap = new Bitmap(image.Width, image.Height,
                                   PixelFormat.Format32bppArgb);
            BitmapData bitmapData1 = image.LockBits(new Rectangle(0, 0,
                                     image.Width, image.Height),
                                     ImageLockMode.ReadOnly,
                                     PixelFormat.Format32bppArgb);
            BitmapData bitmapData2 = returnMap.LockBits(new Rectangle(0, 0,
                                     returnMap.Width, returnMap.Height),
                                     ImageLockMode.ReadOnly,
                                     PixelFormat.Format32bppArgb);
            int a = 0;
            unsafe
            {
                byte* imagePointer1 = (byte*)bitmapData1.Scan0;
                byte* imagePointer2 = (byte*)bitmapData2.Scan0;
                for (int i = 0; i < bitmapData1.Height; i++)
                {
                    for (int j = 0; j < bitmapData1.Width; j++)
                    {
                        // write the logic implementation here
                        a = (imagePointer1[0] + imagePointer1[1] +
                             imagePointer1[2]) / 3;
                        imagePointer2[0] = (byte)a;
                        imagePointer2[1] = (byte)a;
                        imagePointer2[2] = (byte)a;
                        imagePointer2[3] = imagePointer1[3];
                        //4 bytes per pixel
                        imagePointer1 += 4;
                        imagePointer2 += 4;
                    }//end for j
                    //4 bytes per pixel
                    imagePointer1 += bitmapData1.Stride -
                                    (bitmapData1.Width * 4);
                    imagePointer2 += bitmapData1.Stride -
                                    (bitmapData1.Width * 4);
                }//end for i
            }//end unsafe
            returnMap.UnlockBits(bitmapData2);
            image.UnlockBits(bitmapData1);
            return returnMap;
        }//end processImage

    }
}