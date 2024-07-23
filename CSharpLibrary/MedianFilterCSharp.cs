using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpLibrary
{
    public class MedianFilterCSharp
    {
        //filtrowanie tablicy i zapisanie wyniku w drugiej tablicy
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
                    r = getMedian(valuesR);
                    g = getMedian(valuesG);
                    b = getMedian(valuesB);

                    // Store R, G, B values in final image
                    finalImage[(y * width + x) * 3 + 2] = (byte)r;
                    finalImage[(y * width + x) * 3 + 1] = (byte)g;
                    finalImage[(y * width + x) * 3] = (byte)b;
                }
            }
        }

        // Returns the median value of a list of integers
        private int getMedian(List<int> values)
        {
            values.Sort();
            if (values.Count % 2 == 1)
            {
                return values[(values.Count - 1) / 2];
            }
            else
            {
                return (values[values.Count / 2] + values[values.Count / 2 - 1]) / 2;
            }
        }
    }
}

