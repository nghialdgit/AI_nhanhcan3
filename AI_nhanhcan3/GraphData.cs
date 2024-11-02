using System;
using System.Collections.Generic;

namespace AI_nhanhcan3
{
    public class GraphData
    {
        public Dictionary<string, List<GraphDataState>> Graph { get; set; }
        public Dictionary<string, int> HeuristicValues { get; set; }
        public string Start { get; set; }
        public string Goal { get; set; }
        public GraphData()
        {
            Graph = new Dictionary<string, List<GraphDataState>>();
            HeuristicValues = new Dictionary<string, int>();
        }
    }

    public class GraphDataState
    {
        public string StateEnd { get; set; }
        public int StateCost { get; set; }
        public GraphDataState()
        {
            
        }

        public GraphDataState(string stateEnd, int stateCost)
        {
            StateEnd = stateEnd;
            StateCost = stateCost;
        }
    }

}
