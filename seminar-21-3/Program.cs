using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

#region Models

sealed class Node
{
    public char Name;
    public List<Node> Parents = new();
    public List<Node> Children = new();

    public int PriorityLevel;

    public int PriorityLex;
    public List<int> LexKey = new();
}

sealed class Graph
{
    public Dictionary<char, Node> Nodes = new();

    public Node Get(char name)
    {
        if (!Nodes.TryGetValue(name, out var node))
        {
            node = new Node { Name = name };
            Nodes[name] = node;
        }
        return node;
    }

    public void AddEdge(char from, char to)
    {
        var u = Get(from);
        var v = Get(to);

        if (!u.Children.Contains(v)) u.Children.Add(v);
        if (!v.Parents.Contains(u)) v.Parents.Add(u);
    }

    public IEnumerable<(char From, char To)> Edges()
    {
        foreach (var u in Nodes.Values.OrderBy(x => x.Name))
            foreach (var v in u.Children.OrderBy(x => x.Name))
                yield return (u.Name, v.Name);
    }
}

#endregion

#region Comparers

sealed class LexKeyComparerAscending : IComparer<List<int>>
{
    public int Compare(List<int> a, List<int> b)
    {
        int n = Math.Min(a.Count, b.Count);
        for (int i = 0; i < n; i++)
        {
            if (a[i] != b[i])
                return a[i].CompareTo(b[i]);
        }
        return a.Count.CompareTo(b.Count);
    }
}

#endregion

#region Algorithms

static class Algo
{
    public static void EnsureAllTasks(Graph g, IEnumerable<char> tasks)
    {
        foreach (var t in tasks)
            g.Get(t);
    }

    public static void ComputeLevelPriorities(Graph g)
    {
        foreach (var v in g.Nodes.Values)
            v.PriorityLevel = 0;

        int next = 1;

        var sinks = g.Nodes.Values
            .Where(v => v.Children.Count == 0)
            .OrderBy(v => v.Name)
            .ToList();

        foreach (var s in sinks)
            s.PriorityLevel = next++;

        int n = g.Nodes.Count;

        while (next <= n)
        {
            var eligible = g.Nodes.Values
                .Where(v => v.PriorityLevel == 0 && v.Children.Count > 0 && v.Children.All(c => c.PriorityLevel > 0))
                .Select(v => new { V = v, MinChild = v.Children.Min(c => c.PriorityLevel) })
                .ToList();

            if (eligible.Count == 0)
                throw new InvalidOperationException("Не удалось назначить уровневые приоритеты(возможен цикл).");

            int bestMin = eligible.Min(x => x.MinChild);

            var batch = eligible
                .Where(x => x.MinChild == bestMin)
                .Select(x => x.V)
                .OrderBy(v => v.Name)
                .ToList();

            foreach (var v in batch)
            {
                if (next > n) break;
                v.PriorityLevel = next++;
            }
        }
    }

    public static void ComputeLexPriorities(Graph g)
    {
        foreach (var v in g.Nodes.Values)
        {
            v.PriorityLex = 0;
            v.LexKey = new List<int>();
        }

        int next = 1;

        var sinks = g.Nodes.Values
            .Where(v => v.Children.Count == 0)
            .OrderBy(v => v.Name)
            .ToList();

        foreach (var s in sinks)
        {
            s.PriorityLex = next++;
            s.LexKey = new List<int>();
        }

        int n = g.Nodes.Count;
        var comparer = new LexKeyComparerAscending();

        while (next <= n)
        {
            var eligible = g.Nodes.Values
                .Where(v => v.PriorityLex == 0 && v.Children.Count > 0 && v.Children.All(c => c.PriorityLex > 0))
                .ToList();

            if (eligible.Count == 0)
                throw new InvalidOperationException("Не удалось назначить лексикографические приоритеты(возможен цикл)");

            foreach (var v in eligible)
                v.LexKey = v.Children.Select(c => c.PriorityLex).OrderByDescending(x => x).ToList();

            var bestKey = eligible
                .Select(v => v.LexKey)
                .OrderBy(k => k, comparer)
                .First();

            var batch = eligible
                .Where(v => comparer.Compare(v.LexKey, bestKey) == 0)
                .OrderBy(v => v.Name)
                .ToList();

            foreach (var v in batch)
            {
                if (next > n) break;
                v.PriorityLex = next++;
            }
        }
    }

    public static List<List<string>> BuildGanttByPriorityScan(Graph g, int processors, Func<Node, int> prioritySelector)
    {
        var timeline = Enumerable.Range(0, processors).Select(_ => new List<string>()).ToList();
        var done = new HashSet<Node>();

        var orderedAll = g.Nodes.Values
            .OrderByDescending(prioritySelector)
            .ThenBy(v => v.Name)
            .ToList();

        while (done.Count < g.Nodes.Count)
        {
            var chosen = new List<Node>();

            for (int p = 0; p < processors; p++)
            {
                Node pick = null;

                foreach (var v in orderedAll)
                {
                    if (done.Contains(v)) continue;
                    if (chosen.Contains(v)) continue;
                    if (!v.Parents.All(par => done.Contains(par))) continue;

                    pick = v;
                    break;
                }

                if (pick != null)
                {
                    chosen.Add(pick);
                    timeline[p].Add(pick.Name.ToString());
                }
                else
                {
                    timeline[p].Add(".");
                }
            }

            if (chosen.Count == 0)
                throw new InvalidOperationException("Ни одна задача не может быть запущена (возможен цикл/ошибка зависимостей).");

            foreach (var v in chosen)
                done.Add(v);
        }

        return timeline;
    }
}

