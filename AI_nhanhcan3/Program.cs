using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AI_nhanhcan3
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string inputFilePath = @"C:\Users\phuongtk\Desktop\inputAI_nhanhcan.txt";
            string outputFilePath = @"C:\Users\phuongtk\Desktop\result.txt";

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
                        readingEdges = true;
                        continue;
                    }

                    if (line.Contains("heuristic"))
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
                            graphData.Graph[start] = new List<Tuple<string, int>>();

                        graphData.Graph[start].Add(new Tuple<string, int>(end, cost));
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
            Dictionary<string, List<Tuple<string, int>>> graph,
            Dictionary<string, int> heuristicValues,
            string start,
            string goal)
        {
            List<StepRecord> result = new List<StepRecord>();
            var priorityQueue = new SortedSet<(int f, string state, int g)>(
     Comparer<(int, string, int)>.Create((a, b) => a.Item1 == b.Item1 ? a.Item2.CompareTo(b.Item2) : a.Item1.CompareTo(b.Item1))
 );

            HashSet<string> visited = new HashSet<string>();
            priorityQueue.Add((heuristicValues[start], start, 0));

            while (priorityQueue.Count > 0)
            {
                var (currentF, currentState, currentG) = priorityQueue.Min;
                priorityQueue.Remove(priorityQueue.Min);

                if (currentState == goal)
                {
                    result.Add(new StepRecord
                    {
                        TT = result.Count.ToString(),
                        TTK = currentState,
                        K = 0,
                        G = currentG,
                        H = heuristicValues[currentState],
                        F = currentF,
                        DSL1 = "",
                        DSL = string.Join(",", result.Select(r => r.TTK))
                    });
                    break;
                }

                if (!visited.Contains(currentState))
                {
                    visited.Add(currentState);
                    var neighbors = graph.GetValueOrDefault(currentState, new List<Tuple<string, int>>());

                    foreach (var neighbor in neighbors)
                    {
                        string neighborState = neighbor.Item1;
                        int edgeCost = neighbor.Item2;
                        int newG = currentG + edgeCost;
                        int newF = newG + heuristicValues[neighborState];

                        if (!visited.Contains(neighborState))
                        {
                            priorityQueue.Add((newF, neighborState, newG));
                            result.Add(new StepRecord
                            {
                                TT = result.Count.ToString(),
                                TTK = neighborState,
                                K = edgeCost,
                                G = newG,
                                H = heuristicValues[neighborState],
                                F = newF,
                                DSL1 = string.Join(",", neighbors.Select(n => n.Item1)),
                                DSL = string.Join(",", result.Select(r => r.TTK))
                            });
                        }
                    }
                }
            }

            return result;
        }

        public static void WriteResultToFile(List<StepRecord> result, string filePath, string start, string goal)
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                string format = "{0,-5} | {1,-5} | {2,-3} | {3,-3} | {4,-3} | {5,-3} | {6,-15} | {7,-10}";
                writer.WriteLine(format, "TT", "TTK", "K", "G", "H", "F", "DSL1", "DSL");

                foreach (var node in result)
                {
                    writer.WriteLine(format, node.TT, node.TTK, node.K, node.G, node.H, node.F, node.DSL1, node.DSL);
                }

                writer.WriteLine($"Đường đi từ {start} đến {goal}");
                int optimalCost = result.Last().G;
                writer.WriteLine($"Chi phí đường đi: {optimalCost}");
            }
        }
    }
}
