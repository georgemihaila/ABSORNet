using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABSORNet.Core.Network
{
    public class Network
    {
        public Network(int depth)
        {
            for (int i = 0; i < depth; i++)
            {
                int order = (int)Math.Pow(2, i);
                Layers.Add(new Layer[order, order]);
            }
        }

        public List<Layer[,]> Layers { get; set; } = new List<Layer[,]>();
    }
}
