using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABSORNet.Core.Network
{
    public class Layer
    {
        public int Order { get; set; }

        public double[,] RValues = new double[255, 255];

        public double[,] GValues = new double[255, 255];

        public double[,] BValues = new double[255, 255];

        public int X => RValues.GetLength(0);

        public int Y => RValues.GetLength(1);
    }
}