#endregion

#region Output

static class Print
{
    public static void Edges(Graph g)
    {
        Console.WriteLine("Edges:");
        foreach (var (f, t) in g.Edges())
            Console.WriteLine($"{f} -> {t}");
        Console.WriteLine();
    }

    public static void Table(Graph g)
    {
        Console.WriteLine("Tasks:");
        Console.WriteLine("Task  PrLevel  LexKey(desc)        PrLex");
        foreach (var v in g.Nodes.Values.OrderBy(x => x.Name))
        {
            var key = v.LexKey.Count == 0 ? "[]" : "[" + string.Join(",", v.LexKey) + "]";
            Console.WriteLine($"{v.Name,4}  {v.PriorityLevel,9}  {key,-18}  {v.PriorityLex,7}");
        }
        Console.WriteLine();
    }

    public static void Gantt(List<List<string>> gantt, string title)
    {
        int T = gantt[0].Count;
        Console.WriteLine(title);
        Console.Write("t : ");
        for (int t = 0; t < T; t++) Console.Write($"{t,3}");
        Console.WriteLine();

        for (int p = 0; p < gantt.Count; p++)
        {
            Console.Write($"P{p + 1}: ");
            for (int t = 0; t < T; t++)
                Console.Write($"{gantt[p][t],3}");
            Console.WriteLine();
        }
        Console.WriteLine();
    }
}

#endregion

#region GraphViz

static class GraphViz
{
    public static void SaveDot(Graph g, string path)
    {
        var lines = new List<string>
    {
        "digraph G {",
        "  rankdir=LR;",
        "  node [shape=box, style=filled, fontname=\"Consolas\"];"
    };

        foreach (var v in g.Nodes.Values.OrderBy(x => x.Name))
        {
            string color;

            if (v.Children.Count == 0)
                color = "palegreen";      // стоки
            else if (v.Parents.Count == 0)
                color = "lightblue";      // истоки
            else
                color = "white";          // остальные

            lines.Add(
                $"  {v.Name} [fillcolor=\"{color}\", label=\"{v.Name}\\nL={v.PriorityLevel}, X={v.PriorityLex}\"];"
            );
        }

        foreach (var (f, t) in g.Edges())
            lines.Add($"  {f} -> {t};");

        lines.Add("}");
        File.WriteAllLines(path, lines);
    }


    public static bool TryRenderAndOpenPng(string dotPath, string pngPath)
    {
        try
        {
            var render = new ProcessStartInfo
            {
                FileName = "dot",
                Arguments = $"-Tpng \"{dotPath}\" -o \"{pngPath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using (var p = Process.Start(render))
                p.WaitForExit();

            if (!File.Exists(pngPath))
                return false;

            var open = new ProcessStartInfo
            {
                FileName = pngPath,
                UseShellExecute = true
            };

            Process.Start(open);
            return true;
        }
        catch
        {
            return false;
        }
    }
}

#endregion

#region Program

class Program
{
    static void Main()
    {
        var pred = new List<char> { 'A', 'A', 'B', 'C', 'D', 'D', 'E', 'E', 'E', 'F', 'F', 'G', 'G', 'H', 'I', 'J', 'J', 'J', 'K', 'K', 'K' };
        var succ = new List<char> { 'G', 'H', 'H', 'M', 'L', 'M', 'L', 'N', 'O', 'I', 'B', 'C', 'D', 'E', 'G', 'A', 'B', 'I', 'A', 'B', 'I' };

        var g = new Graph();
        for (int i = 0; i < pred.Count; i++)
            g.AddEdge(pred[i], succ[i]);

        Algo.EnsureAllTasks(g, pred.Concat(succ));

        Algo.ComputeLevelPriorities(g);
        Algo.ComputeLexPriorities(g);

        Print.Edges(g);
        Print.Table(g);

        var ganttLevel = Algo.BuildGanttByPriorityScan(g, 2, v => v.PriorityLevel);
        var ganttLex = Algo.BuildGanttByPriorityScan(g, 2, v => v.PriorityLex);

        Print.Gantt(ganttLevel, "GANTT: LEVEL STRATEGY (priority scan, 2 processors, unit tasks)");
        Print.Gantt(ganttLex, "GANTT: LEX STRATEGY (priority scan, 2 processors, unit tasks)");

        var dotPath = Path.Combine(Environment.CurrentDirectory, "graph.dot");
        var pngPath = Path.Combine(Environment.CurrentDirectory, "graph.png");

        GraphViz.SaveDot(g, dotPath);
        Console.WriteLine($"DOT saved: {dotPath}");

        if (GraphViz.TryRenderAndOpenPng(dotPath, pngPath))
            Console.WriteLine("PNG generated and opened.");
        else
            Console.WriteLine("PNG not generated (dot not found). You can render manually: dot -Tpng graph.dot -o graph.png");
    }
}

#endregion
