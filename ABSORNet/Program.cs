using ABSORNet.Core.Imaging;
using ABSORNet.Core.Network;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Complex;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABSORNet
{
    class Program
    {
        internal static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Source directory needed.");
                return;
            }
            string res = args[0];
            string sourceDirectory = $"Resources/{res}";
            string destTempDirectory = $"temp/{res}_preprocessed";

            bool skipPreprocessing = false;
            if (!skipPreprocessing)
            {
                IImageProvider rawImageProvider = new DirectoryImageProvider();
                IImagePreprocessor preprocessor = new ImagePreprocessor();
                preprocessor.ProcessImages(rawImageProvider.LoadImages(sourceDirectory), destTempDirectory);
            }
            Network network = new Network();
            var pictureDirectories = Directory.EnumerateDirectories(destTempDirectory);
            Directory.CreateDirectory("results");
            Directory.CreateDirectory(destTempDirectory);
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            for (int i = 0; i < int.MaxValue; i++)
            {
                stopwatch.Restart();
                network.Layers.Add(new Layer()
                {
                    Order = i,
                    RValues = new double[255, 255],
                    GValues = new double[255, 255],
                    BValues = new double[255, 255],
                });
                Console.Write("Layer " + i + ": ");
                Console.Write("init, ");
                for (int x = 0; x < network.Layers[i].X; x++)
                    for (int y = 0; y < network.Layers[i].Y; y++)
                    {
                        network.Layers[i].RValues[x, y] = 0;
                        network.Layers[i].GValues[x, y] = 0;
                        network.Layers[i].BValues[x, y] = 0;
                    }
                List<Bitmap> images = new List<Bitmap>();
                Console.Write("load, ");
                foreach (var directory in pictureDirectories)
                {
                    if (!File.Exists(directory + "/" + i + ".jpg"))
                    {
                        Console.WriteLine("Analysis completed.");
                        File.WriteAllText("analysis.json", JsonConvert.SerializeObject(network, Formatting.Indented));
                       
                        return;
                    }
                    images.Add((Bitmap)Bitmap.FromFile(directory + "/" + i + ".jpg"));
                }
                Console.Write("process... ");
                for (int x = 0; x < Math.Pow(2, i); x++)
                    for (int y = 0; y < Math.Pow(2, i); y++)
                    {
                        int[] allRpixels = images.Select(o => (int)o.GetPixel(x, y).R).ToArray();
                        Array.Sort(allRpixels);
                        DoMagicWith(ref network.Layers[i].RValues, allRpixels);

                        int[] allGpixels = images.Select(o => (int)o.GetPixel(x, y).G).ToArray();
                        Array.Sort(allGpixels);
                        DoMagicWith(ref network.Layers[i].GValues, allGpixels);

                        int[] allBpixels = images.Select(o => (int)o.GetPixel(x, y).B).ToArray();
                        Array.Sort(allBpixels);
                        DoMagicWith(ref network.Layers[i].BValues, allBpixels);
                    }
                var layer = network.Layers[i];
                Bitmap b = new Bitmap(255 * 4, 255 * 4);
                double maxR = layer.RValues.Cast<double>().Max();
                double maxG = layer.GValues.Cast<double>().Max();
                double maxB = layer.BValues.Cast<double>().Max();

                double minR = layer.RValues.Cast<double>().Min();
                double minG = layer.GValues.Cast<double>().Min();
                double minB = layer.BValues.Cast<double>().Min();

                for (int x = 0; x < b.Width; x++)
                    for (int y = 0; y < b.Height; y++)
                        b.SetPixel(x, y, Color.FromArgb(
                            (int)LinearProjection(minR, maxR, 0, 255, layer.RValues[x / 4, y / 4]),
                            (int)LinearProjection(minG, maxG, 0, 255, layer.GValues[x / 4, y / 4]),
                            (int)LinearProjection(minB, maxB, 0, 255, layer.BValues[x / 4, y / 4])));
                b.Save("results/" + network.Layers.IndexOf(layer) + ".png");
                stopwatch.Stop();
                Console.WriteLine($"export to results/{network.Layers.IndexOf(layer)}.png ({stopwatch.Elapsed.TotalSeconds}s)");
            }

        }

        private static void DoMagicWith(ref double[,] arr, int[] sortedValues)
        {
            for (int x = 0; x < sortedValues.Length - 1; x++)
            {
                for (int y = x + 1; y < sortedValues.Length - 1; y++)
                {
                    int howMany = HowManyBetween(sortedValues, sortedValues[x], sortedValues[y]);

                    for (int i = sortedValues[x]; i < sortedValues[x + 1]; i++)
                    {
                        for (int j = sortedValues[y]; j < sortedValues[y + 1]; j++)
                        {
                            double val = LinearProjection(
                                0, sortedValues.Length,
                                0, (double)1 / ((double)sortedValues.Length * 1 * sortedValues.Length),
                                howMany);
                            arr[i, j] += val;
                            arr[j, i] += val;
                        }
                    }

                }
            }
        }

        private static int HowManyBetween(int[] arr, int min, int max)
        {
            return arr.ToList().Where(q => q >= min && q <= max).Count();
        }

        public static double LinearProjection(double sourceIntervalMinimum, double sourceIntervalMaximum, double destinationIntervalMinimum, double destinationIntervalMaximum, double value)
        {
            if (sourceIntervalMinimum >= sourceIntervalMaximum) throw new InvalidOperationException("Source interval improperly defined. The minimum value must be smaller than the maximum value.");
            if (sourceIntervalMaximum <= sourceIntervalMinimum) throw new InvalidOperationException("Source interval improperly defined. The maximum value must be bigger than the minimum value.");

            if (destinationIntervalMinimum >= destinationIntervalMaximum) throw new InvalidOperationException("Destination interval improperly defined. The minimum value must be smaller than the maximum value.");
            if (destinationIntervalMaximum <= destinationIntervalMinimum) throw new InvalidOperationException("Destination interval improperly defined. The maximum value must be bigger than the minimum value.");

            if (value > sourceIntervalMaximum || value < sourceIntervalMinimum) throw new ArgumentOutOfRangeException("The value must be contained in the source interval. The minimum value must be smaller than the maximum value.");

            return (value * (destinationIntervalMinimum - destinationIntervalMaximum) + (sourceIntervalMinimum * destinationIntervalMaximum) - (destinationIntervalMinimum * sourceIntervalMaximum)) / (sourceIntervalMinimum - sourceIntervalMaximum);
        }
    }
}
