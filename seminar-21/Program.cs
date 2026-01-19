using System;
using System.Collections.Generic;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;

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
                char[] next = { 'A', 'G', 'A', 'A', 'B', 'B', 'L', 'M', 'N', 'O', 'B', 'C', 'D', 'E', 'H', 'G', 'H', 'I', 'I', 'L', 'M' };
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
        var allTasks = new HashSet<char>(Prev.Concat(Next)).OrderBy(x => x).ToList();

        var graph = new Dictionary<char, List<char>>();
        var beforeTask = new Dictionary<char, int>();
        foreach (char task in allTasks)
        {
            graph[task] = new List<char>();
            beforeTask[task] = 0;
        }

        for (int i = 0; i < Prev.Length; i++)
        {
            char p = Prev[i];
            char n = Next[i];

            graph[p].Add(n);
            beforeTask[n]++;
        }

        var availableTasks = new SortedSet<char>(allTasks.Where(t => beforeTask[t] == 0));

        var schedule = new Dictionary<char, int>();
        int time = 0;

        while (availableTasks.Count > 0)
        {
            var accomplishment = availableTasks.Take(2).ToList();
            foreach (char task in accomplishment)
            {
                schedule[task] = time;
                availableTasks.Remove(task);

                foreach (char nextTask in graph[task])
                {
                    beforeTask[nextTask]--;
                    if (beforeTask[nextTask] == 0)
                    {
                        availableTasks.Add(nextTask);
                    }
                }
            }
            time++;
        }

        Console.WriteLine("Диаграмма Ганта:");
        Console.WriteLine("Исп  1  2");
        var maxTime = schedule.Values.Max();
        for (int t = 0; t <= maxTime; t++)
        {
            var tasks = schedule.Where(x => x.Value == t).Select(x => x.Key).ToList();
            Console.WriteLine($"t={t + 1}: {(tasks.Count > 0 ? string.Join(", ", tasks) : "[ GHJGECR ]")}");
        }
    }
}