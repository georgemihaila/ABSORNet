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
        internal static async Task Main(string[] args)
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
            if (args.Length > 1 && bool.Parse(args[1]))
                skipPreprocessing = true;
            if (!skipPreprocessing)
            {
                IImageProvider rawImageProvider = new DirectoryImageProvider();
                IImagePreprocessor preprocessor = new ImagePreprocessor();
                preprocessor.ProcessImages(rawImageProvider.LoadImages(sourceDirectory), destTempDirectory);
            }
            int NUM_LAYERS = 8;
            Network network = new Network(NUM_LAYERS);
            var pictureDirectories = Directory.EnumerateDirectories(destTempDirectory);
            Directory.CreateDirectory("results");
            Directory.CreateDirectory(destTempDirectory);
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

            int NUM_THREADS = 4;
            for (int i = 0; i < NUM_LAYERS; i += NUM_THREADS)
            {
                Task[] splitTask = new Task[NUM_THREADS];
                for(int t = 0; t < NUM_THREADS; t++)
                {
                    int layerIndex = t + i;
                    splitTask[t] = Task.Run(() => DoAnalysis(layerIndex, pictureDirectories, ref network));
                }
                await Task.WhenAll(splitTask);
            }

        }

        private static void DoAnalysis(int i, IEnumerable<string> pictureDirectories, ref Network network)
        {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            var layerDir = "results/layer " + i;
            Directory.CreateDirectory(layerDir);
            int order = (int)Math.Pow(2, i);
            Console.Write("Layer " + i + ": ");
            //Init layer here in order to not use a huge amount of memory upfront.
            for (int x = 0; x < order; x++)
                for (int y = 0; y < order; y++)
                    network.Layers[i][x, y] = new Layer();
            List<Bitmap> images = new List<Bitmap>();
            Console.Write("load, ");
            foreach (var directory in pictureDirectories)
            {
                if (!File.Exists(directory + "/" + i + ".jpg"))
                {
                    Console.WriteLine("Analysis completed.");
                    File.WriteAllText(layerDir + "/layer.json", JsonConvert.SerializeObject(network.Layers[i], Formatting.Indented));
                    return;
                }
                images.Add((Bitmap)Bitmap.FromFile(directory + "/" + i + ".jpg"));
            }
            Console.Write("process... ");
            for (int x = 0; x < order; x++)
                for (int y = 0; y < order; y++)
                {
                    int[] allRpixels = images.Select(o => (int)o.GetPixel(x, y).R).ToArray();
                    Array.Sort(allRpixels);
                    DoMagicWith(ref network.Layers[i][x, y].RValues, allRpixels);

                    int[] allGpixels = images.Select(o => (int)o.GetPixel(x, y).G).ToArray();
                    Array.Sort(allGpixels);
                    DoMagicWith(ref network.Layers[i][x, y].GValues, allGpixels);

                    int[] allBpixels = images.Select(o => (int)o.GetPixel(x, y).B).ToArray();
                    Array.Sort(allBpixels);
                    DoMagicWith(ref network.Layers[i][x, y].BValues, allBpixels);
                }
            for (int x = 0; x < order; x++)
            {
                for (int y = 0; y < order; y++)
                {
                    Bitmap b = new Bitmap(255, 255);
                    var layer = network.Layers[i][x, y];
                    double maxR = layer.RValues.Cast<double>().Max();
                    double maxG = layer.GValues.Cast<double>().Max();
                    double maxB = layer.BValues.Cast<double>().Max();

                    double minR = layer.RValues.Cast<double>().Min();
                    double minG = layer.GValues.Cast<double>().Min();
                    double minB = layer.BValues.Cast<double>().Min();

                    for (int xt = 0; xt < b.Width; xt++)
                        for (int yt = 0; yt < b.Height; yt++)
                            b.SetPixel(xt, yt, Color.FromArgb(

                                (minR != maxR) ?
                                (int)LinearProjection(minR, maxR, 0, 255, layer.RValues[xt, yt])
                                : 0,

                                (minG != maxG) ?
                                (int)LinearProjection(minG, maxG, 0, 255, layer.GValues[xt, yt])
                                : 0,

                                (minB != maxB) ?
                                (int)LinearProjection(minB, maxB, 0, 255, layer.BValues[xt, yt])
                                : 0));
                    var filename = string.Format("{2}/freq_{0}x{1}.png", x, y, layerDir);
                    b.Save(filename);
                    Console.WriteLine($"export to {filename}");
                }
            }
            stopwatch.Stop();
            Console.WriteLine($"Layer {i} analysis completed in ({stopwatch.Elapsed.TotalSeconds}s)");
            File.WriteAllText(layerDir + "/layer.json", JsonConvert.SerializeObject(network.Layers[i], Formatting.Indented));
            //Replace current layer with dummy layer in order to free memory
            Layer[,] dummy = new Layer[0, 0];
            network.Layers.RemoveAt(i);
            network.Layers.Insert(i, dummy);
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
