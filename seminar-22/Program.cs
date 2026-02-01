using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

#region GraphViz

static class GraphViz
{
    public static void SaveDot(
        List<int>[] g,
        int[] color,
        int[] match,
        string path,
        string title)
    {
        var used = UsedVertices(g);

        var lines = new List<string>
        {
            "graph G {",
            "  rankdir=LR;",
            "  graph [bgcolor=\"white\", fontname=\"Consolas\", labelloc=\"t\"];",
            $"  label=\"{Esc(title)}\";",
            "  node [shape=circle, style=filled, fontname=\"Consolas\"];",
            "  edge [color=\"#777777\"];"
        };

        for (int v = 0; v < g.Length; v++)
        {
            if (!used[v]) continue;
            string fill = color[v] == 0 ? "lightblue" : "palegreen";
            string m = match[v] == -1 ? "-" : Label(match[v]);
            lines.Add($"  v{v} [fillcolor=\"{fill}\", label=\"{Label(v)}\\nM={m}\"];");
        }

        var seen = new HashSet<(int, int)>();
        for (int a = 0; a < g.Length; a++)
        {
            if (!used[a]) continue;
            foreach (int b in g[a])
            {
                if (!used[b]) continue;
                int u = Math.Min(a, b), v = Math.Max(a, b);
                if (!seen.Add((u, v))) continue;

                bool inMatching = match[u] == v || match[v] == u;
                if (inMatching)
                    lines.Add($"  v{u} -- v{v} [color=\"red\", penwidth=3.0];");
                else
                    lines.Add($"  v{u} -- v{v};");
            }
        }

        lines.Add("}");
        File.WriteAllLines(path, lines);
    }

    public static bool TryRenderAndOpenPng(string dotPath, string pngPath)
    {
        try
        {
            var p = Process.Start(new ProcessStartInfo
            {
                FileName = "dot",
                Arguments = $"-Tpng \"{dotPath}\" -o \"{pngPath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            });
            p!.WaitForExit();

            if (!File.Exists(pngPath)) return false;
            Process.Start(new ProcessStartInfo { FileName = pngPath, UseShellExecute = true });
            return true;
        }
        catch { return false; }
    }

    static bool[] UsedVertices(List<int>[] g)
    {
        var used = new bool[g.Length];
        for (int i = 0; i < g.Length; i++)
            if (g[i].Count > 0)
            {
                used[i] = true;
                foreach (var j in g[i]) used[j] = true;
            }
        return used;
    }

    static string Label(int v)
    {
        if (v < 26) return ((char)('A' + v)).ToString();
        return (v - 26 + 1).ToString();
    }

    static string Esc(string s) => s.Replace("\\", "\\\\").Replace("\"", "\\\"");
}

#endregion

#region Parsing

static class Parse
{
    public static List<(char L, int[] R)> ReadAdjFromText(string text)
    {
        var res = new List<(char, int[])>();

        foreach (var raw in text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries))
        {
            var line = new string(raw.Where(c => !char.IsWhiteSpace(c)).ToArray());
            if (line.Length == 0) continue;

            char L = char.ToUpperInvariant(line[0]);

            int p1 = line.IndexOf('(');
            int p2 = line.IndexOf(')');
            var inside = (p1 >= 0 && p2 > p1) ? line.Substring(p1 + 1, p2 - p1 - 1) : "";

            var nums = inside.Length == 0
                ? Array.Empty<int>()
                : inside.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(int.Parse)
                        .Distinct()
                        .ToArray();

            res.Add((L, nums));
        }

        return res;
    }

    public static List<(char L, int[] R)> ReadAdjInteractive()
    {
        var res = new List<(char, int[])>();

        while (true)
        {
            var raw = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(raw)) break;

            var line = new string(raw.Where(c => !char.IsWhiteSpace(c)).ToArray());
            if (line.Length == 0) continue;

            char L = char.ToUpperInvariant(line[0]);

            int p1 = line.IndexOf('(');
            int p2 = line.IndexOf(')');
            if (p1 < 0 || p2 <= p1) throw new FormatException("Expected A(1,2,4)");

            var inside = line.Substring(p1 + 1, p2 - p1 - 1);

            var nums = inside.Length == 0
                ? Array.Empty<int>()
                : inside.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(int.Parse)
                        .Distinct()
                        .ToArray();

            res.Add((L, nums));
        }

        return res;
    }

    public static List<(char L, int R)> ParsePairs(string s)
    {
        var res = new List<(char, int)>();
        if (string.IsNullOrWhiteSpace(s)) return res;

        foreach (var raw in s.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries))
        {
            var t = raw.Trim().Replace(" ", "");
            char L = char.ToUpperInvariant(t[0]);
            int i = 1;
            if (i < t.Length && (t[i] == '-' || t[i] == ':')) i++;
            int R = int.Parse(t.Substring(i));
            res.Add((L, R));
        }

