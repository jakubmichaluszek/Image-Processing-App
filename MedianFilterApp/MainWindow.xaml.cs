using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using Microsoft.Win32;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Reflection;
using System.Threading;
using System.Drawing.Imaging;
using Rectangle = System.Drawing.Rectangle;

namespace MedianFilterApp
{

    public partial class MainWindow : Window
    {
        //if photo is loaded correctly, set true
        bool photoLoaded = false;

        delegate void functionPointer(byte[] initialImg, int width, int height, byte[] finalImg);

        functionPointer dll;

        //Initial image bitmap object.
        private Bitmap initialImgBitMap;

        //Image after filtering bitmap object.
        private Bitmap finalImgBitMap;

        //Data of final img bit map
        private BitmapData bitmapData;


        //Initial image measurements.
        private int initialImageHeight;
        private int initialImageWidth;


        public MainWindow()
        {
            InitializeComponent();
        }

        void assemblyDllChoice(byte[] initialImgArray, int width, int height, byte[] finalImgArray)
        {
            //Zostawic sciezke pośrednia (nie bezposrednia)
            var dllFile = new FileInfo(@"C:\Users\kubam\source\repos\ImageProcessingApp\CSharpLibraryWithAsm\bin\Debug\CSharpLibraryWithAsm.dll");
            var DLL = Assembly.LoadFile(dllFile.FullName);
            var class1Type = DLL.GetType("CSharpLibraryWithAsm.MedianFilterCSharpWithAsm");
            dynamic c = Activator.CreateInstance(class1Type);
            c.filtruj(initialImgArray, width, height, finalImgArray);
        }

        void csharpDllChoice(byte[] initialImgArray, int width, int height, byte[] finalImgArray)
        {
            var dllFile = new FileInfo(@"C:\Users\kubam\source\repos\ImageProcessingApp\CSharpLibrary\bin\Debug\CSharpLibrary.dll");
            var DLL = Assembly.LoadFile(dllFile.FullName);
            var class1Type = DLL.GetType("CSharpLibrary.MedianFilterCSharp");
            dynamic c = Activator.CreateInstance(class1Type);
            c.filtruj(initialImgArray, width, height, finalImgArray);
        }

        //Function that loads photo from file explorer, saving photo data to initialImgBitMap object.
        void load_photo(object sender, RoutedEventArgs e)
        {
            OpenFileDialog explorer = new();
            //If opening explorer is true, then load chosen file. 
            if ((bool)explorer.ShowDialog())
            {
                //absolute path to choosen photo
                Uri photo_path = new Uri(explorer.FileName);
                
                //Taking extension of file to avoid problems with exceptions (eg. when user takes something that is not a photo)
                string extension = System.IO.Path.GetExtension(explorer.FileName);

                //If choosen file isn't bmp, then prompt a message. 
                if(extension == ".bmp")
                {
                    //Creating object of bitmap for initial photo.
                    initialImgBitMap = new Bitmap(explorer.FileName);

                    //creating object of bitmap for filtered photo.
                    finalImgBitMap = new Bitmap(initialImgBitMap);

                    //Displaying initial photo in the app. 
                    initial_image.Source = new BitmapImage(photo_path);

                    //Taking measurements from the bitmap 
                    initialImageHeight = initialImgBitMap.Height;
                    initialImageWidth = initialImgBitMap.Width;

                    //Photo is loaded correctly.
                    photoLoaded = true;
                }
                else
                {
                    MessageBox.Show("Aplikacja przyjmuje tylko zdjęcia w formacie .bmp!");
                }
            }
        }
        
        
        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void RadioButton_Click_1(object sender, RoutedEventArgs e)
        {
          
        }

