using System;
using System.Numerics;

class seminar_03
{
    static void Main()
    {
        while (true)
        {
            Console.WriteLine("Меню:");
            Console.WriteLine("1 - Task1 (города)");
            Console.WriteLine("2 - Task2 (инвестиции)");
            Console.WriteLine("0 - Выход");
            Console.Write("Выбор: ");

            string choice = Console.ReadLine().Trim();

            if (choice == "0") return;
            if (choice == "1") Task1();
            else if (choice == "2") Task2();
            else Console.WriteLine("Не понял выбор.\n");
        }
    }

    #region TASK 1
    static void Task1()
    {
        Console.WriteLine("\nTask1:");
        Console.WriteLine("1 - Ввести данные");
        Console.WriteLine("2 - Демо");
        Console.Write("Выбор: ");
        string mode = Console.ReadLine().Trim();

        if (mode == "2")
        {
            Task1Demo();
            Console.WriteLine();
            return;
        }

        Task1Input();
        Console.WriteLine();
    }

    static void Task1Input()
    {
        Console.WriteLine("\nВвод:");
        Console.WriteLine("1) AA AB BC CA BB CC   (6 чисел через пробел)");
        Console.WriteLine("2) n                   (сколько дней)");
        Console.WriteLine("3) finish              (A/B/C)");
        Console.WriteLine("Пример: 1 1 1 1 0 0 \n5 \nA");
        Console.WriteLine();

        var p = Console.ReadLine().Split(' ');
        string aa = p[0], ab = p[1], bc = p[2], ca = p[3], bb = p[4], cc = p[5];

        int n = int.Parse(Console.ReadLine());
        int finish = CityIndex(Console.ReadLine());

        Console.WriteLine("Ответ (long): " + CountRoutesFromA_Long(aa, ab, bc, ca, bb, cc, n, finish));
        //Console.WriteLine("Ответ (BigInteger): " + CountRoutesFromA_Big(aa, ab, bc, ca, bb, cc, n, finish));
    }

    static void Task1Demo()
    {
        // Наш граф:
        // AA=1, AB=1, BC=1, CA=1, BB=1, CC=0
        string aa = "1", ab = "1", bc = "1", ca = "1", bb = "1", cc = "0";
        int finish = 2; 

        Console.WriteLine("\nДемо: все дороги + петли в A и B, финиш C");
        Console.WriteLine("AA AB BC CA BB CC = 1 1 1 1 1 0");
        Console.WriteLine("Старт: A, финиш: C");

        long cPrev2 = 0; 
        long cPrev1 = 1; 

        for (int n = 0; n <= 5; n++)
        {
            long cFormula;
            if (n == 0) cFormula = 0;
            else if (n == 1) cFormula = 1;
            else
            {
                long cNow = 2 * cPrev1 + 2 * cPrev2;
                cPrev2 = cPrev1;
                cPrev1 = cNow;
                cFormula = cNow;
            }

            string ansLong = CountRoutesFromA_Long(aa, ab, bc, ca, bb, cc, n, finish);
            BigInteger ansBig = CountRoutesFromA_Big(aa, ab, bc, ca, bb, cc, n, finish);

            Console.WriteLine($"n={n}: формула c(n)={cFormula}, \tlong={ansLong}, \tBigInteger={ansBig}");
        }

        Console.WriteLine();
    }



    static int CityIndex(string s)
    {
        char c = char.ToUpperInvariant(s.Trim()[0]);
        return c switch { 'A' => 0, 'B' => 1, 'C' => 2, _ => 0 };
    }

    static long AA_L, AB_L, BC_L, CA_L, BB_L, CC_L;
    static long?[] memoA_L, memoB_L, memoC_L;

    static string CountRoutesFromA_Long(string aa, string ab, string bc, string ca, string bb, string cc, int n, int finish)
    {
        AA_L = long.Parse(aa);
        AB_L = long.Parse(ab);
        BC_L = long.Parse(bc);
        CA_L = long.Parse(ca);
        BB_L = long.Parse(bb);
        CC_L = long.Parse(cc);

        memoA_L = new long?[n + 1];
        memoB_L = new long?[n + 1];
        memoC_L = new long?[n + 1];

        try
        {
            long ans = finish switch
            {
                0 => A_L(n),
                1 => B_L(n),
                2 => C_L(n),
                _ => A_L(n)
            };
            return ans.ToString();
        }
        catch (OverflowException)
        {
            return "OVERFLOW";
        }
    }

    static long A_L(int n)
    {
        if (memoA_L[n].HasValue) return memoA_L[n].Value;
        long res;

        if (n == 0) res = 1;
        else
        {
            checked { res = AA_L * A_L(n - 1) + AB_L * B_L(n - 1) + CA_L * C_L(n - 1);}
        }

        memoA_L[n] = res;
        return res;
    }

