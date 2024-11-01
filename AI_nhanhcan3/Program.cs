using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

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

        public static string GetFirstCharacter(string input)
        {
            if (!string.IsNullOrEmpty(input))
            {
                return input[0].ToString(); // Trả về ký tự đầu tiên như chuỗi
            }
            return string.Empty; // Trả về chuỗi rỗng nếu input null hoặc empty
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
            List<string> dslList = new List<string>();
            string currentStateNext = start;

            while (priorityQueue.Count > 0)
            {
                var (currentF, currentState, currentG) = priorityQueue.Min;
                priorityQueue.Remove(priorityQueue.Min);

                if (currentState == goal)
                {
                    result.Add(new StepRecord
                    {
                        TT = currentState,
                        TTK = "",
                        K = 0,
                        G = currentG,
                        H = heuristicValues[currentState],
                        F = currentF,
                        DSL1 = "",
                        DSL = string.Join(",", dslList)
                    });
                    break;
                }

                var neighbors = graph.GetValueOrDefault(currentState, new List<Tuple<string, int>>());
                var sortedNeighbors = neighbors
                    .Select(n => (neighborState: n.Item1, edgeCost: n.Item2, newG: currentG + n.Item2, newF: currentG + n.Item2 + heuristicValues[n.Item1]))
                    .OrderBy(n => n.newF)
                    .ToList();

                Dictionary<string, int> neighborsF = new Dictionary<string, int>();

                neighborsF = sortedNeighbors.ToDictionary(n => n.neighborState, n => n.newF);

                var dsl1 = sortedNeighbors.Select(n => n.neighborState + neighborsF[n.neighborState]);
                dsl1 = dsl1.OrderBy(item => int.TryParse(new string(item.Skip(1).ToArray()), out int num) ? num : int.MaxValue)
                .ThenBy(item => item[0])
                .ToList();

                var startF = dsl1.First();

                if (!string.IsNullOrEmpty(startF))
                {
                    start = startF[0].ToString(); // Trả về ký tự đầu tiên như chuỗi
                }

                dslList.InsertRange(0, dsl1);

                foreach (var (neighborState, edgeCost, newG, newF) in sortedNeighbors)
                {

                    var newSteprecrd = new StepRecord
                    {
                        TT = "",
                        TTK = neighborState,
                        K = edgeCost,
                        G = newG,
                        H = heuristicValues[neighborState],
                        F = newF,
                        DSL1 = "",
                        DSL = ""
                    };

                    priorityQueue.Add((newF, neighborState, newG));
                    dslList.Remove(currentState);

                    if (!result.Any(c => c.TT == currentState))
                    {
                        if (dslList.Count > 0 && dslList[0].StartsWith(currentState))
                        {
                            var fistList = dslList[0];
                            dslList.Remove(fistList);
                        }
                        newSteprecrd.TT = currentState;
                        newSteprecrd.DSL1 = string.Join(",", dsl1);
                        newSteprecrd.DSL = string.Join(",", dslList);
                    }

                    result.Add(newSteprecrd);

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