        //Necessary imports for delete object. 
        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        public static extern bool DeleteObject([In] IntPtr hObject);
        private BitmapSource ConvertBitmapToBitmapSource(Bitmap bitmap)
        {
            // Create a BitmapSource object from the Bitmap object
            BitmapSource src = Imaging.CreateBitmapSourceFromHBitmap(
                bitmap.GetHbitmap(),
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            // Free the memory associated with the Bitmap object
            DeleteObject(bitmap.GetHbitmap());

            return src;
        }


        private void save_photo(object sender, RoutedEventArgs e)
        {
            SaveFileDialog explorer = new();
            explorer.Filter = "JPG (*.jpg)|*.jpg|PNG (*.png)|*.png";
            if ((bool)explorer.ShowDialog())
            {
                var fileName = explorer.FileName;
                var extension = System.IO.Path.GetExtension(explorer.FileName);
                final_save(fileName, extension);
            }
        }

        private void final_save(string fileName, string extension)
        {
            switch (extension.ToLower())
            {
                case ".jpg":
                    finalImgBitMap.Save(fileName, System.Drawing.Imaging.ImageFormat.Jpeg);
                    break;
                case ".png":
                    finalImgBitMap.Save(fileName, System.Drawing.Imaging.ImageFormat.Png);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(extension);
            }
        }

        private void start_filtering(object sender, RoutedEventArgs e)
        {

            //checking which library has been choosen.
            if ((bool)AssemblerButton.IsChecked)
            {
                dll = assemblyDllChoice;
            }
            else
            {
                dll = csharpDllChoice;
            }

            //Checking if photo is loaded correctly, if not - prompt a message.
            if (photoLoaded)
            {

                //Creating copy of an image object, 
                finalImgBitMap = initialImgBitMap.Clone(rect: new System.Drawing.Rectangle(0, 0, initialImageWidth, initialImageHeight), System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                //Bitmap data of finalImgBitMap. 
                bitmapData = finalImgBitMap.LockBits(new Rectangle(0, 0, initialImageWidth, initialImageHeight), ImageLockMode.ReadWrite, finalImgBitMap.PixelFormat);

                //Number of threads that user wants to use. 
                int numberOfThreads = (int)threads.Value;

                //Pointer on first byte of the image.
                IntPtr arrayPointer = bitmapData.Scan0;

                // Bytes needed for the whole row - bytes needed to keep pixels data.
                int offset = Math.Abs(bitmapData.Stride) - (initialImageWidth * 3);

                //RGB values of initial image. ( width * 3 ( R, G, B ) * Height) 
                byte[] initialImgRGB = new byte[bitmapData.Width * 3 * bitmapData.Height];

                //RGB values of final image. ( width * 3 ( R, G, B ) * Height)
                byte[] finalImgRGB = new byte[bitmapData.Width * 3 * bitmapData.Height];

                int end = initialImageHeight * Math.Abs(bitmapData.Stride);

                int position = 0;

                //Iterating over each of image row, and copying pixel data to initialImgRGB array.
                //Each row is width * 3, as there are 3 colors ( rgb ).
                for (int i = 0; i < end; i += initialImageWidth * 3)
                {
                    Marshal.Copy(arrayPointer + i, initialImgRGB, position, initialImageWidth * 3);
                    position += initialImageWidth * 3;
                    i += offset;
                }


                //Starting timer.
                var time = System.Diagnostics.Stopwatch.StartNew();

                //If number of threads is 1, then trigger function without multithreading.
                if(numberOfThreads == 1)
                {
                    dll(initialImgRGB, initialImageWidth, initialImageHeight, finalImgRGB);
                }
                else
                {
                    multithreading(numberOfThreads,initialImgRGB, finalImgRGB, initialImageWidth, initialImageHeight);
                }
                //Stopping timer.
                time.Stop();

                //Reading value of timer in milliseconds.
                var timeValue = time.ElapsedMilliseconds;

                //Displaying time in the gui.
                czas.Content = timeValue.ToString();
                position = 0;

                //Iterating over each of image row, and copying pixel data from finalImgRGB (filtered) array.
                //Each row is width * 3, as there are 3 colors ( rgb ).
                for (int i = 0; i < end; i += initialImageWidth * 3)
                {
                    Marshal.Copy(finalImgRGB, position, arrayPointer + i, initialImageWidth * 3);
                    position += initialImageWidth * 3;
                    i += offset;
                }

                //Converting final image bit map into a source,that can be displayed in gui.
                filtered_image.Source = ConvertBitmapToBitmapSource(finalImgBitMap);

            }
            else
            {
                MessageBox.Show("Nalezy wybrac zdjecie przed rozpoczeciem filtracji.");
            }
        }

        private void multithreading(int numberOfThreads, byte[] initialImgRGB, byte [] finalImgRGB, int width, int height)
        {
            if (numberOfThreads > height)
            {
                numberOfThreads = (int)(height / 3);
            }
            //needed to store rest of dividing height per threads, as it can be a float number, then we need to take into consideration the rest of image.
            int restOfDividing = height % numberOfThreads;

            //Rows of image per thread.
            int rowsPerThread = (int)(height / numberOfThreads);

            //array of bytes per thread.
            byte[][] threadArrays = new byte[numberOfThreads][];

            //Filtered array of bytes per thread.
            byte[][] threadArraysAfter = new byte[numberOfThreads][];

            //List of threads to create.
            Thread[] threadList = new Thread[numberOfThreads];

            //List of smallImage objects, that stores data needed to filter each of small image per thread.
            List<smallImage> dataList = new List<smallImage>();

            int threadArraysCounter = 0;

            
            for (int y = 0; y < (height - restOfDividing - 1); y = y + rowsPerThread)
            {
                int istart = y;
                int iend = y + rowsPerThread;
                if (restOfDividing > 0)
                {
                    iend++;
                    restOfDividing--;
                    y++;
                }
                if (istart > 0)
                {
                    istart--;
                }
                if (iend < height)
                {
                    iend++;
                }

                //size of each of the small images, that will be filtered by one thread.
                int size = iend * width * 3 - (istart * width * 3);
                threadArrays[threadArraysCounter] = new byte[size];
                int threadArrayIndex = threadArraysCounter;
                Array.Copy(initialImgRGB, istart * width * 3, threadArrays[threadArrayIndex], 0, size);
                threadArraysAfter[threadArrayIndex] = new byte[size];
                smallImage smallImage = new smallImage(threadArrays[threadArrayIndex], width, iend - istart, threadArraysAfter[threadArrayIndex], dll);

                threadList[threadArraysCounter] = new Thread(smallImage.run_filtering);


                threadArraysCounter++;
            }

            for (int i = 0; i < numberOfThreads; i++)
            {
                threadList[i].Start();
            }

            // Wait for all threads to complete
            for (int i = 0; i < numberOfThreads; i++)
            {
                threadList[i].Join();
            }


            //position of smaller image merging. 
            int finalImageIndex = 0;

            //Count all processed rows in filtered image.
            int totalProcessedRows = 0;
            for (int i = 0; i < numberOfThreads; i++)
            {
                threadList[i].Join();
                byte[] himage = threadArraysAfter[i];
                totalProcessedRows = totalProcessedRows + (himage.Length / (width * 3));
                if (i == 0)
                {
                    Array.Copy(himage, 0, finalImgRGB, finalImageIndex, himage.Length - (width * 3));
                    finalImageIndex = finalImageIndex + (himage.Length / (width * 3) - 1);
                }
                else
                {
                    Array.Copy(himage, width * 3, finalImgRGB, finalImageIndex * width * 3, himage.Length - (width * 3));
                    finalImageIndex = finalImageIndex + (himage.Length / (width * 3) - 2);
                }
            }
        }


        class smallImage
        {
            //Data of image.
            public byte[] initialImgRGB;
            public int width;
            public int height;
            public byte[] finalImgRGB;

            //pointer to DLL library that should be run.
            public functionPointer DLL;

            //constructor
            public smallImage(byte[] initialImgRGB, int width,  int height, byte[] finalImgRGB, functionPointer DLL)
            {
                this.initialImgRGB = initialImgRGB;
                this.width = width;
                this.height = height;
                this.finalImgRGB = finalImgRGB;
                this.DLL = DLL;
            }
            
            //Filtering method that will be executed.
            public void run_filtering(object data)
            {
                DLL(initialImgRGB, width, height, finalImgRGB);
            }
        }

    }
}