    static long B_L(int n)
    {
        if (memoB_L[n].HasValue) return memoB_L[n].Value;
        long res;

        if (n == 0) res = 0;
        else
        {
            checked { res = AB_L * A_L(n - 1) + BB_L * B_L(n - 1) + BC_L * C_L(n - 1); }
        }
        memoB_L[n] = res;
        return res;
    }

    static long C_L(int n)
    {
        if (memoC_L[n].HasValue) return memoC_L[n].Value;
        long res;

        if (n == 0) res = 0;
        else
        {
            checked { res = CA_L * A_L(n - 1) + BC_L * B_L(n - 1) + CC_L * C_L(n - 1); }
        }

        memoC_L[n] = res;
        return res;
    }

    #region BigInteger(the same) 

    static BigInteger AA_B, AB_B, BC_B, CA_B, BB_B, CC_B;
    static BigInteger?[] memoA_B, memoB_B, memoC_B;

    static BigInteger CountRoutesFromA_Big(string aa, string ab, string bc, string ca, string bb, string cc, int n, int finish)
    {
        AA_B = BigInteger.Parse(aa);
        AB_B = BigInteger.Parse(ab);
        BC_B = BigInteger.Parse(bc);
        CA_B = BigInteger.Parse(ca);
        BB_B = BigInteger.Parse(bb);
        CC_B = BigInteger.Parse(cc);

        memoA_B = new BigInteger?[n + 1];
        memoB_B = new BigInteger?[n + 1];
        memoC_B = new BigInteger?[n + 1];

        return finish switch
        {
            0 => A_B(n),
            1 => B_B(n),
            2 => C_B(n),
            _ => A_B(n)
        };
    }

    static BigInteger A_B(int n)
    {
        if (memoA_B[n].HasValue) return memoA_B[n]!.Value;
        BigInteger res = (n == 0) ? 1 : AA_B * A_B(n - 1) + AB_B * B_B(n - 1) + CA_B * C_B(n - 1);
        memoA_B[n] = res;
        return res;
    }

    static BigInteger B_B(int n)
    {
        if (memoB_B[n].HasValue) return memoB_B[n]!.Value;
        BigInteger res = (n == 0) ? 0 : AB_B * A_B(n - 1) + BB_B * B_B(n - 1) + BC_B * C_B(n - 1);
        memoB_B[n] = res;
        return res;
    }

    static BigInteger C_B(int n)
    {
        if (memoC_B[n].HasValue) return memoC_B[n]!.Value;
        BigInteger res = (n == 0) ? 0 : CA_B * A_B(n - 1) + BC_B * B_B(n - 1) + CC_B * C_B(n - 1);
        memoC_B[n] = res;
        return res;
    }
    #endregion

    #endregion

    #region TASK 2

    static void Task2()
    {
        Console.WriteLine("\n=== Оптимальное распределение инвестиций ===\n");

        while (true)
        {
            Console.WriteLine("Выберите режим:");
            Console.WriteLine("1 - Демо (5 проектов, investSum=60, шаг=10)");
            Console.WriteLine("2 - Случайные данные");
            Console.WriteLine("0 - Назад в главное меню");
            string input = Console.ReadLine();

            if (input == "1")
            {
                var data = DemoData_Task2();
                Optimization_Task2(data.projectCount, data.investSum, data.profitTable, 10);
                Console.WriteLine("\n" + new string('-', 50) + "\n");
            }
            else if (input == "2")
            {
                var data = GenerateRandomData_Task2();
                Optimization_Task2(data.projectCount, data.investSum, data.profitTable, 10);
                Console.WriteLine("\n" + new string('-', 50) + "\n");
            }
            else if (input == "0")
            {
                Console.WriteLine();
                return;
            }
            else
            {
                Console.WriteLine("\nНекорректный ввод. Введите 1, 2 или 0.\n");
            }
        }
    }

    static (int projectCount, int investSum, int[,] profitTable) DemoData_Task2()
    {
        int projectCount = 5;
        int investSum = 60;
        int[,] profitTable = new int[5, 7]
        {
            { 0,  9, 17, 26, 37, 49, 62 },
            { 0, 11, 19, 28, 39, 51, 64 },
            { 0, 10, 18, 27, 38, 50, 63 },
            { 0, 12, 20, 29, 40, 52, 65 },
            { 0, 13, 21, 30, 41, 53, 66 }
        };

        Console.WriteLine("\nРежим: Демо\n");
        return (projectCount, investSum, profitTable);
    }

