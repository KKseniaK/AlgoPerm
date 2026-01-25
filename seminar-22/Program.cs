using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

public static class BipartiteMatchingViz
{
    #region Graph
    public static List<int>[] CreateGraph(int n)
    {
        var g = new List<int>[n];
        for (int i = 0; i < n; i++) g[i] = new List<int>();
        return g;
    }

    public static void AddUndirectedEdge(List<int>[] g, int a, int b)
    {
        g[a].Add(b);
        g[b].Add(a);
    }
    #endregion

    #region Bipartite
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
                    if (u == v) return false;
                    if (color[u] == -1)
                    {
                        color[u] = 1 - color[v];
                        q.Enqueue(u);
                    }
                    else if (color[u] == color[v]) return false;
                }
            }
        }

        return true;
    }

    public static void BuildPartitions(int[] color, out List<int> X, out List<int> Y)
    {
        X = new List<int>();
        Y = new List<int>();
        for (int v = 0; v < color.Length; v++)
        {
            if (color[v] == 0) X.Add(v);
            else if (color[v] == 1) Y.Add(v);
            else throw new ArgumentException("Color contains -1.");
        }
    }
    #endregion

    #region MatchingWave
    public static int MaxMatchingWaveBfs(List<int>[] g, int[] color, out int[] match)
    {
        int n = g.Length;
        match = new int[n];
        Array.Fill(match, -1);
        int ans = 0;

        while (true)
        {
            var parent = new int[n];
            Array.Fill(parent, -1);
            var q = new Queue<int>();

            for (int x = 0; x < n; x++)
                if (color[x] == 0 && match[x] == -1)
                { parent[x] = -2; q.Enqueue(x); }

            int freeY = -1;

            while (q.Count > 0 && freeY == -1)
            {
                int x = q.Dequeue();
                if (color[x] != 0) continue;

                foreach (int y in g[x])
                {
                    if (color[y] != 1) continue;
                    if (match[x] == y) continue;
                    if (parent[y] != -1) continue;

                    parent[y] = x;

                    if (match[y] == -1) { freeY = y; break; }

                    int x2 = match[y];
                    if (parent[x2] == -1)
                    { parent[x2] = y; q.Enqueue(x2); }
                }
            }

            if (freeY == -1) break;

            for (int y = freeY; ;)
            {
                int x = parent[y];
                int prevY = parent[x];
                match[x] = y;
                match[y] = x;
                if (prevY == -2) break;
                y = prevY;
            }

            ans++;
        }

        return ans;
    }
    #endregion

    #region MatchingKuhn
    public static int MaxMatchingKuhnDfs(List<int>[] g, int[] color, out int[] match)
    {
        int n = g.Length;
        match = new int[n];
        Array.Fill(match, -1);
        int ans = 0;
        var used = new bool[n];

        bool Dfs(int x)
        {
            if (used[x]) return false;
            used[x] = true;

            foreach (int y in g[x])
            {
                if (color[y] != 1) continue;
                if (match[y] == -1 || Dfs(match[y]))
                {
                    match[x] = y;
                    match[y] = x;
                    return true;
                }
            }
            return false;
        }

        for (int x = 0; x < n; x++)
        {
            if (color[x] != 0) continue;
            if (match[x] != -1) continue;
            Array.Fill(used, false);
            if (Dfs(x)) ans++;
        }

        return ans;
    }
    #endregion

    #region GraphvizDot
    static string NodeId(int v) => $"v{v}";

    public static string ToDotBipartite(List<int>[] g, int[] color, int[] match, string title = "Bipartite Matching")
    {
        int n = g.Length;
        BuildPartitions(color, out var X, out var Y);

        var sb = new StringBuilder();
        sb.AppendLine("graph G {");
        sb.AppendLine("  graph [bgcolor=\"white\", fontname=\"Arial\", labelloc=\"t\", fontsize=18];");
        sb.AppendLine($"  label=\"{Escape(title)}\";");
        sb.AppendLine("  node  [shape=circle, fontname=\"Arial\", fontsize=14, style=filled, fillcolor=\"white\", color=\"#333333\"];");
        sb.AppendLine("  edge  [color=\"#999999\", penwidth=1.2];");
        sb.AppendLine("  rankdir=LR;");

        sb.Append("  { rank=same; ");
        for (int i = 0; i < X.Count; i++) sb.Append(NodeId(X[i]) + (i + 1 < X.Count ? " " : ""));
        sb.AppendLine(" }");

        sb.Append("  { rank=same; ");
        for (int i = 0; i < Y.Count; i++) sb.Append(NodeId(Y[i]) + (i + 1 < Y.Count ? " " : ""));
        sb.AppendLine(" }");

        for (int v = 0; v < n; v++)
        {
            string fill = color[v] == 0 ? "#E8F0FE" : "#E6F4EA";
            string label = v.ToString();
            sb.AppendLine($"  {NodeId(v)} [label=\"{label}\", fillcolor=\"{fill}\"];");
        }

        var seen = new HashSet<(int, int)>();
        for (int a = 0; a < n; a++)
        {
            foreach (int b in g[a])
            {
                int u = Math.Min(a, b), v = Math.Max(a, b);
                if (!seen.Add((u, v))) continue;

                bool inMatching = (match[a] == b);
                if (inMatching)
                    sb.AppendLine($"  {NodeId(u)} -- {NodeId(v)} [color=\"#D93025\", penwidth=3.0];");
                else
                    sb.AppendLine($"  {NodeId(u)} -- {NodeId(v)};");
            }
        }

        sb.AppendLine("}");
        return sb.ToString();
    }

    static string Escape(string s) => s.Replace("\\", "\\\\").Replace("\"", "\\\"");
    #endregion

    #region GraphvizRender
    public static void WriteDot(string dot, string dotPath)
    {
        File.WriteAllText(dotPath, dot, Encoding.UTF8);
    }

    public static bool RenderWithDotExe(string dotExePath, string dotPath, string outputPath, string format = "png")
    {
        var psi = new ProcessStartInfo
        {
            FileName = dotExePath,
            Arguments = $"-T{format} \"{dotPath}\" -o \"{outputPath}\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        using var p = Process.Start(psi);
        if (p == null) return false;
        p.WaitForExit();
        return p.ExitCode == 0;
    }
    #endregion

    #region DemoMain
    public static void Main()
    {
        var g = CreateGraph(6);
        AddUndirectedEdge(g, 0, 3);
        AddUndirectedEdge(g, 0, 4);
        AddUndirectedEdge(g, 1, 3);
        AddUndirectedEdge(g, 1, 5);
        AddUndirectedEdge(g, 2, 4);

        if (!IsBipartite(g, out var color))
        {
            Console.WriteLine("NOT BIPARTITE");
            return;
        }

        int size = MaxMatchingWaveBfs(g, color, out var match);
        var dot = ToDotBipartite(g, color, match, $"Wave BFS matching size = {size}");

        var dotPath = "graph.dot";
        var pngPath = "graph.png";
        WriteDot(dot, dotPath);

        Console.WriteLine(dotPath);
        Console.WriteLine(pngPath);
    }
    #endregion
}
