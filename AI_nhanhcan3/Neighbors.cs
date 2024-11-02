using System;
using System.Collections.Generic;
using System.Text;

namespace AI_nhanhcan3
{
    public class Neighbors
    {
        public string NeighborState { get; set; }
        public int EdgeCost { get; set; }
        public int NewG { get; set; }
        public int NewF { get; set; }
    }
}
