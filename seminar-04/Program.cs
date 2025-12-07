using System;
using System.Collections.Generic;
using System.Linq;

namespace JohnsonDemo
{
    class Program
    {
        private static readonly Random Rand = new Random();

        static void Main()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== Алгоритм Джонсона (2 этапа) ===");
                Console.WriteLine("1. Демо-режим (10 задач)");
                Console.WriteLine("2. Случайные данные (5–15 задач)");
                Console.WriteLine("0. Выход");
                Console.Write("Выберите режим: ");

                string choice = Console.ReadLine();
                switch (choice)
                {
                    case "1":
                        Demo();
                        break;
                    case "2":
                        Random();
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("Неверный выбор. Нажмите любую клавишу...");
                        Console.ReadKey();
                        continue;
                }

                Console.WriteLine("\nНажмите любую клавишу для возврата в меню...");
                Console.ReadKey();
            }
        }

        static void Demo()
        {
            var jobs = new List<(string Name, int Stage1, int Stage2)>
            {
                ("A", 5, 8),
                ("B", 3, 6),
                ("C", 7, 2),
                ("D", 4, 9),
                ("E", 6, 4),
                ("F", 2, 7),
                ("G", 8, 3),
                ("H", 9, 5),
                ("I", 1, 10),
                ("J", 10, 1)
            };
            Console.WriteLine($"------------------------------------------------------------------------------");
            Console.WriteLine($"\nДЕМО ВЕРСИЯ");
            Console.WriteLine("\nДемо-задачи (10 шт.):");
            PrintJobTable(jobs);

            var order = JohnsonAlgorithm(jobs);
            var schedule = BuildSchedule(order);
            int time = schedule.Last().End2;

            Console.WriteLine("\nОптимальный порядок:");
            Console.WriteLine("1 этап: " + string.Join(", ", order.Select(j => $"{j.Name}({j.Stage1},{j.Stage2})")));
            Console.WriteLine("2 этап: " + string.Join(", ",
                schedule.Select(s => $"{s.Name}({s.Start2}–{s.End2})")));

            Console.WriteLine($"\nОбщее время выполнения: {time}");
            Console.WriteLine("Оптимальный порядок: " + string.Join(", ", order.Select(j => j.Name)));
        }

        static void Random()
        {
            int count = Rand.Next(5, 16);
            var jobs = GenerateRandomJobs(count);
            Console.WriteLine($"------------------------------------------------------------------------------");
            Console.WriteLine($"\nГЕНЕРАЦИЯ ЗАДАЧ И ИХ ДЛИТЕЛЬНОСТЕЙ");
            Console.WriteLine($"\nСгенерировано {count} случайных задач:");
            PrintJobTable(jobs);

            var order = JohnsonAlgorithm(jobs);
            var schedule = BuildSchedule(order);
            int time = schedule.Last().End2;

            Console.WriteLine("\n");
            Console.WriteLine("1 этап: " + string.Join(", ", order.Select(j => $"{j.Name}({j.Stage1},{j.Stage2})")));
            Console.WriteLine("2 этап: " + string.Join(", ",
                schedule.Select(s => $"{s.Name}({s.Start2}–{s.End2})")));

            Console.WriteLine($"\nОбщее время выполнения: {time}");
            Console.WriteLine("Оптимальный порядок: " + string.Join(", ", order.Select(j => j.Name)));
        }

        static List<(string Name, int Stage1, int Stage2)> GenerateRandomJobs(int count)
        {
            var jobs = new List<(string, int, int)>();
            for (int i = 0; i < count; i++)
            {
                string name = ((char)('A' + i)).ToString();
                int stage1 = Rand.Next(1, 21);
                int stage2 = Rand.Next(1, 21);
                jobs.Add((name, stage1, stage2));
            }
            return jobs;
        }


        static List<(string Name, int Stage1, int Stage2)> JohnsonAlgorithm(List<(string Name, int Stage1, int Stage2)> jobs)
        {
            var group1 = new List<(string Name, int Stage1, int Stage2)>();
            var group2 = new List<(string Name, int Stage1, int Stage2)>();

            foreach (var job in jobs)
            {
                if (job.Stage1 <= job.Stage2)
                    group1.Add(job);
                else
                    group2.Add(job);
            }
            //ЛИБО КОРОТКО, НО ЧУТЬ СЛОЖНЕЕ
            //var group1 = jobs.Where(j => j.Stage1 <= j.Stage2).ToList();
            //var group2 = jobs.Where(j => j.Stage1 > j.Stage2).ToList();

            for (int i = 0; i < group1.Count - 1; i++)
            {
                for (int j = i + 1; j < group1.Count; j++)
                {
                    if (group1[i].Stage1 > group1[j].Stage1)
                    {
                        var temp = group1[i];
                        group1[i] = group1[j];
                        group1[j] = temp;
                    }
                }
            }

            for (int i = 0; i < group2.Count - 1; i++)
            {
                for (int j = i + 1; j < group2.Count; j++)
                {
                    if (group2[i].Stage2 < group2[j].Stage2)
                    {
                        var temp = group2[i];
                        group2[i] = group2[j];
                        group2[j] = temp;
                    }
                }
            }

            //ЛИБО КОРОТКО, НО ЧУТЬ СЛОЖНЕЕ
            //group1.Sort((a, b) => a.Stage1.CompareTo(b.Stage1));
            //group2.Sort((a, b) => b.Stage2.CompareTo(a.Stage2));

            var result = new List<(string Name, int Stage1, int Stage2)>();
            result.AddRange(group1);
            result.AddRange(group2);
            return result;
        }

        //static List<(string Name, int Stage1, int Stage2)> JohnsonAlgorithm(List<(string Name, int Stage1, int Stage2)> jobs)
        //{
        //    var group1 = jobs.Where(j => j.Stage1 <= j.Stage2).ToList();
        //    var group2 = jobs.Where(j => j.Stage1 > j.Stage2).ToList();

        //    group1.Sort((a, b) => a.Stage1.CompareTo(b.Stage1));
        //    group2.Sort((a, b) => b.Stage2.CompareTo(a.Stage2));

        //    return group1.Concat(group2).ToList();
        //}

        record TaskSchedule(string Name, int Start1, int End1, int Start2, int End2);

        static List<TaskSchedule> BuildSchedule(List<(string Name, int Stage1, int Stage2)> order)
        {
            var schedule = new List<TaskSchedule>();
            int time1 = 0;
            int time2 = 0;

            foreach (var job in order)
            {
                int start1 = time1;
                int end1 = time1 + job.Stage1;
                time1 = end1;

                int start2 = Math.Max(end1, time2);
                int end2 = start2 + job.Stage2;
                time2 = end2;

                schedule.Add(new TaskSchedule(job.Name, start1, end1, start2, end2));
            }

            return schedule;
        }

        static void PrintJobTable(List<(string Name, int Stage1, int Stage2)> jobs)
        {
            if (jobs.Count == 0) return;

            int maxWidth = 0;

            foreach (var job in jobs)
            {
                maxWidth = Math.Max(maxWidth, job.Name.Length);
                maxWidth = Math.Max(maxWidth, job.Stage1.ToString().Length);
                maxWidth = Math.Max(maxWidth, job.Stage2.ToString().Length);
            }

            maxWidth += 1;

            var headerParts = new List<string>();
            var stage1Parts = new List<string>();
            var stage2Parts = new List<string>();

            foreach (var job in jobs)
            {
                headerParts.Add(job.Name.PadRight(maxWidth));
                stage1Parts.Add(job.Stage1.ToString().PadRight(maxWidth));
                stage2Parts.Add(job.Stage2.ToString().PadRight(maxWidth));
            }

            string headerRow = "            " + string.Join(" ", headerParts);
            string stage1Row = "Первый этап " + string.Join(" ", stage1Parts);
            string stage2Row = "Второй этап " + string.Join(" ", stage2Parts);

            Console.WriteLine(headerRow);
            Console.WriteLine(stage1Row);
            Console.WriteLine(stage2Row);
        }
    }
}
