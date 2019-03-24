using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABSORNet.Core.Imaging
{
    /// <summary>
    /// Represents an image preprocessor.
    /// </summary>
    public interface IImagePreprocessor
    {
        /// <summary>
        /// Preprocesses the images and outputs them to a directory.
        /// </summary>
        void ProcessImages(IEnumerable<Bitmap> images, string outputDirectory);
    }
}