        return res;
    }
}

#endregion

#region Matching

static class Matching
{
    static int L(char c) => c - 'A';
    static int R(int k) => 26 + (k - 1);

    public static List<int>[] BuildGraph(
        List<(char L, int[] R)> adj,
        out int[] color,
        out bool[] rightUsed)
    {
        int maxR = 0;
        foreach (var (_, rs) in adj)
            foreach (var r in rs)
                maxR = Math.Max(maxR, r);

        rightUsed = new bool[maxR + 1];
        foreach (var (_, rs) in adj)
            foreach (var r in rs)
            {
                if (r <= 0) throw new ArgumentException("Right vertex must be >= 1");
                rightUsed[r] = true;
            }

        int n = 26 + maxR;
        var g = new List<int>[n];
        for (int i = 0; i < n; i++) g[i] = new List<int>();

        foreach (var (l, rs) in adj)
        {
            int li = L(l);
            if (li < 0 || li >= 26) throw new ArgumentException("Left vertex must be A..Z");
            foreach (var r in rs)
            {
                int v = R(r);
                g[li].Add(v);
                g[v].Add(li);
            }
        }

        if (!IsBipartite(g, out color))
            throw new Exception("Graph is not bipartite");

        return g;
    }

    public static bool IsBipartite(List<int>[] g, out int[] color)
    {
        int n = g.Length;
        color = new int[n];
        Array.Fill(color, -1);

        var q = new Queue<int>();

        for (int s = 0; s < n; s++)
        {
            if (color[s] != -1) continue;
            color[s] = 0;
            q.Enqueue(s);

            while (q.Count > 0)
            {
                int v = q.Dequeue();
                foreach (int u in g[v])
                {
                    if (color[u] == -1)
                    {
                        color[u] = 1 - color[v];
                        q.Enqueue(u);
                    }
                    else if (color[u] == color[v])
                        return false;
                }
            }
        }

        return true;
    }

    public static int[] InitMatching(int n, IEnumerable<(char L, int R)> pairs, bool[] rightUsed, List<int>[] g)
    {
        var match = new int[n];
        Array.Fill(match, -1);

        foreach (var (l, r) in pairs)
        {
            int a = L(l);
            if (a < 0 || a >= 26) throw new ArgumentException("Bad left in initial matching");
            if (r <= 0 || r >= rightUsed.Length || !rightUsed[r]) throw new ArgumentException("Bad right in initial matching");
            int b = R(r);

            if (!g[a].Contains(b)) throw new ArgumentException("Initial matching uses non-edge");
            if (match[a] != -1 || match[b] != -1) throw new ArgumentException("Initial matching is not a matching");

            match[a] = b;
            match[b] = a;
        }

        return match;
    }

    public static int MatchingSize(int[] match) => match.Count(x => x != -1) / 2;

    public static int AugmentWave(List<int>[] g, int[] color, int[] match)
    {
        int n = g.Length; // колво вершин в графе\

        int size = MatchingSize(match); // начальный размер паросочетания

        while (true)
        {
            var parent = new int[n]; // для сохранения пути
            Array.Fill(parent, -1); // -1 = непройденное
            var q = new Queue<int>();

            for (int i = 0; i < n; i++)
                if (color[i] == 0 && match[i] == -1) // если рабочий свободен
                {
                    parent[i] = -2; // тогда он становится началм пути 
                    q.Enqueue(i); // складываем его в очередь = запускаем волну = нулевой фронт
                }

            int freeY = -1; // здесь храним свободное задание

            while (q.Count > 0 && freeY == -1) 
            {
                int x = q.Dequeue(); // достаем рабочего последнего
                foreach (int y in g[x]) // cсмотрим на все его задачи
                {
                    if (color[y] != 1) continue; // скипы чтобы уйти в правильную доли и не ходить кругами
                    if (match[x] == y) continue;
                    if (parent[y] != -1) continue;

                    parent[y] = x; // нашли его задание = новый фронт

                    if (match[y] == -1) { freeY = y; break; } // если его никто не выполняет Победа = задание непокрытое

                    int x2 = match[y]; // если кто-то выполняет то берем этого рабочего
                    if (parent[x2] == -1) // мы точно в него ещё не ходили в этой цепи = без повторов
                    {
                        parent[x2] = y; 
                        q.Enqueue(x2); // рабочего складываем в очередь, чтобы потом посомтреть, модет ли он делать каку-то другую работу = новый фронт
                    }
                }
            }

            if (freeY == -1) break; // задание так и не нашли свободное = тупик

            for (int y = freeY; ;) // перекраска
            {
                int x = parent[y]; // Кто пришел в задание y? (Рабочий x)
                int py = parent[x]; // Откуда пришел рабочий x?
                match[x] = y; // Теперь рабочий x делает задание y
                match[y] = x; // Задание y закреплено за рабочим x
                if (py == -2) break; // Если мы дошли до начала пути = стоп = выходим из бесконечного фора
                y = py; // Продолжаем разматывать дальше
            }

            size++; 
        }

        return size;
    }
}

