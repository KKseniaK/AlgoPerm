using System;
using System.Collections.Generic;
using System.Linq;

class Program
{
    static void Main()
    {
        Console.WriteLine("Здравствуйте! Выберите способ ввода данных для задачи по лексикографической стратегии расписания");
        Console.WriteLine("1 - готовый набор данных");
        Console.WriteLine("2 - самостоятельный ввод");
        int choice = int.Parse(Console.ReadLine());

        switch (choice)
        {
            case 1:
                char[] prev = { 'J', 'A', 'K', 'F', 'J', 'K', 'D', 'D', 'E', 'E', 'F', 'G', 'G', 'H', 'B', 'I', 'A', 'K', 'J', 'E', 'C' };
                char[] next = { 'A', 'G', 'A', 'I', 'B', 'B', 'L', 'M', 'N', 'O', 'B', 'C', 'D', 'E', 'H', 'G', 'H', 'I', 'I', 'L', 'M' };
                GeneralProgram(prev, next);
                break;
            case 2:
                Console.Clear();
                Console.WriteLine("Через запятую введите предшествующие задачи:");
                string prevLine = Console.ReadLine();
                Console.WriteLine("Через запятую введите последующие задачи:");
                string nextLine = Console.ReadLine();

                string[] prevParts = prevLine.Split(',');
                string[] nextParts = nextLine.Split(',');

                char[] prev2 = new char[prevParts.Length];
                char[] next2 = new char[nextParts.Length];

                for (int i = 0; i < prevParts.Length; i++)
                {
                    prev2[i] = prevParts[i].Trim()[0];
                    next2[i] = nextParts[i].Trim()[0];
                }

                GeneralProgram(prev2, next2);
                break;
        }
    }
    public static void GeneralProgram(char[] Prev, char[] Next)
    {
        var graph = BuildGraph(Prev, Next);
        var allTasks = new HashSet<char>(Prev.Concat(Next)).OrderBy(x => x).ToList();

        var stok = allTasks.Where(task => !graph.ContainsKey(task) || graph[task].Count == 0).ToList();

        var priority = new Dictionary<char, int>();
        for (int i = 0; i < stok.Count; i++)
        {
            priority[stok[i]] = i + 1;
        }

        var othersPrio = new HashSet<char>(allTasks.Except(stok));
        int nextPrio = stok.Count + 1;

        while (othersPrio.Any())
        {
            var variantsOfTask = new List<(char task, string key)>();

            foreach (var task in othersPrio)
            {
                var childPriorities = graph[task]
                    .Where(child => priority.ContainsKey(child))
                    .Select(child => priority[child])
                    .OrderByDescending(p => p)
                    .ToList();

                if (childPriorities.Count == 0) continue;

                string key = string.Concat(childPriorities.Select(p => p.ToString("D3")));
                variantsOfTask.Add((task, key));
            }

            variantsOfTask.Sort((a, b) =>
            {
                int cmp = string.Compare(a.key, b.key, StringComparison.Ordinal);
                if (cmp != 0) return cmp;
                return a.task.CompareTo(b.task);
            });

            var best = variantsOfTask[0];
            priority[best.task] = nextPrio++;
            othersPrio.Remove(best.task);
        }


        Console.Clear();
        Console.WriteLine("\nПриоритеты задач:");
        foreach (var task in priority.OrderBy(task => task.Value))
        {
            if (graph.ContainsKey(task.Key))
            {
                var childPrios = graph[task.Key]
                    .Select(child => priority[child])
                    .OrderByDescending(p => p)
                    .ToList();
                string finalKey = childPrios.Count > 0 ? "\"" + string.Join("", childPrios) + "\"" : "\"\"";
                Console.WriteLine($"Задача {task.Key}: приоритет {task.Value}, ключ = {finalKey}");
            }
            else { Console.WriteLine($"Задача {task.Key}: приоритет {task.Value}"); }
        }

        //Построение обратного графа
        var previous = allTasks.ToDictionary(t => t, _ => new List<char>());
        for (int i = 0; i < Prev.Length; i++)
        {
            previous[Next[i]].Add(Prev[i]);
        }

        //Диаграмма ганта
        var completed = new HashSet<char>();
        var timeline = new List<char[]>();

        while (completed.Count < allTasks.Count)
        {
            var ready = allTasks // Задачи, которые еще не завершены, но все их предшественники завершены
                .Where(task => !completed.Contains(task) && previous[task].All(pred => completed.Contains(pred)))
                .OrderByDescending(task => priority[task])
                .ToList();

            var timeSlot = new char[2];
            for (int i = 0; i < 2; i++)
            {
                if (i < ready.Count)
                {
                    timeSlot[i] = ready[i];
                    completed.Add(ready[i]);
                }
                else { timeSlot[i] = '-'; }
            }
            timeline.Add(timeSlot);
        }

        Console.WriteLine("\nДиаграмма Ганта:");
        Console.WriteLine("Время | Исп 1 | Исп 2");
        Console.WriteLine("------+-------+-------");
        for (int t = 0; t < timeline.Count; t++)
        {
            var a = timeline[t];
            Console.WriteLine($"t={t + 1 + " "}  |   {a[0]}   |   {a[1]}");
        }
    }

    private static Dictionary<char, List<char>> BuildGraph(char[] prev, char[] next)
    {
        var graph = new Dictionary<char, List<char>>();
        for (int i = 0; i < prev.Length; i++)
        {
            if (!graph.ContainsKey(prev[i]))
                graph[prev[i]] = new List<char>();
            graph[prev[i]].Add(next[i]);
        }
        return graph;
    }
}