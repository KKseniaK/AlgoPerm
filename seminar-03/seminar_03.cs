using System;

namespace InvestmentOptimization
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Оптимальное распределение инвестиций ===\n");

            Console.WriteLine("Выберите режим:");
            Console.WriteLine("1 - Демо (4 проекта, D=50, шаг=10)");
            Console.WriteLine("2 - Случайные данные");
            string mode = Console.ReadLine();

            int m, D;
            int[,] profitTable;
            int step = 10;

            if (mode == "1")
            {
                m = 4;
                D = 50;
                profitTable = new int[4, 6]
                {
                    { 0, 11, 15, 22, 36, 51 },
                    { 0, 13, 17, 25, 32, 54 },
                    { 0, 14, 19, 28, 40, 53 },
                    { 0, 12, 21, 31, 44, 52 }
                };
                Console.WriteLine("Режим: Демо\n");
            }
            else
            {
                Random rnd = new Random();
                m = rnd.Next(3, 11);
                int minD = m * 10;
                int maxD = 100;
                D = rnd.Next(minD / 10, maxD / 10 + 1) * 10;
                int steps = D / 10;
                profitTable = new int[m, steps + 1];

                for (int i = 0; i < m; i++)
                {
                    profitTable[i, 0] = 0;
                    for (int s = 1; s <= steps; s++)
                    {
                        int prev = profitTable[i, s - 1];
                        int maxInc = Math.Min(20, 99 - prev);
                        profitTable[i, s] = prev + rnd.Next(1, Math.Max(2, maxInc + 1));
                    }
                }
                Console.WriteLine($"Режим: Случайные данные\nКоличество проектов: {m}\nОбщая сумма инвестиций: {D}\n");
            }

            PrintProfitTable(m, D, profitTable, step);

            var resultWithZero = SolveInvestmentProblem(m, D, profitTable, step, true);
            var resultWithoutZero = SolveInvestmentProblem(m, D, profitTable, step, false);

            Console.WriteLine("\n" + new string('=', 60));
            Console.WriteLine("Вариант 1: Разрешено вложение 0");
            Console.WriteLine($"Максимальная прибыль: {resultWithZero.MaxProfit}");
            Console.WriteLine("Оптимальное распределение:");
            for (int i = 0; i < m; i++)
            {
                Console.WriteLine($"Проект {GetProjectName(i)}: {resultWithZero.Allocation[i]} у.е.");
            }

            Console.WriteLine("\n" + new string('=', 60));
            Console.WriteLine("Вариант 2: Запрещено вложение 0 (все проекты >= 10)");
            if (m * 10 > D)
            {
                Console.WriteLine("Ошибка: Невозможно распределить средства — минимум нужно 10 на каждый проект.");
                Console.WriteLine($"Требуется минимум {m * 10} у.е., но доступно только {D}.");
            }
            else
            {
                Console.WriteLine($"Максимальная прибыль: {resultWithoutZero.MaxProfit}");
                Console.WriteLine("Оптимальное распределение:");
                for (int i = 0; i < m; i++)
                {
                    Console.WriteLine($"Проект {GetProjectName(i)}: {resultWithoutZero.Allocation[i]} у.е.");
                }
            }

            Console.WriteLine("\nНажмите любую клавишу для выхода...");
            Console.ReadKey();
        }

        static void PrintProfitTable(int m, int D, int[,] profitTable, int step)
        {
            int steps = profitTable.GetLength(1) - 1;

            Console.WriteLine("Таблица прибылей (вложения x проект):");
            Console.Write("Влож. | ");
            for (int i = 0; i < m; i++)
            {
                Console.Write($"{GetProjectName(i),3} | ");
            }
            Console.WriteLine();
            Console.Write(new string('-', 8 + m * 5));
            Console.WriteLine();

            for (int s = 1; s <= steps; s++)
            {
                int amount = s * step;
                Console.Write($"{amount,5} | ");
                for (int i = 0; i < m; i++)
                {
                    Console.Write($"{profitTable[i, s],3} | ");
                }
                Console.WriteLine();
            }
        }

        static string GetProjectName(int index)
        {
            if (index < 26) return ((char)('A' + index)).ToString();
            return "P" + (index + 1);
        }

        static Result SolveInvestmentProblem(int m, int D, int[,] profitTable, int step, bool allowZero)
        {
            int steps = D / step;
            int minStepsPerProject = allowZero ? 0 : 1;
            int totalMinSteps = m * minStepsPerProject;

            if (totalMinSteps > steps)
            {
                int[] allocation0 = new int[m];
                return new Result { MaxProfit = 0, Allocation = allocation0 };
            }

            int remainingSteps = steps - totalMinSteps;
            int[,] dp = new int[m + 1, remainingSteps + 1];
            int[,] parent = new int[m + 1, remainingSteps + 1];

            for (int s = 0; s <= remainingSteps; s++)
                dp[0, s] = 0;

            for (int i = 1; i <= m; i++)
            {
                for (int s = 0; s <= remainingSteps; s++)
                {
                    dp[i, s] = dp[i - 1, s];
                    parent[i, s] = 0;

                    for (int k = 0; k <= s; k++)
                    {
                        int currentProfit = dp[i - 1, s - k] + profitTable[i - 1, k + minStepsPerProject];
                        if (currentProfit > dp[i, s])
                        {
                            dp[i, s] = currentProfit;
                            parent[i, s] = k;
                        }
                    }
                }
            }

            int[] allocation = new int[m];
            int remaining = remainingSteps;
            for (int i = m; i >= 1; i--)
            {
                int investedSteps = parent[i, remaining];
                allocation[i - 1] = (investedSteps + minStepsPerProject) * step;
                remaining -= investedSteps;
            }

            return new Result { MaxProfit = dp[m, remainingSteps], Allocation = allocation };
        }
    }

    struct Result
    {
        public int MaxProfit;
        public int[] Allocation;
    }
}
