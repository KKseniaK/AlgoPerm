using seminar_27.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace seminar_27.Problems
{
    public sealed class TravelingSalesman : IGeneticProblem<int[]>
    {
        private readonly int[,] distances;
        private readonly int cityCount;
        private readonly int startCity;
        private readonly int[] availableCities;

        public string Name => "Traveling Salesman";
        public OptimizationGoal Goal => OptimizationGoal.Minimize;

        public TravelingSalesman(int[,] distances, int startCity = 1)
        {
            if (distances is null)
                throw new ArgumentNullException(nameof(distances));

            var rows = distances.GetLength(0);
            var cols = distances.GetLength(1);

            if (rows == 0 || cols == 0)
                throw new ArgumentException("Матрица расстояний должны быть не нулевой.");

            if (rows != cols)
                throw new ArgumentException("матрица расстояний дб квадратной.");

            if (startCity < 1 || startCity > rows)
                throw new ArgumentException("Индекс стартового города за пределами доступных.");

            this.distances = (int[,])distances.Clone();
            cityCount = rows;
            this.startCity = startCity;
            availableCities = BuildAvailableCities(cityCount, startCity);
        }

        public int[] CreateRandomChromosome(Random random)
        {
            if (random is null)
                throw new ArgumentNullException(nameof(random));

            var chromosome = (int[])availableCities.Clone();

            for (var i = chromosome.Length - 1; i > 0; i--)
            {
                var j = random.Next(i + 1);
                (chromosome[i], chromosome[j]) = (chromosome[j], chromosome[i]);
            }

            return chromosome;
        }

        public int[] CloneChromosome(int[] chromosome)
        {
            if (chromosome is null)
                throw new ArgumentNullException(nameof(chromosome));

            return (int[])chromosome.Clone();
        }

        public double EvaluateFitness(int[] chromosome)
        {
            if (!IsValid(chromosome))
                return double.MaxValue;

            return RouteLength(chromosome);
        }

        public bool IsValid(int[] chromosome)
        {
            if (chromosome is null)
                return false;

            if (chromosome.Length != cityCount - 1)
                return false;

            var used = new HashSet<int>();

            for (var i = 0; i < chromosome.Length; i++)
            {
                var city = chromosome[i];

                if (city < 1 || city > cityCount)
                    return false;

                if (city == startCity)
                    return false;

                if (!used.Add(city))
                    return false;
            }

            return used.Count == cityCount - 1;
        }

        public bool AreEqual(int[] a, int[] b)
        {
            if (a is null || b is null)
                return false;

            return a.SequenceEqual(b);
        }

        public int[] Mutate(int[] chromosome, Random random)
        {
            if (chromosome is null)
                throw new ArgumentNullException(nameof(chromosome));

            if (random is null)
                throw new ArgumentNullException(nameof(random));

            if (chromosome.Length < 2)
                return CloneChromosome(chromosome);

            var mutated = CloneChromosome(chromosome);

            var i = random.Next(mutated.Length);
            var j = random.Next(mutated.Length);

            while (j == i)
                j = random.Next(mutated.Length);

            (mutated[i], mutated[j]) = (mutated[j], mutated[i]);

            return mutated;
        }

        public (int[] Child1, int[] Child2) Crossover(
            int[] parent1,
            int[] parent2,
            CrossoverType type,
            Random random)
        {
            if (parent1 is null || parent2 is null)
                throw new ArgumentNullException();

            if (random is null)
                throw new ArgumentNullException(nameof(random));

            if (!IsValid(parent1) || !IsValid(parent2))
                throw new ArgumentException("Кто-то из родителей сломан");

            return type switch
            {
                CrossoverType.OnePoint => OnePoint(parent1, parent2, random),
                CrossoverType.TwoPoint => TwoPoint(parent1, parent2, random),
                CrossoverType.Uniform => Uniform(parent1, parent2, random),
                _ => throw new InvalidOperationException("Unknown crossover type.")
            };
        }

        public string ChromosomeToString(int[] chromosome)
        {
            if (chromosome is null)
                return "null";

            if (!IsValid(chromosome))
                return $"Invalid: [{string.Join(", ", chromosome)}]";

            var route = new List<int>(chromosome.Length + 2) { startCity };
            route.AddRange(chromosome);
            route.Add(startCity);

            return string.Join(" -> ", route);
        }

        private (int[] Child1, int[] Child2) OnePoint(int[] parent1, int[] parent2, Random random)
        {
            var point = random.Next(1, parent1.Length);

            var child1 = BuildOnePointChild(parent1, parent2, point);
            var child2 = BuildOnePointChild(parent2, parent1, point);

            return (child1, child2);
        }

        private int[] BuildOnePointChild(int[] prefixParent, int[] otherParent, int point)
        {
            var child = new int[prefixParent.Length];
            var used = new HashSet<int>();
            var index = 0;

            for (var i = 0; i < point; i++)
            {
                child[index++] = prefixParent[i];
                used.Add(prefixParent[i]);
            }

            for (var i = 0; i < otherParent.Length; i++)
            {
                var city = otherParent[i];

                if (used.Add(city))
                    child[index++] = city;
            }

            return child;
        }

        private (int[] Child1, int[] Child2) TwoPoint(int[] parent1, int[] parent2, Random random)
        {
            var left = random.Next(0, parent1.Length - 1);
            var right = random.Next(left + 1, parent1.Length);

            var child1 = BuildTwoPointChild(parent1, parent2, left, right);
            var child2 = BuildTwoPointChild(parent2, parent1, left, right);

            return (child1, child2);
        }

        private int[] BuildTwoPointChild(int[] segmentParent, int[] otherParent, int left, int right)
        {
            var child = Enumerable.Repeat(-1, segmentParent.Length).ToArray();
            var used = new HashSet<int>();

            for (var i = left; i < right; i++)
            {
                child[i] = segmentParent[i];
                used.Add(segmentParent[i]);
            }

            var writeIndex = right % child.Length;

            for (var i = 0; i < otherParent.Length; i++)
            {
                var city = otherParent[(right + i) % otherParent.Length];

                if (!used.Add(city))
                    continue;

                while (child[writeIndex] != -1)
                    writeIndex = (writeIndex + 1) % child.Length;

                child[writeIndex] = city;
            }

            return child;
        }

        private (int[] Child1, int[] Child2) Uniform(int[] parent1, int[] parent2, Random random)
        {
            var mask = new bool[parent1.Length];

            for (var i = 0; i < mask.Length; i++)
                mask[i] = random.Next(2) == 1;

            var child1 = BuildUniformChild(parent1, parent2, mask);
            var child2 = BuildUniformChild(parent2, parent1, mask);

            return (child1, child2);
        }

        private int[] BuildUniformChild(int[] preferredParent, int[] otherParent, bool[] mask)
        {
            var child = Enumerable.Repeat(-1, preferredParent.Length).ToArray();
            var used = new HashSet<int>();

            for (var i = 0; i < preferredParent.Length; i++)
            {
                if (!mask[i])
                    continue;

                child[i] = preferredParent[i];
                used.Add(preferredParent[i]);
            }

            var otherIndex = 0;

            for (var i = 0; i < child.Length; i++)
            {
                if (child[i] != -1)
                    continue;

                while (used.Contains(otherParent[otherIndex]))
                    otherIndex++;

                child[i] = otherParent[otherIndex];
                used.Add(otherParent[otherIndex]);
                otherIndex++;
            }

            return child;
        }

        private int RouteLength(int[] chromosome)
        {
            var sum = 0;
            var current = startCity;

            for (var i = 0; i < chromosome.Length; i++)
            {
                var next = chromosome[i];
                sum += Distance(current, next);
                current = next;
            }

            sum += Distance(current, startCity);

            return sum;
        }

        private int Distance(int from, int to)
        {
            return distances[from - 1, to - 1];
        }

        private static int[] BuildAvailableCities(int cityCount, int startCity)
        {
            var result = new int[cityCount - 1];
            var index = 0;

            for (var city = 1; city <= cityCount; city++)
            {
                if (city == startCity)
                    continue;

                result[index++] = city;
            }

            return result;
        }
    }
}
