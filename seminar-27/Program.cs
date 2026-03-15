using seminar_27.Core;
using seminar_27.Problems;
using System;

namespace seminar_27
{
    class Program
    {
        #region Main

        private static void Main()
        {
            Console.WriteLine("Genetic Algorithm demos");
            Console.WriteLine("1 - Задача о рюкзаке");
            Console.WriteLine("2 - Задача о торговце");
            Console.Write("Выбери задачу: ");

            var problemMode = ReadInt(1, 2);

            Console.WriteLine();

            switch (problemMode)
            {
                case 1:
                    RunBackpackDemo();
                    break;

                case 2:
                    RunTravelingSalesmanDemo();
                    break;

                default:
                    throw new InvalidOperationException();
            }
        }

        #endregion

        #region Backpack

        private static void RunBackpackDemo()
        {
            Console.WriteLine("Backpack GA");
            Console.WriteLine("1 - с параметрами из ручного решения");
            Console.WriteLine("2 - биг сайз пример");
            Console.WriteLine("3 - ввести все руками");
            Console.Write("Выбрать свой путь: ");

            var mode = ReadInt(1, 3);

            var input = mode switch
            {
                1 => GetManualSolvedCase(),
                2 => GetLargeCase(),
                3 => ReadBackpackInput(),
                _ => throw new InvalidOperationException()
            };

            var config = mode switch
            {
                1 => GetManualCaseConfig(),
                2 => GetLargeCaseConfig(),
                3 => ReadGAConfig(),
                _ => throw new InvalidOperationException()
            };

            Console.WriteLine();
            PrintConfig(config);

            var problem = new BackpackProblem(input.Weights, input.Costs, input.Capacity);
            var engine = new GeneticAlgorithm<int[]>(problem, config);

            Console.WriteLine();
            var result = engine.Run();

            Console.WriteLine();
            Console.WriteLine("Лидер:");
            Console.WriteLine(problem.ChromosomeToString(result.BestIndividual.Chromosome));
            Console.WriteLine($"Fitness: {result.BestIndividual.Fitness}");
        }

        private static GAConfig GetManualCaseConfig()
        {
            return new GAConfig
            {
                PopulationSize = 6,
                ParentCount = 4,
                EliteCount = 3,
                ChildCount = 3,
                MaxGenerations = 10,
                StagnationLimit = 2,
                TournamentSize = 2,
                SelectionMethod = SelectionMethod.Roulette,
                CrossoverType = CrossoverType.TwoPoint
            };
        }

        private static GAConfig GetLargeCaseConfig()
        {
            return new GAConfig
            {
                PopulationSize = 80,
                ParentCount = 40,
                EliteCount = 20,
                ChildCount = 20,
                MaxGenerations = 150,
                StagnationLimit = 15,
                TournamentSize = 4,
                SelectionMethod = SelectionMethod.Tournament,
                CrossoverType = CrossoverType.Uniform
            };
        }

        private static BackpackInput GetManualSolvedCase()
        {
            return new BackpackInput(
                new[] { 3, 5, 4, 7, 6, 2, 8 },
                new[] { 6, 9, 7, 12, 10, 4, 11 },
                15
            );
        }

        private static BackpackInput GetLargeCase()
        {
            return new BackpackInput(
                new[]
                {
                    4, 7, 2, 9, 3, 8, 5, 6, 4, 7,
                    3, 2, 10, 6, 5, 8, 9, 4, 3, 7,
                    6, 5, 11, 2, 8, 4, 7, 9, 3, 6,
                    5, 12, 4, 8, 7, 3, 6, 10, 2, 9,
                    5, 4, 11, 6, 3, 8, 7, 5, 9, 4
                },
                new[]
                {
                    8, 13, 5, 17, 6, 14, 10, 11, 7, 12,
                    6, 4, 18, 10, 9, 15, 16, 8, 5, 13,
                    11, 9, 19, 4, 14, 7, 12, 16, 6, 10,
                    8, 20, 7, 15, 13, 5, 11, 18, 4, 17,
                    9, 7, 19, 10, 6, 14, 12, 8, 16, 7
                },
                140
            );
        }

