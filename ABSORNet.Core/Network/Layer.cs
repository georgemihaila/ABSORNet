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

        public double[,] RValues;

        public double[,] GValues;

        public double[,] BValues;

        public int X => RValues.GetLength(0);

        public int Y => RValues.GetLength(1);
    }
}
