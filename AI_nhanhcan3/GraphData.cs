using System;
using System.Collections.Generic;

namespace AI_nhanhcan3
{
    public class GraphData
    {
        public Dictionary<string, List<Tuple<string, int>>> Graph { get; set; }
        public Dictionary<string, int> HeuristicValues { get; set; }
        public string Start { get; set; }
        public string Goal { get; set; }
        public GraphData()
        {
            Graph = new Dictionary<string, List<Tuple<string, int>>>();
            HeuristicValues = new Dictionary<string, int>();
        }
    }
}
