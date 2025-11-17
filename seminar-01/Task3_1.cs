using System;

namespace seminar_01
{
    class Task3_1
    {
        public static double DeterminantIterative(double a, double b, double c, int n)
        {
            if (n < 1) return -1;
            if (n == 1) return a;
            if (n == 2) return a * a - b * c;

            //det от матриц размерности 1 и 2
            double prev2 = a;                       
            double prev1 = a * a - b * c;           

            //по рекуррентной формуле в цикле
            for (int k = 3; k <= n; k++)
            {
                double current = a * prev1 - b * c * prev2;
                prev2 = prev1;
                prev1 = current;
            }

            return prev1;
        }

        static void Main()
        {
            Console.Write("Введите n: ");
            int n = int.Parse(Console.ReadLine());
            Console.Write("Введите a, b, c: ");
            var parts = Console.ReadLine().Split();
            double a = double.Parse(parts[0]);
            double b = double.Parse(parts[1]);
            double c = double.Parse(parts[2]);

            double byRec = DeterminantIterative(a, b, c, n);
            Console.WriteLine($"По рекурренте: {byRec}");
        }
    }
}
