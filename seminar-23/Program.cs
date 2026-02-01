using System;

public class HungarianAlgorithm
{
    public static void Main()
    {
        Console.WriteLine("Здравствуйте! Я решаю задачу о назначениях для матрицы затрат!");
        Console.WriteLine("Выберите способ задания матрицы:");
        Console.WriteLine("1 - данные с ручного решения");
        Console.WriteLine("2 - самостоятельный ввод");
        Console.WriteLine("3 - генератор тестовых данных");

        int choice = int.Parse(Console.ReadLine());

        int[,] matrix = null;

        switch (choice)
        {
            case 1:
                Console.Clear();
                matrix = new int[,] {
                    {23, 39, 28, 12, 22},
                    {37, 16, 24, 10, 5},
                    {19, 8, 29, 15, 21},
                    {20, 30, 6, 27, 18},
                    {35, 25, 9, 36, 11}
                };
                break;

            case 2:
                Console.Write("Введите размер квадратной матрицы (n): ");
                int n = int.Parse(Console.ReadLine());
                matrix = new int[n, n];

                Console.WriteLine("Введите элементы матрицы построчно (через пробел):");
                for (int i = 0; i < n; i++)
                {
                    Console.Write($"Строка {i + 1}: ");
                    string[] input = Console.ReadLine().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (input.Length != n)
                    {
                        Console.WriteLine("Ошибка: количество чисел не совпадает с размером матрицы.");
                        return;
                    }
                    for (int j = 0; j < n; j++) { matrix[i, j] = int.Parse(input[j]); }
                }
                break;

            case 3:
                Console.Write("Введите размер квадратной матрицы (n): ");
                int size = int.Parse(Console.ReadLine());

                Random rand = new Random();
                matrix = new int[size, size];
                for (int i = 0; i < size; i++)
                {
                    for (int j = 0; j < size; j++)
                    {
                        matrix[i, j] = rand.Next(1, 101);
                    }
                }

                Console.WriteLine("Сгенерированная матрица:");
                for (int i = 0; i < size; i++)
                {
                    for (int j = 0; j < size; j++)
                    {
                        Console.Write(matrix[i, j].ToString().PadLeft(4));
                    }
                    Console.WriteLine();
                }
                Console.WriteLine();
                break;

            default:
                Console.WriteLine("Неверный выбор.");
                return;
        }

        try
        {
            var (assignment, totalCost) = GeneralMethod(matrix);

            Console.WriteLine("Назначение работ:");
            for (int i = 0; i < assignment.Length; i++)
                Console.WriteLine($"Работник {i + 1} → Задача {assignment[i] + 1}");

            Console.WriteLine($"Минимальная стоимость: {totalCost}");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine("Ошибка: " + ex.Message);
        }
    }
    public static (int[] assignment, int totalCost) GeneralMethod(int[,] costMatrix)
    {
        if (costMatrix == null || costMatrix.GetLength(0) != costMatrix.GetLength(1))
            throw new ArgumentException("Матрица должна быть квадратной.");

        int n = costMatrix.GetLength(0);

        int[,] mat = new int[n, n];
        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
                mat[i, j] = costMatrix[i, j];

        //Редукция строк
        for (int i = 0; i < n; i++)
        {
            int min = mat[i, 0];
            for (int j = 1; j < n; j++)
                if (mat[i, j] < min) min = mat[i, j];
            for (int j = 0; j < n; j++)
                mat[i, j] -= min;
        }

        //Редукция столбцов
        for (int j = 0; j < n; j++)
        {
            int min = mat[0, j];
            for (int i = 1; i < n; i++)
                if (mat[i, j] < min) min = mat[i, j];
            for (int i = 0; i < n; i++)
                mat[i, j] -= min;
        }

        // Основной цикл для поиска нулей и максимального паросочетания
        while (true)
        {
            int[] assignment = FindMaximumMatching(mat);

            bool isPerfect = true;
            for (int i = 0; i < n; i++)
                if (assignment[i] == -1)
                    isPerfect = false;

            if (isPerfect)
            {
                int total = 0;
                for (int i = 0; i < n; i++)
                    total += costMatrix[i, assignment[i]];
                return (assignment, total);
            }

            // Покрываем все нули минимальным числом линий
            var (rowCovered, colCovered) = CoverZerosWithMinimumLines(mat, assignment);

            // Ищем минимальное непокрытое значение
            int delta = int.MaxValue;
            for (int i = 0; i < n; i++)
            {
                if (rowCovered[i]) continue;
                for (int j = 0; j < n; j++)
                {
                    if (!colCovered[j] && mat[i, j] < delta)
                        delta = mat[i, j];
                }
            }

            //Корректируем матрицу
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (!rowCovered[i] && !colCovered[j])
                        mat[i, j] -= delta;
                    else if (rowCovered[i] && colCovered[j])
                        mat[i, j] += delta;
                }
            }
        }
    }
    // Поиск максимального паросочетания в двудольном графе
    private static int[] FindMaximumMatching(int[,] mat)
    {
        int n = mat.GetLength(0);
        bool[,] graph = new bool[n, n];
        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
                graph[i, j] = (mat[i, j] == 0);

        int[] matchY = new int[n];
        for (int j = 0; j < n; j++) matchY[j] = -1;

        for (int x = 0; x < n; x++)
        {
            bool[] visited = new bool[n];
            TryAugment(graph, x, visited, matchY);
        }

        int[] assignment = new int[n];
        for (int i = 0; i < n; i++) assignment[i] = -1;
        for (int j = 0; j < n; j++)
            if (matchY[j] != -1)
                assignment[matchY[j]] = j;

        return assignment;
    }

    // Ищем увеличивающийся путь
    private static bool TryAugment(bool[,] graph, int x, bool[] visited, int[] matchY)
    {
        int n = graph.GetLength(0);
        for (int y = 0; y < n; y++)
        {
            if (graph[x, y] && !visited[y])
            {
                visited[y] = true;
                if (matchY[y] == -1 || TryAugment(graph, matchY[y], visited, matchY))
                {
                    matchY[y] = x;
                    return true;
                }
            }
        }
        return false;
    }

    // Покрываем нули минимальным числом линий
    private static (bool[] rowCovered, bool[] colCovered) CoverZerosWithMinimumLines(int[,] mat, int[] assignment)
    {
        int n = mat.GetLength(0);
        bool[,] starred = new bool[n, n];
        for (int i = 0; i < n; i++)
            if (assignment[i] != -1)
                starred[i, assignment[i]] = true;

        bool[] rowCovered = new bool[n];
        bool[] colCovered = new bool[n];

        //Все строки без отметки
        bool[] markedRows = new bool[n];
        for (int i = 0; i < n; i++)
            if (!HasStarInRow(starred, i, n))
                markedRows[i] = true;

        //Отмечаем столбцы с нулями в отмеченных строках
        bool[] markedCols = new bool[n];
        bool changed = true;
        while (changed)
        {
            changed = false;
            // Отметить столбцы
            for (int i = 0; i < n; i++)
            {
                if (markedRows[i])
                {
                    for (int j = 0; j < n; j++)
                    {
                        if (mat[i, j] == 0 && !markedCols[j])
                        {
                            markedCols[j] = true;
                            changed = true;
                        }
                    }
                }
            }
            for (int j = 0; j < n; j++)
            {
                if (markedCols[j])
                {
                    for (int i = 0; i < n; i++)
                    {
                        if (starred[i, j] && !markedRows[i])
                        {
                            markedRows[i] = true;
                            changed = true;
                        }
                    }
                }
            }
        }

        for (int i = 0; i < n; i++)
            rowCovered[i] = !markedRows[i];
        for (int j = 0; j < n; j++)
            colCovered[j] = markedCols[j];

        return (rowCovered, colCovered);
    }

    private static bool HasStarInRow(bool[,] starred, int row, int n)
    {
        for (int j = 0; j < n; j++)
            if (starred[row, j]) return true;
        return false;
    }
}