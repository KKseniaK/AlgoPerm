using System;
using System.Collections.Generic;
using System.Linq;

namespace last_work
{
    class Program
    {
        static void Main()
        {
            RunBruteForceTspDemo();
        }

        #region Brute force TSP demo

        private static void RunBruteForceTspDemo()
        {
            Console.WriteLine("Переборный алгоритм для задачи коммивояжёра");
            Console.WriteLine("1 - пример 5 на 5 ранд(лекции ещё не было)");
            Console.WriteLine("2 - пример с практики");
            Console.WriteLine("3 - пример из дз (алго Литтла)");
            Console.WriteLine("4 - ввести матрицу вручную(только размерность 5 на 5)");
            Console.Write("Выбери вариант: ");

            int mode = ReadInt(1, 4);

            int[,] matrix = mode switch
            {
                1 => GetLectureCase(),
                2 => GetPracticeCase(),
                3 => GetHomeworkCase(),
                4 => ReadMatrix(5),
                _ => GetLectureCase()
            };

            Console.WriteLine();
            Console.WriteLine("Матрица расстояний:");
            PrintMatrix(matrix);

            var result = SolveTspBruteForce(matrix);

            Console.WriteLine();
            Console.WriteLine($"Проверено маршрутов: {result.RouteCount}");
            Console.WriteLine($"Лучший цикл: {FormatRoute(result.BestRoute)}");
            Console.WriteLine($"Длина лучшего цикла: {result.BestDistance}");

            Console.WriteLine();
            Console.WriteLine("Хотите прогнать все встроенные примеры сразу?");
            Console.WriteLine("1 - да");
            Console.WriteLine("2 - нет");
            Console.Write("Выбор: ");

            int answer = ReadInt(1, 2);

            if (answer == 1)
            {
                RunAllBuiltInCases();
            }
        }

        private static void RunAllBuiltInCases()
        {
            var cases = new List<(string Name, int[,] Matrix)>
            {
                ("Пример с лекции", GetLectureCase()),
                ("Пример с практики", GetPracticeCase()),
                ("Пример из домашнего задания", GetHomeworkCase())
            };

            Console.WriteLine();
            Console.WriteLine("=== Проверка всех встроенных примеров ===");

            foreach (var testCase in cases)
            {
                var result = SolveTspBruteForce(testCase.Matrix);

                Console.WriteLine();
                Console.WriteLine(testCase.Name);
                PrintMatrix(testCase.Matrix);
                Console.WriteLine($"Маршрутов проверено: {result.RouteCount}");
                Console.WriteLine($"Лучший цикл: {FormatRoute(result.BestRoute)}");
                Console.WriteLine($"Минимальная длина: {result.BestDistance}");
            }
        }

        #endregion

        #region Brute force solver

        private static TspResult SolveTspBruteForce(int[,] matrix)
        {
            int n = matrix.GetLength(0);

            int[] cities = Enumerable.Range(0, n).ToArray();

            int bestDistance = int.MaxValue;
            int[] bestRoute = Array.Empty<int>();
            int routeCount = 0;

            Console.WriteLine();
            Console.WriteLine("Все маршруты и их длины:");
            Console.WriteLine();

            GeneratePermutations(cities, 0, permutation =>
            {
                routeCount++;

                int distance = CalculateCycleLength(permutation, matrix);

                Console.WriteLine(
                    $"{routeCount,3}: {FormatRoute(permutation)}  |  длина = {distance}"
                );

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestRoute = (int[])permutation.Clone();
                }
            });

            return new TspResult
            {
                BestRoute = bestRoute,
                BestDistance = bestDistance,
                RouteCount = routeCount
            };
        }

        private static void GeneratePermutations(int[] arr, int position, Action<int[]> action)
        {
            if (position == arr.Length)
            {
                action((int[])arr.Clone());
                return;
            }

            for (int i = position; i < arr.Length; i++)
            {
                Swap(arr, position, i);
                GeneratePermutations(arr, position + 1, action);
                Swap(arr, position, i);
            }
        }

        private static int CalculateCycleLength(int[] route, int[,] matrix)
        {
            int total = 0;

            for (int i = 0; i < route.Length - 1; i++)
            {
                total += matrix[route[i], route[i + 1]];
            }

            total += matrix[route[^1], route[0]];

            return total;
        }

        private static void Swap(int[] arr, int i, int j)
        {
            (arr[i], arr[j]) = (arr[j], arr[i]);
        }

        #endregion

        #region Test matrices

        private static int[,] GetLectureCase()
        {
            return new int[,]
            {
                { 0, 12, 10, 19,  8 },
                { 12, 0,  3,  7,  2 },
                { 10, 3,  0,  6, 20 },
                { 19, 7,  6,  0,  4 },
                { 8,  2, 20,  4,  0 }
            };
        }

        private static int[,] GetPracticeCase()
        {
            return new int[,]
            {
                { 0, 9, 4, 8, 6 },
                { 7, 0, 8, 9, 5 },
                { 8, 7, 0, 3, 4 },
                { 2, 8, 5, 0, 5 },
                { 4, 1, 6, 8, 0 }
            };
        }

        private static int[,] GetHomeworkCase()
        {
            return new int[,]
            {
                { 0,  3, 8,  7, 9 },
                { 8,  0,  6, 12, 10 },
                { 14, 5,  0, 15,  4 },
                { 6, 16,  8,  0,  9 },
                { 10, 7, 11,  3,  0 }
            };
        }

        #endregion

        #region Input / output

        private static int[,] ReadMatrix(int size)
        {
            var matrix = new int[size, size];

            Console.WriteLine();
            Console.WriteLine($"Введи матрицу {size}x{size}:");

            for (int i = 0; i < size; i++)
            {
                Console.WriteLine($"Строка {i + 1}: введи {size} чисел через пробел");

                while (true)
                {
                    var parts = Console.ReadLine()?
                        .Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                    if (parts == null || parts.Length != size)
                    {
                        Console.WriteLine("Нужно ввести ровно 5 чисел.");
                        continue;
                    }

                    bool ok = true;

                    for (int j = 0; j < size; j++)
                    {
                        if (!int.TryParse(parts[j], out int value) || value < 0)
                        {
                            ok = false;
                            break;
                        }

                        matrix[i, j] = value;
                    }

                    if (!ok)
                    {
                        Console.WriteLine("Ввод некорректный. Повтори строку.");
                        continue;
                    }

                    break;
                }
            }

            return matrix;
        }

        private static void PrintMatrix(int[,] matrix)
        {
            int n = matrix.GetLength(0);

            Console.WriteLine();

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    Console.Write($"{matrix[i, j],4}");
                }

                Console.WriteLine();
            }
        }

        private static string FormatRoute(int[] route)
        {
            var humanRoute = route.Select(x => x + 1).ToList();
            humanRoute.Add(route[0] + 1);

            return string.Join(" -> ", humanRoute);
        }

        private static int ReadInt(int min, int max)
        {
            while (true)
            {
                if (int.TryParse(Console.ReadLine(), out int value) && value >= min && value <= max)
                    return value;

                Console.Write($"Введи число от {min} до {max}: ");
            }
        }

        #endregion

        #region Result model

        private class TspResult
        {
            public int[] BestRoute { get; set; } = Array.Empty<int>();
            public int BestDistance { get; set; }
            public int RouteCount { get; set; }
        }

        #endregion
    }
}
