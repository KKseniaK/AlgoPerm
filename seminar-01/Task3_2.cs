using System;

namespace seminar_01
{
    class Task3_2
    {
        static long CalculateDeterminant(long a, long b, long c, long n)
        {
            if (n < 1) return -1;
            if (n == 1) return a;
            if (n == 2) return a * a - b * c;

            long x1 = a;
            long x2 = a * a - b * c;

            double discriminant = a * a - 4 * b * c; // дискриминант характеристического уравнения

            if (discriminant > 0) // два разных действительных корня
            {
                double sqrtD = Math.Sqrt(discriminant);
                double r1 = (a + sqrtD) / 2.0;
                double r2 = (a - sqrtD) / 2.0;

                // x_n = A*r1^n + B*r2^n 
                // решение системы уравнений Крамером для вычисления коэффициентов А и Б
                double denom = r1 * r2 * (r2 - r1);

                double A = (x1 * r2 * r2 - x2 * r2) / denom;
                double B = (r1 * x2 - x1 * r1 * r1) / denom;

                double result = A * Math.Pow(r1, n) + B * Math.Pow(r2, n);

                return (long)Math.Round(result);
            }
            else if (discriminant == 0) // один кратный корень
            {
                double r = a / 2.0;

                // x_n = (A + B*n)*r^n
                double B = (x2 - x1 * r) / (r * r);
                double A = (2 * x1 * r - x2) / (r * r);

                double result = (A + B * n) * Math.Pow(r, n);
                return (long)Math.Round(result);
            }
            else
            {
                // D < 0 и корни комплексные
                Console.WriteLine("Дискриминант отрицательный, корни комплексные, формула требует комплексных чисел.");
                return -1;
            }
        }
        D
        static void Main()
        {
            Console.Write("Введите n: ");
            int n = int.Parse(Console.ReadLine());

            Console.Write("Введите a, b, c: ");
            var parts = Console.ReadLine().Split();
            long a = long.Parse(parts[0]);
            long b = long.Parse(parts[1]);
            long c = long.Parse(parts[2]);

            long det = CalculateDeterminant(a, b, c, n);
            Console.WriteLine($"Определитель: {det}");
        }
    }
}
