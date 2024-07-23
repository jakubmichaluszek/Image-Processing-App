using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CSharpLibraryWithAsm
{
    public class MedianFilterCSharpWithAsm
    {
        [DllImport("..\\dllASM\\dllASM.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetMedian(IntPtr values, int length);


        public void processImage(byte[] initialImage, int width, int height, byte[] finalImage)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Initialize R, G, B to 0
                    int r = 0, g = 0, b = 0;

                    // Get 3x3 neighborhood around (x, y)
                    List<int> valuesR = new List<int>();
                    List<int> valuesG = new List<int>();
                    List<int> valuesB = new List<int>();
                    for (int i = -1; i <= 1; i++)
                    {
                        for (int j = -1; j <= 1; j++)
                        {
                            int xCoord = x + j;
                            int yCoord = y + i;
                            if (xCoord >= 0 && xCoord < width && yCoord >= 0 && yCoord < height)
                            {
                                valuesR.Add(initialImage[(yCoord * width + xCoord) * 3 + 2]);
                                valuesG.Add(initialImage[(yCoord * width + xCoord) * 3 + 1]);
                                valuesB.Add(initialImage[(yCoord * width + xCoord) * 3]);
                            }
                        }
                    }
                    
                    // Get median value of each list and assign to R, G, B
                    valuesR.Sort();
                    int[] arrayR = valuesR.ToArray();
                    IntPtr ptrR = Marshal.AllocHGlobal(arrayR.Length * 4);
                    Marshal.Copy(arrayR, 0, ptrR, arrayR.Length);

                    valuesG.Sort();
                    int[] arrayG = valuesG.ToArray();
                    IntPtr ptrG = Marshal.AllocHGlobal(arrayG.Length * 4);
                    Marshal.Copy(arrayG, 0, ptrG, arrayG.Length);

                    valuesB.Sort();
                    int[] arrayB = valuesB.ToArray();
                    IntPtr ptrB = Marshal.AllocHGlobal(arrayB.Length * 4);
                    Marshal.Copy(arrayB, 0, ptrB, arrayB.Length);

                    r = GetMedian(ptrR, arrayR.Length);
                    g = GetMedian(ptrG, arrayG.Length);
                    b = GetMedian(ptrB, arrayB.Length);

                    Marshal.FreeHGlobal(ptrR);
                    Marshal.FreeHGlobal(ptrG);
                    Marshal.FreeHGlobal(ptrB);

                    // Store R, G, B values in final image
                    finalImage[(y * width + x) * 3 + 2] = (byte)r;
                    finalImage[(y * width + x) * 3 + 1] = (byte)g;
                    finalImage[(y * width + x) * 3] = (byte)b;
                }
            }
        }
    }
}
