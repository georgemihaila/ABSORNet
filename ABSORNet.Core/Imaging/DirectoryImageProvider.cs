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
    /// Represents an image provider.
    /// </summary>
    /// <seealso cref="ABSORNet.Core.Imaging.IImageProvider" />
    public class DirectoryImageProvider : IImageProvider
    {
        /// <summary>
        /// Loads images from a directory.
        /// </summary>
        public IEnumerable<Bitmap> LoadImages(string directory)
        {
            if (!Directory.Exists(directory))
                throw new DirectoryNotFoundException(nameof(directory));
            foreach(var file in Directory.EnumerateFiles(directory))
            {
                yield return (Bitmap)Bitmap.FromFile(file);
            }
        }
    }
}
