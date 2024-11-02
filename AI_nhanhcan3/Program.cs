using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Xml.Linq;

namespace AI_nhanhcan3
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string inputFilePath = @"C:\Users\hunam\OneDrive\Máy tính\inputAI_nhanhcan.txt";
            string outputFilePath = @"C:\Users\hunam\OneDrive\Máy tính\result.txt";

            var graphData = ReadGraphData(inputFilePath);

            var result = BranchAndBoundSearch(graphData.Graph, graphData.HeuristicValues, graphData.Start, graphData.Goal);

            WriteResultToFile(result, outputFilePath, graphData.Start, graphData.Goal);

            Console.WriteLine($"Hoàn thành quá trình tìm kiếm. Kết quả được lưu vào {outputFilePath}");
        }

        public static GraphData ReadGraphData(string filePath)
        {
            GraphData graphData = new GraphData();
            try
            {
                string[] lines = File.ReadAllLines(filePath);
                bool readingEdges = false, readingHeuristics = false;

                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                        continue;

                    if (line.StartsWith("Start"))
                    {
                        graphData.Start = line.Split(' ')[1];
                        continue;
                    }

                    if (line.StartsWith("Goal"))
                    {
                        graphData.Goal = line.Split(' ')[1];
                        continue;
                    }

                    if (line.Contains("Edges"))
                    {
                        readingHeuristics = false;
                        readingEdges = true;
                        continue;
                    }

                    if (line.Contains("Heuristic"))
                    {
                        readingEdges = false;
                        readingHeuristics = true;
                        continue;
                    }

                    if (readingEdges)
                    {
                        var parts = line.Split(' ');
                        if (parts.Length != 3 || !int.TryParse(parts[2], out int cost))
                        {
                            Console.WriteLine($"Invalid edge format: {line}. Expected: 'StartNode EndNode Cost'");
                            continue;
                        }
                        var start = parts[0];
                        var end = parts[1];

                        if (!graphData.Graph.ContainsKey(start))
                            graphData.Graph[start] = new List<GraphDataState>();

                        graphData.Graph[start].Add(new GraphDataState(end, cost));
                    }

                    if (readingHeuristics)
                    {
                        var parts = line.Split(' ');
                        if (parts.Length != 2 || !int.TryParse(parts[1], out int heuristicValue))
                        {
                            Console.WriteLine($"Invalid heuristic format: {line}. Expected: 'Node HeuristicValue'");
                            continue;
                        }
                        graphData.HeuristicValues[parts[0]] = heuristicValue;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return null;
            }
            return graphData;
        }

        public static List<StepRecord> BranchAndBoundSearch(
           Dictionary<string, List<GraphDataState>> graph,
           Dictionary<string, int> heuristicValues,
           string start,
           string goal)
        {
            List<StepRecord> result = new List<StepRecord>();

            int tempCost = 0;

            var priorityQueue = new List<PriorityQueue>()
            {
                new PriorityQueue
                {
                    F = heuristicValues[start],
                    State = start,
                    G = 0
                }
            };

            while (priorityQueue.Count > 0)
            {
                var priorityQueuecurrentStateNext = priorityQueue.First();

                var currentF = priorityQueuecurrentStateNext.F;
                var currentState = priorityQueuecurrentStateNext.State;
                var currentG = priorityQueuecurrentStateNext.G;

                priorityQueue.Remove(priorityQueuecurrentStateNext);

                if (currentState == goal)
                {
                    tempCost = currentG;

                    var priorityFirstNext = priorityQueue.FirstOrDefault();

                    if (priorityFirstNext == null || tempCost <= priorityQueue.First()?.F)
                    {
                        result.Add(new StepRecord
                        {
                            TT = currentState,
                            TTK = string.Format("TTKT,tìm được đường đi ngắn nhất, độ dài {0}", tempCost),
                            K = 0,
                            H = heuristicValues[currentState],
                            G = currentG,
                            F = currentF,
                            DSL1 = $"",
                            DSL = string.Join(",", priorityQueue.Select(pqn => pqn.State + pqn.F))
                        });
                        break;
                    }
                    else
                    {
                        result.Add(new StepRecord
                        {
                            TT = currentState,
                            TTK = $"TTKT,tìm được đường đi tạm thời, độ dài {tempCost}",
                            K = 0,
                            H = heuristicValues[currentState],
                            G = currentG,
                            F = currentF,
                            DSL1 = "",
                            DSL = string.Join(",", priorityQueue.Select(pqn => pqn.State + pqn.F))
                        });
                    }
                }

                var neighbors = graph.GetValueOrDefault(currentState, new List<GraphDataState>());

                var sortedNeighbors = neighbors
                    .Select(n =>
                        new Neighbors
                        {
                            NeighborState = n.StateEnd,
                            EdgeCost = n.StateCost,
                            NewG = currentG + n.StateCost,
                            NewF = currentG + n.StateCost + heuristicValues[n.StateEnd]
                        }
                        )
                    .OrderByDescending(n => n.NewF)
                    .ToList();

                Dictionary<string, int> neighborsF = new Dictionary<string, int>();

                neighborsF = sortedNeighbors.ToDictionary(n => n.NeighborState, n => n.NewF);

                var dsl1 = sortedNeighbors.Select(n => n.NeighborState + neighborsF[n.NeighborState]);
                dsl1 = dsl1.OrderBy(item => int.TryParse(new string(item.Skip(1).ToArray()), out int num) ? num : int.MaxValue)
                .ThenBy(item => item[0])
                .ToList();

                var newSteprecrds = new List<StepRecord>();
                if (sortedNeighbors != null && sortedNeighbors.Count > 0)
                {
                    foreach (var Neighbors in sortedNeighbors)
                    {
                        var newSteprecrd = new StepRecord
                        {
                            TT = currentState,
                            TTK = Neighbors.NeighborState,
                            K = Neighbors.EdgeCost,
                            H = heuristicValues[Neighbors.NeighborState],
                            G = Neighbors.NewG,
                            F = Neighbors.NewF,
                            DSL1 = "",
                            DSL = ""
                        };

                        priorityQueue.Insert(0, new PriorityQueue
                        {
                            F = Neighbors.NewF,
                            State = Neighbors.NeighborState,
                            G = Neighbors.NewG
                        });

                        newSteprecrds.Add(newSteprecrd);
                    }
                }
                else
                {
                    var newSteprecrd = new StepRecord
                    {
                        TT = currentState,
                        TTK = "",
                        K = 0,
                        H = heuristicValues[currentState],
                        G = currentG,
                        F = currentF,
                        DSL1 = $"",
                        DSL = string.Join(",", priorityQueue.Select(pqn => pqn.State + pqn.F))
                    };

                    newSteprecrds.Add(newSteprecrd);
                }

                if (newSteprecrds != null && newSteprecrds.Count > 0)
                {
                    var stepRecord = newSteprecrds.FirstOrDefault();
                    stepRecord.DSL1 = string.Join(",", dsl1);
                    stepRecord.DSL = string.Join(",", priorityQueue.Select(pqn => pqn.State + pqn.F));
                }

                result.AddRange(newSteprecrds);

            }
            return result;
        }

        static string CenterText(string text, int width)
        {
            int padding = (width - text.Length) / 2;
            return text.PadLeft(text.Length + padding).PadRight(width);
        }

        public static void WriteResultToFile(List<StepRecord> result, string filePath, string start, string goal)
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                string format = "{0} | {1} | {2} | {3} | {4} | {5} | {6} | {7}";
                writer.WriteLine(format,
                    CenterText("TT", 3),
                    CenterText("TTK", 5),
                    CenterText("K", 5),
                    CenterText("H", 5),
                    CenterText("G", 5),
                    CenterText("F", 5),
                    "DSL1".PadRight(20),
                    "DSL".PadRight(20));

                int optimalCost = result.Last().G;
                var pathResult = FindPath(result, start, goal);

                List<string> path = new List<string>();

                string lastTT = null;

                foreach (var node in result)
                {
                    var ttFlag = node.TT;

                    if (node.TT == lastTT) 
                    {
                        node.TT = "";
                    }

                    if (node.TT == goal)
                    {
                        string formatGoal = "{0} | {1} | {2}";
                        writer.WriteLine(formatGoal,
                            CenterText(node.TT, 3),
                            CenterText(node.TTK, 60),
                            "DSL".PadRight(20));
                    }
                    else
                    {
                        writer.WriteLine(format,
                            CenterText(node.TT, 3),
                            CenterText(node.TTK, 5),
                            CenterText(node.K.ToString(), 5),
                            CenterText(node.H.ToString(), 5),
                            CenterText(node.G.ToString(), 5),
                            CenterText(node.F.ToString(), 5),
                            node.DSL1.PadRight(20),
                            node.DSL.PadRight(20));
                    }

                    lastTT = ttFlag;

                }


                writer.WriteLine($"Đường đi từ {start} đến {goal}");

                writer.WriteLine($"Đường đi từ {start} đến {goal}: {string.Join(" => ", pathResult)}");

                writer.WriteLine($"Chi phí đường đi: {optimalCost}");
            }
        }

        static List<string> FindPath(List<StepRecord> result, string start, string goal)
        {
            List<string> path = new List<string>();

            string currentGoal = goal;
            while (currentGoal != null)
            {
                path.Add(currentGoal);

                StepRecord record = result.FirstOrDefault(r => r.TTK == currentGoal);

                if (record == null || record.TT == start)
                {
                    path.Add(record?.TT);
                    break;
                }

                currentGoal = record.TT;
            }

            path.Reverse();
            return path;
        }

    }
}
