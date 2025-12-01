//using System;

//namespace seminar
//{
//    class Program
//    {
//        static void Main(string[] args)
//        {
//            Console.WriteLine("=== Оптимальное распределение инвестиций ===\n");

//            while (true)
//            {
//                Console.WriteLine("Выберите режим:");
//                Console.WriteLine("1 - Демо (5 проектов, investSum=60, шаг=10)");
//                Console.WriteLine("2 - Случайные данные");
//                Console.WriteLine("3 - Выход");
//                string input = Console.ReadLine();

//                if (input == "1")
//                {
//                    var (projectCount, investSum, profitTable) = DemoData();
//                    Optimization(projectCount, investSum, profitTable, 10);
//                    Console.WriteLine("\n" + new string('-', 50) + "\n");
//                }
//                else if (input == "2")
//                {
//                    var (projectCount, investSum, profitTable) = GenerateRandomData();
//                    Optimization(projectCount, investSum, profitTable, 10);
//                    Console.WriteLine("\n" + new string('-', 50) + "\n");
//                }
//                else if (input == "3")
//                {
//                    Console.WriteLine("Выход из программы.");
//                    break;
//                }
//                else
//                {
//                    Console.WriteLine("\nНекорректный ввод. Введите 1, 2 или 3.\n");
//                }
//            }

//            Console.WriteLine("Нажмите любую клавишу для завершения...");
//            Console.ReadKey();
//        }

//        static (int projectCount, int investSum, int[,] profitTable) DemoData()
//        {
//            int projectCount = 5;
//            int investSum = 60;
//            int[,] profitTable = new int[5, 7]
//            {
//                { 0,  9, 17, 26, 37, 49, 62 },
//                { 0, 11, 19, 28, 39, 51, 64 },
//                { 0, 10, 18, 27, 38, 50, 63 },
//                { 0, 12, 20, 29, 40, 52, 65 },
//                { 0, 13, 21, 30, 41, 53, 66 }

//            };

//            Console.WriteLine("\nРежим: Демо\n");
//            return (projectCount, investSum, profitTable);
//        }

//        static (int projectCount, int investSum, int[,] profitTable) GenerateRandomData()
//        {
//            Random rnd = new Random();
//            int projectCount = rnd.Next(3, 11);
//            int minInvestSum = projectCount * 10;
//            int maxInvestSum = 100;
//            int investSum = rnd.Next(minInvestSum / 10, maxInvestSum / 10 + 1) * 10;
//            int steps = investSum / 10;
//            int[,] profitTable = new int[projectCount, steps + 1];

//            for (int i = 0; i < projectCount; i++)
//            {
//                profitTable[i, 0] = 0;
//                for (int s = 1; s <= steps; s++)
//                {
//                    int prev = profitTable[i, s - 1];
//                    int maxInc = Math.Min(20, 99 - prev);
//                    profitTable[i, s] = prev + rnd.Next(1, Math.Max(2, maxInc + 1));
//                }
//            }

//            Console.WriteLine($"\nРежим: Случайные данные\nКоличество проектов: {projectCount}\nОбщая сумма инвестиций: {investSum}\n");
//            return (projectCount, investSum, profitTable);
//        }

//        static void Optimization(int projectCount, int investSum, int[,] profitTable, int step)
//        {
//            TableStructure(projectCount, investSum, profitTable, step);

//            var resultWithZero = SolveProblem(projectCount, investSum, profitTable, step, true);
//            var resultWithoutZero = SolveProblem(projectCount, investSum, profitTable, step, false);

//            Console.WriteLine("\n" + new string('=', 60));
//            Console.WriteLine("Вариант 1: Разрешено вложение 0");
//            Console.WriteLine($"Максимальная прибыль: {resultWithZero.MaxProfit}");
//            Console.WriteLine("Оптимальное распределение:");
//            for (int i = 0; i < projectCount; i++)
//            {
//                Console.WriteLine($"Проект {GetProjectName(i)}: {resultWithZero.Allocation[i]} у.е.");
//            }

//            Console.WriteLine("\n" + new string('=', 60));
//            Console.WriteLine("Вариант 2: Запрещено вложение 0");
//            if (projectCount * 10 > investSum)
//            {
//                Console.WriteLine("Ошибка: Невозможно распределить средства — минимум нужно 10 на каждый проект.");
//                Console.WriteLine($"Требуется минимум {projectCount * 10} у.е., но доступно только {investSum}.");
//            }
//            else
//            {
//                Console.WriteLine($"Максимальная прибыль: {resultWithoutZero.MaxProfit}");
//                Console.WriteLine("Оптимальное распределение:");
//                for (int i = 0; i < projectCount; i++)
//                {
//                    Console.WriteLine($"Проект {GetProjectName(i)}: {resultWithoutZero.Allocation[i]} у.е.");
//                }
//            }
//        }

//        static void TableStructure(int projectCount, int investSum, int[,] profitTable, int step)
//        {
//            int steps = profitTable.GetLength(1) - 1;

//            Console.WriteLine("\nТаблица прибылей:");
//            Console.Write("Влож. | ");
//            for (int i = 0; i < projectCount; i++)
//            {
//                Console.Write($"{GetProjectName(i),3} | ");
//            }
//            Console.WriteLine();
//            Console.Write(new string('-', 8 + projectCount * 5));
//            Console.WriteLine();

//            for (int s = 1; s <= steps; s++)
//            {
//                int amount = s * step;
//                Console.Write($"{amount,5} | ");
//                for (int i = 0; i < projectCount; i++)
//                {
//                    Console.Write($"{profitTable[i, s],3} | ");
//                }
//                Console.WriteLine();
//            }
//        }

//        static string GetProjectName(int index)
//        {
//            if (index < 26) return ((char)('A' + index)).ToString();
//            return "P" + (index + 1);
//        }

//        static (int MaxProfit, int[] Allocation) SolveProblem(int projectCount, int investSum, int[,] profitTable, int step, bool allowZero)
//        {
//            int steps = investSum / step;
//            int minStepsPerProject = allowZero ? 0 : 1;
//            int totalMinSteps = projectCount * minStepsPerProject;

//            if (totalMinSteps > steps)
//            {
//                int[] allocation0 = new int[projectCount];
//                return (0, allocation0);
//            }

//            int remainingSteps = steps - totalMinSteps;
//            int[,] profit = new int[projectCount + 1, remainingSteps + 1];
//            int[,] parent = new int[projectCount + 1, remainingSteps + 1];

//            for (int s = 0; s <= remainingSteps; s++)
//                profit[0, s] = 0;

//            for (int i = 1; i <= projectCount; i++)
//            {
//                for (int s = 0; s <= remainingSteps; s++)
//                {
//                    profit[i, s] = profit[i - 1, s];
//                    parent[i, s] = 0;

//                    for (int k = 0; k <= s; k++)
//                    {
//                        int currentProfit = profit[i - 1, s - k] + profitTable[i - 1, k + minStepsPerProject];
//                        if (currentProfit > profit[i, s])
//                        {
//                            profit[i, s] = currentProfit;
//                            parent[i, s] = k;
//                        }
//                    }
//                }
//            }

//            int[] allocation = new int[projectCount];
//            int remaining = remainingSteps;
//            for (int i = projectCount; i >= 1; i--)
//            {
//                int investedSteps = parent[i, remaining];
//                allocation[i - 1] = (investedSteps + minStepsPerProject) * step;
//                remaining -= investedSteps;
//            }

//            return (profit[projectCount, remainingSteps], allocation);
//        }
//    }
//}
