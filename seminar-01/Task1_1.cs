using System;

namespace seminar_01
{
    class Task1_1
    {
        public static long FibRec(int n)
        {
            if (n < 1) return -1;   
            if (n == 1 || n == 2) return 1;
            return FibRec(n - 1) + FibRec(n - 2);
        }

        static void Main()
        {
            int input = int.Parse(Console.ReadLine());
            Console.WriteLine(FibRec(input));
        }
    }
}
