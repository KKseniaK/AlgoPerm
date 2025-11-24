using System;
using System.Collections.Generic;
using System.Diagnostics;

class seminar_02
{
    static void Main()
    {
        while (true)
        {
            Console.WriteLine("\n===");
            Console.WriteLine("1 - Задание 1: Генерация бинарных строк (нет двух нулей подряд)");
            Console.WriteLine("2 - Задание 2: Сочетания C(n,k) рекурсивно 3 метода и сравнение времени");
            Console.WriteLine("3 - Задание 3: Сочетания C(n,k) итеративно");
            Console.WriteLine("d1 - Демо для задания 1");
            Console.WriteLine("d2 - Демо для задания 2");
            Console.WriteLine("d3 - Демо для задания 3");
            Console.WriteLine("x - Выход");
            Console.Write("Выберите пункт: ");
            var cmd = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(cmd)) continue;
            if (cmd == "x") break;

            switch (cmd)
            {
                case "1":
                    Task1();
                    break;
                case "2":
                    Task2();
                    break;
                case "3":
                    Task3();
                    break;
                case "d1":
                    Task1Demo();
                    break;
                case "d2":
                    Task2Demo();
                    break;
                case "d3":
                    Task3Demo();
                    break;
                default:
                    Console.WriteLine("Неизвестная команда.");
                    break;
            }
        }
    }

    #region Task1

    static void Task1()
    {
        Console.Write("Введите n (длина строк, целое положительное): ");
        if (!int.TryParse(Console.ReadLine(), out int n) || n <= 0)
        {
            Console.WriteLine("Некорректное значение n.");
            return;
        }

        var res = GenerateValidStrings(n);
        Console.WriteLine($"Найдено {res.Count} строк(а):");
        foreach (var s in res) Console.WriteLine(s);
    }

    static void Task1Demo()
    {
        int n = 4;
        Console.WriteLine($"Демо (n={n}):");
        var res = GenerateValidStrings(n);
        Console.WriteLine($"Найдено {res.Count} строк(а):");
        foreach (var s in res) Console.WriteLine(s);
    }

    static List<string> GenerateValidStrings(int n)
    {
        var result = new List<string>();
        if (n <= 0) return result;

        if (n == 1)
        {
            result.Add("0");
            result.Add("1");
            return result;
        }

        GenerateEndingWithOne(n, "1", result);
        GenerateEndingWithZero(n, "0", result);
        return result;
    }

    static void GenerateEndingWithOne(int n, string current, List<string> result)
    {
        if (current.Length == n)
        {
            result.Add(current);
            return;
        }

        GenerateEndingWithZero(n, current + "0", result);
        GenerateEndingWithOne(n, current + "1", result);
    }

    static void GenerateEndingWithZero(int n, string current, List<string> result)
    {
        if (current.Length == n)
        {
            result.Add(current);
            return;
        }

        GenerateEndingWithOne(n, current + "1", result);
    }

    #endregion

    #region Task2

    static long[,] memo;

    static void Task2()
    {
        Console.Write("Введите n (целое >=0): ");
        if (!int.TryParse(Console.ReadLine(), out int n) || n < 0)
        {
            Console.WriteLine("Некорректное n.");
            return;
        }
        Console.Write("Введите k (0..n): ");
        if (!int.TryParse(Console.ReadLine(), out int k) || k < 0 || k > n)
        {
            Console.WriteLine("Некорректное k.");
            return;
        }

        ComparePerformance(n, k);
    }

    static void Task2Demo()
    {
        int n = 15, k = 7;
        Console.WriteLine($"Демо: n={n}, k={k}");
        ComparePerformance(n, k);

        Console.WriteLine("\nМалые значения для контроля:");
        for (int i = 0; i <= 5; i++)
        {
            for (int j = 0; j <= i; j++)
            {
                Console.WriteLine($"C({i},{j}) = {CnkPascal(i, j)}");
            }
        }
    }

    static long CnkPascal(int n, int k)
    {
        if (k < 0 || k > n) return 0;
        if (k == 0 || k == n) return 1;
        return CnkPascal(n - 1, k - 1) + CnkPascal(n - 1, k);
    }

    static long CnkRecurrence(int n, int k)
    {
        if (k < 0 || k > n) return 0;
        if (k == 0) return 1;
        if (k > n - k) k = n - k;
        return n * CnkRecurrence(n - 1, k - 1) / k;
    }

    static long CnkPascalMemo(int n, int k)
    {
        if (k < 0 || k > n) return 0;
        if (k == 0 || k == n) return 1;
        if (memo[n, k] != -1) return memo[n, k];
        memo[n, k] = CnkPascalMemo(n - 1, k - 1) + CnkPascalMemo(n - 1, k);
        return memo[n, k];
    }

    static void ComparePerformance(int n, int k)
    {
        const int repeats = 1;

        Console.WriteLine($"Вычисляем C({n},{k}) тремя методами, повторений {repeats}...");

        var sw = Stopwatch.StartNew();
        long r1 = 0;
        for (int i = 0; i < repeats; i++)
            r1 = CnkPascal(n, k);
        sw.Stop();
        Console.WriteLine($"1) Паскаль рекурсивно: {r1} | Время: {sw.Elapsed.TotalMilliseconds} мс");

        sw.Restart();
        long r2 = 0;
        for (int i = 0; i < repeats; i++)
            r2 = CnkRecurrence(n, k);
        sw.Stop();
        Console.WriteLine($"2) Формула произведения: {r2} | Время: {sw.Elapsed.TotalMilliseconds} мс");

        memo = new long[n + 1, k + 1];
        for (int i = 0; i <= n; i++)
            for (int j = 0; j <= k; j++)
                memo[i, j] = -1;

        sw.Restart();
        long r3 = 0;
        for (int i = 0; i < repeats; i++)
            r3 = CnkPascalMemo(n, k);
        sw.Stop();
        Console.WriteLine($"3) Паскаль + мемоизация: {r3} | Время: {sw.Elapsed.TotalMilliseconds} мс");
    }


    #endregion

    #region Task3

    static void Task3()
    {
        Console.Write("Введите n (целое >=0): ");
        if (!int.TryParse(Console.ReadLine(), out int n) || n < 0)
        {
            Console.WriteLine("Некорректное n.");
            return;
        }
        Console.Write("Введите k (0..n): ");
        if (!int.TryParse(Console.ReadLine(), out int k) || k < 0 || k > n)
        {
            Console.WriteLine("Некорректное k.");
            return;
        }

        CompareIterative(n, k);
    }

    static void Task3Demo()
    {
        int n = 15, k = 7;
        Console.WriteLine($"Демо: n={n}, k={k}");
        CompareIterative(n, k);
    }

    static long CnkIter2D(int n, int k)
    {
        long[,] c = new long[n + 1, k + 1];

        for (int i = 0; i <= n; i++)
        {
            int maxJ = Math.Min(i, k);
            c[i, 0] = 1;
            for (int j = 1; j <= maxJ; j++)
            {
                if (j == i) c[i, j] = 1;
                else
                    c[i, j] = c[i - 1, j - 1] + c[i - 1, j];
            }
        }

        return c[n, k];
    }

    static long CnkIter1D(int n, int k)
    {
        long[] c = new long[k + 1];
        c[0] = 1;

        for (int i = 1; i <= n; i++)
        {
            int maxJ = Math.Min(i, k);
            for (int j = maxJ; j > 0; j--)
            {
                c[j] += c[j - 1];
            }
        }

        return c[k];
    }

    static void CompareIterative(int n, int k)
    {
        const int repeats = 10000;

        Console.WriteLine($"Вычисляем C({n},{k}) итерационными методами и сравниваем с рекурсией, повторений {repeats}...");

        var sw = Stopwatch.StartNew();
        long r1 = 0;
        for (int i = 0; i < repeats; i++)
            r1 = CnkIter2D(n, k);
        sw.Stop();
        Console.WriteLine($"1) 2D матрица: {r1} | Время: {sw.ElapsedMilliseconds} мс");

        sw.Restart();
        long r2 = 0;
        for (int i = 0; i < repeats; i++)
            r2 = CnkIter1D(n, k);
        sw.Stop();
        Console.WriteLine($"2) 1D массив: {r2} | Время: {sw.ElapsedMilliseconds} мс");

        memo = new long[n + 1, k + 1];
        for (int i = 0; i <= n; i++)
            for (int j = 0; j <= k; j++)
                memo[i, j] = -1;

        sw.Restart();
        long r3 = 0;
        for (int i = 0; i < repeats; i++)
            r3 = CnkPascalMemo(n, k);
        sw.Stop();
        Console.WriteLine($"3) Паскаль + мемоизация (рекурсивно): {r3} | Время: {sw.ElapsedMilliseconds} мс");
    }


    #endregion
}
