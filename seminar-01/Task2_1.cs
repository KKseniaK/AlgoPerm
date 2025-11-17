using System;

namespace seminar_01
{
    class Task2_1
    {
        public static long DeterminantRecursive(long[,] a)
        {
            int n = a.GetLength(0);
            if (n != a.GetLength(1)) throw new ArgumentException("Матрица не квадратная!");

            if (n == 1) return a[0, 0];
            if (n == 2) return a[0, 0] * a[1, 1] - a[0, 1] * a[1, 0];

            long det = 0;
            int sign = 1;

            for (int j = 0; j < n; j++)
            {
                long[,] m = GetMinor(a, 0, j);
                det += sign * a[0, j] * DeterminantRecursive(m);
                sign = -sign;
            }

            return det;
        }

        private static long[,] GetMinor(long[,] a, int row, int col)
        {
            int n = a.GetLength(0);
            long[,] m = new long[n - 1, n - 1];

            int r = 0;
            for (int i = 0; i < n; i++)
            {
                if (i == row) continue;
                int c = 0;
                for (int j = 0; j < n; j++)
                {
                    if (j == col) continue;
                    m[r, c] = a[i, j];
                    c++;
                }
                r++;
            }

            return m;
        }

        static void Main()
        {
            Console.Write("Введите n: ");
            int n = int.Parse(Console.ReadLine());

            long[,] a = new long[n, n];

            Console.WriteLine("Введите матрицу построчно. Например: \n1 2\n3 4\n");
            for (int i = 0; i < n; i++)
            {
                string[] parts = Console.ReadLine().Split();

                for (int j = 0; j < n; j++)
                {
                    a[i, j] = long.Parse(parts[j]);
                }
            }

            long det = DeterminantRecursive(a);
            Console.WriteLine($"Определитель = {det}");
        }

    }
}
