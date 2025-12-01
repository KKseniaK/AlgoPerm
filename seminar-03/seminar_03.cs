using System;
using System.Numerics;

class seminar_03
{
    static void Main()
    {
        while (true)
        {
            Console.WriteLine("Меню:");
            Console.WriteLine("1 - Task1 (3 города, дороги + петли, взаимная рекурсия)");
            Console.WriteLine("2 - Task2 (позже добавим)");
            Console.WriteLine("0 - Выход");
            Console.Write("Выбор: ");

            string choice = Console.ReadLine()?.Trim();

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
        Console.WriteLine("2 - Демо (пример: AA=1 AB=1 BC=1 CA=1 BB=0 CC=0, n=5, finish=A -> 41)");
        Console.Write("Выбор: ");
        string mode = Console.ReadLine()?.Trim();

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
        string aa = "1", ab = "1", bc = "1", ca = "1", bb = "0", cc = "0";
        int n = 5;
        int finish = 0; // A по умолчанию в демо

        Console.WriteLine("\nДемо:");
        Console.WriteLine("AA AB BC CA BB CC = 1 1 1 1 0 0");
        Console.WriteLine("n = 5");
        Console.WriteLine("finish = A");
        Console.WriteLine();

        string ansLong = CountRoutesFromA_Long(aa, ab, bc, ca, bb, cc, n, finish);
        Console.WriteLine("Ответ (long): " + ansLong);
        Console.WriteLine("Ответ (BigInteger): " + CountRoutesFromA_Big(aa, ab, bc, ca, bb, cc, n, finish));
        Console.WriteLine("Ожидаемо: 41");
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

    #region Task2
    static void Task2()
    {
        Console.WriteLine("\nTask2: пока не вставлен. Кинешь код/условие — подключим сюда.\n");
    }
    #endregion
}
