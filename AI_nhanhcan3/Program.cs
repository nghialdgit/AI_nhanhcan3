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

        public static List<StepRecord> BranchAndBoundSearch(
    Dictionary<string, List<Tuple<string, int>>> graph,
    Dictionary<string, int> heuristicValues,
    string start,
    string goal)
        {
            List<StepRecord> result = new List<StepRecord>();

            // Initialize list L with the start state
            var L = new List<string> { start };
            int cost = int.MaxValue; // Initialize cost to a very large value

            // Dictionary to store the best g values for each state
            Dictionary<string, int> visited = new Dictionary<string, int>
    {
        { start, 0 }
    };

            while (L.Count > 0) // Main loop
            {
                // Remove the state at the front of the list L
                string currentState = L[0];
                L.RemoveAt(0);

                // Check if the current state is the goal state
                int currentG = visited[currentState]; // g(u)
                if (currentState == goal)
                {
                    // Update cost if the current path is better
                    if (currentG <= cost)
                    {
                        cost = currentG;

                        result.Add(new StepRecord
                        {
                            TT = currentState, // Updated to reflect the current state
                            TTK = "", // No adjacent state since it's the goal
                            K = 0,
                            G = currentG,
                            H = heuristicValues[currentState],
                            F = currentG + heuristicValues[currentState], // F = G + H
                            DSL1 = "",
                            DSL = string.Join(",", result.Select(r => r.TTK))
                        });
                    }
                    continue; // Go back to the beginning of the loop
                }

                // Expand the current state
                var neighbors = graph.GetValueOrDefault(currentState, new List<Tuple<string, int>>());
                var L1 = new List<(string state, int g, int f)>(); // Temporary list for next states

                foreach (var neighbor in neighbors)
                {
                    string neighborState = neighbor.Item1;
                    int edgeCost = neighbor.Item2;
                    int newG = currentG + edgeCost; // Calculate g(v)
                    int newF = newG + heuristicValues[neighborState]; // Calculate f(v)

                    // Check if we should add the neighbor to L1
                    if (!visited.ContainsKey(neighborState) || newG < visited[neighborState])
                    {
                        visited[neighborState] = newG; // Update the best g value
                        L1.Add((neighborState, newG, newF)); // Add the neighbor to L1

                        result.Add(new StepRecord
                        {
                            TT = currentState, // Current state
                            TTK = neighborState, // Neighbor state
                            K = edgeCost,
                            G = newG,
                            H = heuristicValues[neighborState],
                            F = newF,
                            DSL1 = string.Join(",", neighbors.Select(n => n.Item1)),
                            DSL = string.Join(",", result.Select(r => r.TTK))
                        });
                    }
                }

                // Sort L1 by f(v) and add it to the front of L
                L1.Sort((a, b) => a.f.CompareTo(b.f));
                foreach (var item in L1)
                {
                    L.Insert(0, item.state); // Insert states from L1 to L
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