    static (int projectCount, int investSum, int[,] profitTable) GenerateRandomData_Task2()
    {
        Random rnd = new Random();
        int projectCount = rnd.Next(3, 11);
        int minInvestSum = projectCount * 10;
        int maxInvestSum = 100;
        int investSum = rnd.Next(minInvestSum / 10, maxInvestSum / 10 + 1) * 10;
        int steps = investSum / 10;
        int[,] profitTable = new int[projectCount, steps + 1];

        for (int i = 0; i < projectCount; i++)
        {
            profitTable[i, 0] = 0;
            for (int s = 1; s <= steps; s++)
            {
                int prev = profitTable[i, s - 1];
                int maxInc = Math.Min(20, 99 - prev);
                profitTable[i, s] = prev + rnd.Next(1, Math.Max(2, maxInc + 1));
            }
        }

        Console.WriteLine($"\nРежим: Случайные данные\nКоличество проектов: {projectCount}\nОбщая сумма инвестиций: {investSum}\n");
        return (projectCount, investSum, profitTable);
    }

    static void Optimization_Task2(int projectCount, int investSum, int[,] profitTable, int step)
    {
        TableStructure_Task2(projectCount, investSum, profitTable, step);

        var resultWithZero = SolveProblem_Task2(projectCount, investSum, profitTable, step, true);
        var resultWithoutZero = SolveProblem_Task2(projectCount, investSum, profitTable, step, false);

        Console.WriteLine("\n" + new string('=', 60));
        Console.WriteLine("Вариант 1: Разрешено вложение 0");
        Console.WriteLine($"Максимальная прибыль: {resultWithZero.MaxProfit}");
        Console.WriteLine("Оптимальное распределение:");
        for (int i = 0; i < projectCount; i++)
            Console.WriteLine($"Проект {GetProjectName_Task2(i)}: {resultWithZero.Allocation[i]} у.е.");

        Console.WriteLine("\n" + new string('=', 60));
        Console.WriteLine("Вариант 2: Запрещено вложение 0");
        if (projectCount * 10 > investSum)
        {
            Console.WriteLine("Ошибка: Невозможно распределить средства — минимум нужно 10 на каждый проект.");
            Console.WriteLine($"Требуется минимум {projectCount * 10} у.е., но доступно только {investSum}.");
        }
        else
        {
            Console.WriteLine($"Максимальная прибыль: {resultWithoutZero.MaxProfit}");
            Console.WriteLine("Оптимальное распределение:");
            for (int i = 0; i < projectCount; i++)
                Console.WriteLine($"Проект {GetProjectName_Task2(i)}: {resultWithoutZero.Allocation[i]} у.е.");
        }
    }

    static void TableStructure_Task2(int projectCount, int investSum, int[,] profitTable, int step)
    {
        int steps = profitTable.GetLength(1) - 1;

        Console.WriteLine("\nТаблица прибылей:");
        Console.Write("Влож. | ");
        for (int i = 0; i < projectCount; i++)
            Console.Write($"{GetProjectName_Task2(i),3} | ");
        Console.WriteLine();
        Console.Write(new string('-', 8 + projectCount * 5));
        Console.WriteLine();

        for (int s = 1; s <= steps; s++)
        {
            int amount = s * step;
            Console.Write($"{amount,5} | ");
            for (int i = 0; i < projectCount; i++)
                Console.Write($"{profitTable[i, s],3} | ");
            Console.WriteLine();
        }
    }

    static string GetProjectName_Task2(int index)
    {
        if (index < 26) return ((char)('A' + index)).ToString();
        return "P" + (index + 1);
    }

    static (int MaxProfit, int[] Allocation) SolveProblem_Task2(int projectCount, int investSum, int[,] profitTable, int step, bool allowZero)
    {
        int steps = investSum / step;
        int minStepsPerProject = allowZero ? 0 : 1;
        int totalMinSteps = projectCount * minStepsPerProject;

        if (totalMinSteps > steps)
        {
            int[] allocation0 = new int[projectCount];
            return (0, allocation0);
        }

        int remainingSteps = steps - totalMinSteps;
        int[,] profit = new int[projectCount + 1, remainingSteps + 1];
        int[,] parent = new int[projectCount + 1, remainingSteps + 1];

        for (int s = 0; s <= remainingSteps; s++)
            profit[0, s] = 0;

        for (int i = 1; i <= projectCount; i++)
        {
            for (int s = 0; s <= remainingSteps; s++)
            {
                profit[i, s] = profit[i - 1, s];
                parent[i, s] = 0;

                for (int k = 0; k <= s; k++)
                {
                    int currentProfit = profit[i - 1, s - k] + profitTable[i - 1, k + minStepsPerProject];
                    if (currentProfit > profit[i, s])
                    {
                        profit[i, s] = currentProfit;
                        parent[i, s] = k;
                    }
                }
            }
        }

        int[] allocation = new int[projectCount];
        int remaining = remainingSteps;
        for (int i = projectCount; i >= 1; i--)
        {
            int investedSteps = parent[i, remaining];
            allocation[i - 1] = (investedSteps + minStepsPerProject) * step;
            remaining -= investedSteps;
        }

        return (profit[projectCount, remainingSteps], allocation);
    }
    #endregion
}