#endregion

#region Program

struct DemoCase
{
    public int Id;
    public string AdjText;
    public string InitialMatching;

    public DemoCase(int id, string adjText, string initialMatching)
    {
        Id = id;
        AdjText = adjText;
        InitialMatching = initialMatching;
    }
}

class Program
{
    static void Main()
    {
        var demos = new List<DemoCase>
        {
            new DemoCase(
                1,
                "A(1,5)\nB(2)\nC(1,3)\nD(2,4,5)\nE(2,5)\n",
                "B-2, C-1, D-5"
            ),
            new DemoCase(
                2,
                "A(1,5)\nB(2,4)\nC(2,3,4)\nD(2,4)\nE(3)\n",
                "A-5, B-2, C-4"
            ),
            new DemoCase(
                3,
                "A(1,2,3)\nB(1,4,5)\nC(1,2)\nD(3,4,5)\nE(1)\n",
                "B-1, C-2, D-4"
            ),
            new DemoCase(
                4,
                "A(1,2,5)\nB(1,4)\nC(2,5)\nD(1,3)\nE(2,3)\n",
                "A-5, B-4, D-1, E-2"
            ),
            new DemoCase(
                5,
                "A(4)\nB(1,5)\nC(2,3)\nD(4)\nE(3)\n",
                "B-1, C-3, D-4"
            ),
            new DemoCase(
                6,
                "A(2)\nB(3)\nC(3,5)\nD(1,4)\nE(2)\n",
                "A-2, C-3, D-4"
            )
        };

        while (true)
        {
            PrintMenu(demos.Count);
            var choice = (Console.ReadLine() ?? "").Trim();

            if (choice == "0") return;

            if (choice == "7")
            {
                RunCustom();
                continue;
            }

            if (int.TryParse(choice, out int id) && id >= 1 && id <= demos.Count)
            {
                RunDemo(demos[id - 1]);
                continue;
            }

            Console.WriteLine("Unknown option.");
        }
    }

    static void PrintMenu(int demoCount)
    {
        Console.WriteLine();
        Console.WriteLine("Menu:");
        for (int i = 1; i <= demoCount; i++)
            Console.WriteLine($"{i} - show demo graph {i}");
        Console.WriteLine("7 - input graph manually");
        Console.WriteLine("0 - exit");
        Console.Write("Choose: ");
    }

    static void RunDemo(DemoCase c)
    {
        Console.WriteLine();
        Console.WriteLine($"Demo graph {c.Id}");
        Console.WriteLine("Adjacency:");
        Console.WriteLine(c.AdjText.TrimEnd());
        Console.WriteLine($"Initial matching: {(string.IsNullOrWhiteSpace(c.InitialMatching) ? "(empty)" : c.InitialMatching)}");

        var adj = Parse.ReadAdjFromText(c.AdjText);
        RunCase($"g{c.Id}", adj, c.InitialMatching);
    }

    static void RunCustom()
    {
        Console.WriteLine();
        Console.WriteLine("Enter lines like A(1,2,4). Empty line to finish:");
        var adj = Parse.ReadAdjInteractive();
        Console.WriteLine("Initial matching (optional), e.g. A-1, C-4, D-5. Empty for none:");
        var init = Console.ReadLine() ?? "";
        RunCase("custom", adj, init);
    }

    static void RunCase(string prefix, List<(char L, int[] R)> adj, string initialMatching)
    {
        try
        {
            var g = Matching.BuildGraph(adj, out var color, out var rightUsed);
            var initPairs = Parse.ParsePairs(initialMatching);
            var match = Matching.InitMatching(g.Length, initPairs, rightUsed, g);

            int sizeWave = Matching.AugmentWave(g, color, match);

            Console.WriteLine($"Wave size = {sizeWave}");

            var dot = Path.Combine(Environment.CurrentDirectory, $"{prefix}_wave.dot");
            var png = Path.Combine(Environment.CurrentDirectory, $"{prefix}_wave.png");

            GraphViz.SaveDot(g, color, match, dot, $"{prefix} | Wave size = {sizeWave}");
            Console.WriteLine($"DOT saved: {dot}");

            Console.WriteLine(GraphViz.TryRenderAndOpenPng(dot, png)
                ? "PNG generated and opened."
                : "PNG not generated (dot not found).");
        }
        catch (Exception ex)
        {
            Console.WriteLine("ERROR: " + ex.Message);
        }
    }
}

#endregion