        private static BackpackInput ReadBackpackInput()
        {
            Console.Write("Введите вместимость рюкзака: ");
            var capacity = ReadInt(1);

            Console.Write("Введите кол-во предметов: ");
            var itemCount = ReadInt(1);

            Console.WriteLine($"Введите {itemCount} значений весов через пробел:");
            var weights = ReadIntArray(itemCount);

            Console.WriteLine($"Введите {itemCount} значений цен через пробел:");
            var costs = ReadIntArray(itemCount);

            return new BackpackInput(weights, costs, capacity);
        }

        #endregion

        #region Traveling Salesman

        private static void RunTravelingSalesmanDemo()
        {
            Console.WriteLine("Traveling Salesman GA");
            Console.WriteLine("1 - пример с семинара 5x5");
            Console.WriteLine("2 - матрица из ручного решения 7x7");
            Console.WriteLine("3 - дополнительный большой пример 15x15");
            Console.WriteLine("4 - ввести матрицу руками");
            Console.Write("Выбрать свой путь: ");

            var mode = ReadInt(1, 4);

            var input = mode switch
            {
                1 => GetTravelingSalesmanSeminarCase(),
                2 => GetTravelingSalesmanManualSolvedCase(),
                3 => GetTravelingSalesmanLargeCase(),
                4 => ReadTravelingSalesmanInput(),
                _ => throw new InvalidOperationException()
            };

            Console.WriteLine();
            Console.WriteLine($"Пример: {input.CaseName}");

            Console.WriteLine();
            Console.WriteLine("Матрица расстояний:");
            PrintMatrix(input.Distances);

            Console.WriteLine();
            Console.WriteLine($"Стартовый город: {input.StartCity}");

            Console.WriteLine();
            Console.WriteLine("Параметры генетического алгоритма:");
            Console.WriteLine("1 - базовые для TSP");
            Console.WriteLine("2 - ввести руками");
            Console.Write("Выбрать конфигурацию: ");

            var configMode = ReadInt(1, 2);

            var config = configMode switch
            {
                1 => GetTravelingSalesmanDefaultConfig(),
                2 => ReadGAConfig(),
                _ => throw new InvalidOperationException()
            };

            Console.WriteLine();
            PrintConfig(config);

            var problem = new TravelingSalesman(input.Distances, input.StartCity);
            var engine = new GeneticAlgorithm<int[]>(problem, config);

            Console.WriteLine();
            var result = engine.Run();

            Console.WriteLine();
            Console.WriteLine("Лидер:");
            Console.WriteLine(problem.ChromosomeToString(result.BestIndividual.Chromosome));
            Console.WriteLine($"Fitness: {result.BestIndividual.Fitness}");
        }

        private static TravelingSalesmanInput GetTravelingSalesmanSeminarCase()
        {
            return new TravelingSalesmanInput(
                "Пример с семинара 5x5",
                new int[,]
                {
            { int.MaxValue, 9, 4, 8, 6 },
            { 7, int.MaxValue, 8, 9, 5 },
            { 8, 7, int.MaxValue, 3, 4 },
            { 2, 8, 5, int.MaxValue, 5 },
            { 4, 1, 6, 8, int.MaxValue }
                },
                1
            );
        }

        private static TravelingSalesmanInput GetTravelingSalesmanManualSolvedCase()
        {
            return new TravelingSalesmanInput(
                "Матрица из ручного решения 7x7",
                new int[,]
                {
            { 0, 3, 8, 7, 9, 11, 10 },
            { 6, 0, 2, 6, 8, 9, 7 },
            { 7, 5, 0, 3, 6, 8, 9 },
            { 9, 4, 5, 0, 2, 7, 8 },
            { 8, 7, 4, 6, 0, 5, 3 },
            { 4, 8, 7, 6, 5, 0, 6 },
            { 6, 9, 7, 5, 4, 2, 0 }
                },
                1
            );
        }

