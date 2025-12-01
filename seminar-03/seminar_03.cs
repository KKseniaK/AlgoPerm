using System;
using System.Numerics;

class seminar_03
{
    static void Main()
    {
        // ===== ВВОД ДАННЫХ =====
        // 1 строка: три числа AB BC CA (сколько дорог между A-B, B-C, C-A)
        var p = Console.ReadLine().Split();
        string abStr = p[0], bcStr = p[1], caStr = p[2];

        // 2 строка: n — длина маршрута (сколько шагов/дней)
        int n = int.Parse(Console.ReadLine());

        // 3 строка: куда надо прийти (A/B/C)
        int finish = CityIndex(Console.ReadLine());

        // ===== ВЫБОР ВЕРСИИ =====
        // Включена long-версия (может переполниться). BigInteger закомментирована.

        Console.WriteLine(CountRoutesFromA_Long(abStr, bcStr, caStr, n, finish));        // long (может OVERFLOW)
        //Console.WriteLine(CountRoutesFromA_BigInteger(abStr, bcStr, caStr, n, finish)); // BigInteger (точно)
    }

    // Переводит "A"/"B"/"C" в индексы 0/1/2
    static int CityIndex(string s)
    {
        // Берём первый символ, приводим к верхнему регистру
        char c = char.ToUpperInvariant(s.Trim()[0]);

        // Возвращаем индекс города
        return c switch
        {
            'A' => 0,
            'B' => 1,
            'C' => 2,
            _ => 0 // если что-то странное ввели — считаем что это A
        };
    }

    // =========================================================
    // 1) ТРОЙНАЯ ВЗАИМНАЯ РЕКУРСИЯ: LONG (с мемо + overflow)
    // =========================================================

    // Эти 3 переменные — кратности дорог (сколько ребёр между городами)
    static long AB_L, BC_L, CA_L;

    // memoA_L[n] хранит уже посчитанное значение A(n)
    // memoB_L[n] хранит уже посчитанное значение B(n)
    // memoC_L[n] хранит уже посчитанное значение C(n)
    // Тип long? означает: "либо значение есть, либо null (ещё не считали)".
    static long?[] memoA_L, memoB_L, memoC_L;

    // Главная функция для long-версии:
    // возвращает строку, потому что может вернуть "OVERFLOW".
    // Старт всегда из A.
    static string CountRoutesFromA_Long(string ab, string bc, string ca, int n, int finish)
    {
        // Парсим кратности дорог из строк
        AB_L = long.Parse(ab);
        BC_L = long.Parse(bc);
        CA_L = long.Parse(ca);

        // Создаём массивы мемоизации на 0..n
        memoA_L = new long?[n + 1];
        memoB_L = new long?[n + 1];
        memoC_L = new long?[n + 1];

        try
        {
            // В зависимости от финиша возвращаем:
            // 0 -> A(n), 1 -> B(n), 2 -> C(n)
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
            // Если в checked-арифметике произошло переполнение long
            return "OVERFLOW";
        }
    }

    // A_L(n) — число способов оказаться в городе A через n шагов, если старт в A.
    // Рекуррентная формула:
    // A(n) = AB * B(n-1) + CA * C(n-1)
    static long A_L(int n)
    {
        // Если уже считали — сразу возвращаем (это и есть мемоизация)
        if (memoA_L[n].HasValue) return memoA_L[n].Value;

        long res;

        // База рекурсии:
        // за 0 шагов мы в A ровно 1 способом (стоим на месте)
        if (n == 0) res = 1;
        else
        {
            // checked: если переполнение long — будет OverflowException
            checked
            {
                res = AB_L * B_L(n - 1) + CA_L * C_L(n - 1);
            }
        }

        // Запоминаем результат и возвращаем
        memoA_L[n] = res;
        return res;
    }

    // B_L(n) — число способов оказаться в городе B через n шагов, старт в A.
    // Формула:
    // B(n) = AB * A(n-1) + BC * C(n-1)
    static long B_L(int n)
    {
        if (memoB_L[n].HasValue) return memoB_L[n].Value;

        long res;

        // За 0 шагов в B мы не можем оказаться, стартуем в A => 0 способов
        if (n == 0) res = 0;
        else
        {
            checked
            {
                res = AB_L * A_L(n - 1) + BC_L * C_L(n - 1);
            }
        }

        memoB_L[n] = res;
        return res;
    }

    // C_L(n) — число способов оказаться в городе C через n шагов, старт в A.
    // Формула:
    // C(n) = CA * A(n-1) + BC * B(n-1)
    static long C_L(int n)
    {
        if (memoC_L[n].HasValue) return memoC_L[n].Value;

        long res;

        // За 0 шагов в C тоже 0 способов (старт A)
        if (n == 0) res = 0;
        else
        {
            checked
            {
                res = CA_L * A_L(n - 1) + BC_L * B_L(n - 1);
            }
        }

        memoC_L[n] = res;
        return res;
    }

    // =========================================================
    // 2) ТРОЙНАЯ ВЗАИМНАЯ РЕКУРСИЯ: BigInteger (с мемо)
    // =========================================================

    // Аналогичные переменные, но уже типа BigInteger
    static BigInteger AB_B, BC_B, CA_B;

    // Мемоизация для BigInteger
    static BigInteger?[] memoA_B, memoB_B, memoC_B;

    // Главная функция для BigInteger-версии:
    // возвращает BigInteger — переполнения нет.
    static BigInteger CountRoutesFromA_BigInteger(string ab, string bc, string ca, int n, int finish)
    {
        AB_B = BigInteger.Parse(ab);
        BC_B = BigInteger.Parse(bc);
        CA_B = BigInteger.Parse(ca);

        memoA_B = new BigInteger?[n + 1];
        memoB_B = new BigInteger?[n + 1];
        memoC_B = new BigInteger?[n + 1];

        // Выбираем, какое значение вернуть: A(n), B(n) или C(n)
        return finish switch
        {
            0 => A_B(n),
            1 => B_B(n),
            2 => C_B(n),
            _ => A_B(n)
        };
    }

    // A_B(n) — то же самое что A_L(n), только BigInteger
    static BigInteger A_B(int n)
    {
        if (memoA_B[n].HasValue) return memoA_B[n]!.Value;

        // База: A(0)=1
        // Рекурсия: A(n)=AB*B(n-1)+CA*C(n-1)
        BigInteger res = (n == 0) ? 1 : AB_B * B_B(n - 1) + CA_B * C_B(n - 1);

        memoA_B[n] = res;
        return res;
    }

    // B_B(n): B(0)=0, B(n)=AB*A(n-1)+BC*C(n-1)
    static BigInteger B_B(int n)
    {
        if (memoB_B[n].HasValue) return memoB_B[n]!.Value;

        BigInteger res = (n == 0) ? 0 : AB_B * A_B(n - 1) + BC_B * C_B(n - 1);

        memoB_B[n] = res;
        return res;
    }

    // C_B(n): C(0)=0, C(n)=CA*A(n-1)+BC*B(n-1)
    static BigInteger C_B(int n)
    {
        if (memoC_B[n].HasValue) return memoC_B[n]!.Value;

        BigInteger res = (n == 0) ? 0 : CA_B * A_B(n - 1) + BC_B * B_B(n - 1);

        memoC_B[n] = res;
        return res;
    }
}
