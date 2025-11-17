using System;

namespace seminar_01
{
    class Task1_3
    {
        public static long FibIterativeWithArray(int n)
        {
            if (n <= 0) return -1;
            if (n == 1 || n == 2) return 1;

            long[] fib = new long[n + 1];
            fib[1] = 1;
            fib[2] = 1;

            for (int i = 3; i <= n; i++)
            {
                fib[i] = fib[i - 1] + fib[i - 2];
            }

            return fib[n];
        }

        static void Main()
        {
            int input = int.Parse(Console.ReadLine());
            Console.WriteLine(FibIterativeWithArray(input));
        }
    }
}