        private static TravelingSalesmanInput GetTravelingSalesmanLargeCase()
        {
            return new TravelingSalesmanInput(
                "Большой пример 15x15",
                new int[,]
                {
            { 0, 29, 20, 21, 16, 31, 100, 12, 4, 31, 18, 23, 17, 9, 25 },
            { 29, 0, 15, 29, 28, 40, 72, 21, 29, 41, 12, 27, 19, 15, 31 },
            { 20, 15, 0, 15, 14, 25, 81, 9, 23, 27, 13, 11, 7, 18, 14 },
            { 21, 29, 15, 0, 4, 12, 92, 12, 25, 13, 25, 13, 17, 10, 22 },
            { 16, 28, 14, 4, 0, 16, 94, 9, 20, 16, 22, 8, 15, 11, 19 },
            { 31, 40, 25, 12, 16, 0, 95, 24, 36, 3, 37, 18, 27, 13, 30 },
            { 100, 72, 81, 92, 94, 95, 0, 90, 101, 99, 84, 95, 90, 101, 96 },
            { 12, 21, 9, 12, 9, 24, 90, 0, 15, 25, 13, 17, 9, 12, 11 },
            { 4, 29, 23, 25, 20, 36, 101, 15, 0, 35, 18, 28, 22, 16, 20 },
            { 31, 41, 27, 13, 16, 3, 99, 25, 35, 0, 38, 20, 29, 17, 33 },
            { 18, 12, 13, 25, 22, 37, 84, 13, 18, 38, 0, 21, 11, 14, 16 },
            { 23, 27, 11, 13, 8, 18, 95, 17, 28, 20, 21, 0, 12, 9, 17 },
            { 17, 19, 7, 17, 15, 27, 90, 9, 22, 29, 11, 12, 0, 11, 10 },
            { 9, 15, 18, 10, 11, 13, 101, 12, 16, 17, 14, 9, 11, 0, 13 },
            { 25, 31, 14, 22, 19, 30, 96, 11, 20, 33, 16, 17, 10, 13, 0 }
                },
                1
            );
        }

        private static GAConfig GetTravelingSalesmanDefaultConfig()
        {
            return new GAConfig
            {
                PopulationSize = 20,
                ParentCount = 10,
                EliteCount = 6,
                ChildCount = 10,
                MaxGenerations = 50,
                StagnationLimit = 10,
                TournamentSize = 3,
                SelectionMethod = SelectionMethod.Tournament,
                CrossoverType = CrossoverType.OnePoint
            };
        }

        private static TravelingSalesmanInput ReadTravelingSalesmanInput()
        {
            Console.Write("Введите название примера: ");
            var caseName = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(caseName))
                caseName = "Пользовательский ввод";

            Console.Write("Введите число городов: ");
            var cityCount = ReadInt(2);

            Console.Write("Введите стартовый город: ");
            var startCity = ReadInt(1, cityCount);

            var matrix = new int[cityCount, cityCount];

            Console.WriteLine("Введите матрицу расстояний построчно.");
            Console.WriteLine($"В каждой строке должно быть {cityCount} чисел через пробел.");
            Console.WriteLine("Для диагонали можешь вводить 0.");

            for (var i = 0; i < cityCount; i++)
            {
                Console.WriteLine($"Строка {i + 1}:");
                var row = ReadIntArray(cityCount);

                for (var j = 0; j < cityCount; j++)
                    matrix[i, j] = row[j];
            }

            return new TravelingSalesmanInput(caseName, matrix, startCity);
        }

        private static void PrintMatrix(int[,] matrix)
        {
            var rows = matrix.GetLength(0);
            var cols = matrix.GetLength(1);

            for (var i = 0; i < rows; i++)
            {
                for (var j = 0; j < cols; j++)
                {
                    if (matrix[i, j] == int.MaxValue)
                        Console.Write($"{"∞",4}");
                    else
                        Console.Write($"{matrix[i, j],4}");
                }

                Console.WriteLine();
            }
        }

        #endregion

        private static void PrintConfig(GAConfig config)
        {
            Console.WriteLine("Параметры генетического алгоритма:");
            Console.WriteLine($"  Размер популяции: {config.PopulationSize}");
            Console.WriteLine($"  Число родителей: {config.ParentCount}");
            Console.WriteLine($"  Лучших родителей в новое поколение: {config.EliteCount}");
            Console.WriteLine($"  Число детей в новое поколение: {config.ChildCount}");
            Console.WriteLine($"  Максимум поколений: {config.MaxGenerations}");
            Console.WriteLine($"  Лимит стагнации: {config.StagnationLimit}");
            Console.WriteLine($"  Размер турнира: {config.TournamentSize}");
            Console.WriteLine($"  Метод отбора: {SelectionMethodToRu(config.SelectionMethod)}");
            Console.WriteLine($"  Тип скрещивания: {CrossoverTypeToRu(config.CrossoverType)}");
        }

