using seminar_27.Core;
using seminar_27.Problems;

namespace seminar_27
{
    class Program
    {
        private static void Main()
        {
            RunBackpackDemo();
        }

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
                new[]{
                    4, 7, 2, 9, 3, 8, 5, 6, 4, 7,
                    3, 2, 10, 6, 5, 8, 9, 4, 3, 7,
                    6, 5, 11, 2, 8, 4, 7, 9, 3, 6,
                    5, 12, 4, 8, 7, 3, 6, 10, 2, 9,
                    5, 4, 11, 6, 3, 8, 7, 5, 9, 4
                },
                new[]{
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
}
