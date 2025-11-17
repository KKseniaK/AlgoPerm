using System;

namespace seminar_01
{
    class Task1_2
    {
        public static long FibIter(int n)
        {
            if (n < 1) return -1;
            if (n <= 2) return 1;

            long a = 1, b = 1;           
            for (int i = 3; i <= n; i++)
            {
                long next = a + b;       
                a = b;
                b = next;
            }
            return b;
        }

        static void Main()
        {
            int input = int.Parse(Console.ReadLine());
            Console.WriteLine(FibIter(input));
        }
    }
}