        private static string SelectionMethodToRu(SelectionMethod method)
        {
            return method switch
            {
                SelectionMethod.Elite => "Элитный",
                SelectionMethod.Tournament => "Турнирный",
                SelectionMethod.Roulette => "Рулетка",
                _ => "Неизвестно"
            };
        }

        private static string CrossoverTypeToRu(CrossoverType type)
        {
            return type switch
            {
                CrossoverType.OnePoint => "Одноточечное",
                CrossoverType.TwoPoint => "Двухточечное",
                CrossoverType.Uniform => "Равномерное",
                _ => "Неизвестно"
            };
        }

        private static GAConfig ReadGAConfig()
        {
            Console.Write("Population size: ");
            var populationSize = ReadInt(2);

            Console.Write("Parent count: ");
            var parentCount = ReadInt(2, populationSize);

            Console.Write("Elite count: ");
            var eliteCount = ReadInt(0, populationSize);

            Console.Write("Child count: ");
            var childCount = ReadInt(0, populationSize);

            Console.Write("Max generations: ");
            var maxGenerations = ReadInt(1);

            Console.Write("Stagnation limit: ");
            var stagnationLimit = ReadInt(1);

            Console.Write("Tournament size: ");
            var tournamentSize = ReadInt(2);

            Console.WriteLine("Selection method:");
            Console.WriteLine("1 - Elite");
            Console.WriteLine("2 - Tournament");
            Console.WriteLine("3 - Roulette");
            var selectionMethod = ReadSelectionMethod();

            Console.WriteLine("Crossover type:");
            Console.WriteLine("1 - OnePoint");
            Console.WriteLine("2 - TwoPoint");
            Console.WriteLine("3 - Uniform");
            var crossoverType = ReadCrossoverType();

            return new GAConfig
            {
                PopulationSize = populationSize,
                ParentCount = parentCount,
                EliteCount = eliteCount,
                ChildCount = childCount,
                MaxGenerations = maxGenerations,
                StagnationLimit = stagnationLimit,
                TournamentSize = tournamentSize,
                SelectionMethod = selectionMethod,
                CrossoverType = crossoverType
            };
        }

        private static int ReadInt(int minValue, int maxValue = int.MaxValue)
        {
            while (true)
            {
                var raw = Console.ReadLine();

                if (int.TryParse(raw, out var value) && value >= minValue && value <= maxValue)
                    return value;

                Console.Write($"Enter an integer in range [{minValue}; {maxValue}]: ");
            }
        }

        private static int[] ReadIntArray(int expectedLength)
        {
            while (true)
            {
                var raw = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(raw))
                {
                    Console.WriteLine("Input cannot be empty. Try again:");
                    continue;
                }

                var parts = raw.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length != expectedLength)
                {
                    Console.WriteLine($"Expected {expectedLength} numbers. Try again:");
                    continue;
                }

                var result = new int[expectedLength];
                var ok = true;

                for (var i = 0; i < parts.Length; i++)
                {
                    if (!int.TryParse(parts[i], out result[i]))
                    {
                        ok = false;
                        break;
                    }
                }

                if (ok)
                    return result;

                Console.WriteLine("Invalid numbers. Try again:");
            }
        }

        private static SelectionMethod ReadSelectionMethod()
        {
            return ReadInt(1, 3) switch
            {
                1 => SelectionMethod.Elite,
                2 => SelectionMethod.Tournament,
                3 => SelectionMethod.Roulette,
                _ => throw new InvalidOperationException()
            };
        }

        private static CrossoverType ReadCrossoverType()
        {
            return ReadInt(1, 3) switch
            {
                1 => CrossoverType.OnePoint,
                2 => CrossoverType.TwoPoint,
                3 => CrossoverType.Uniform,
                _ => throw new InvalidOperationException()
            };
        }
    }

    internal sealed record BackpackInput(int[] Weights, int[] Costs, int Capacity);
    internal sealed record TravelingSalesmanInput(string CaseName, int[,] Distances, int StartCity);
}