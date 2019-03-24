using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABSORNet.Core.Imaging
{
    /// <summary>
    /// Represents an image provider.
    /// </summary>
    public interface IImageProvider
    {
        /// <summary>
        /// Loads images from a directory.
        /// </summary>
        IEnumerable<Bitmap> LoadImages(string directory);
    }
}
