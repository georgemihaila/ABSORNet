using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABSORNet.Core.Imaging
{
    /// <summary>
    /// Represents an image preprocessor.
    /// </summary>
    public class ImagePreprocessor : IImagePreprocessor
    {
        public void ProcessImages(IEnumerable<Bitmap> images, string outputDirectory)
        {
            int c = -1;
            Console.Write("Resizing {0} image{1}...", images.Count(), (images.Count() != 1) ? "s" : string.Empty);
            foreach (var image in images)
            {
                c++;
                Directory.CreateDirectory(outputDirectory + "/" + c.ToString());
                Console.WriteLine();
                Console.Write($"({c + 1}) ");
                for (int i = ClosestLargestPowerOfTwo(image.Width, image.Height); i >= 0; i--)
                {
                    Console.Write("{0}{1}", i, (i != 0) ? ", " : string.Empty);
                    image.Scale((int)Math.Pow(2, i), (int)Math.Pow(2, i)).Save(outputDirectory + "/" + c + "/" + i + ".jpg");
                }
            }
            Console.WriteLine();
        }

        private int ClosestLargestPowerOfTwo(int a, int b)
        {
            if (a > b)
                b = a;
            int c = 1;
            int order = 0;
            while (c < b)
            {
                c *= 2;
                order++;
            }
            return order;
        }
    }
}
