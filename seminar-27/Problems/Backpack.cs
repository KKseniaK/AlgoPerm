using seminar_27.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace seminar_27.Problems
{
    public sealed class BackpackProblem : IGeneticProblem<int[]>
    {
        private readonly int[] weights;
        private readonly int[] costs;
        private readonly int capacity;

        public string Name => "Backpack";
        public OptimizationGoal Goal => OptimizationGoal.Maximize;

        public BackpackProblem(int[] weights, int[] costs, int capacity)
        {
            if (weights is null || costs is null)
                throw new ArgumentNullException();

            if (weights.Length == 0 || costs.Length == 0)
                throw new ArgumentException("Weights and costs must not be empty.");

            if (weights.Length != costs.Length)
                throw new ArgumentException("Weights and costs must have the same length.");

            if (capacity <= 0)
                throw new ArgumentException("Capacity must be positive.");

            this.weights = (int[])weights.Clone();
            this.costs = (int[])costs.Clone();
            this.capacity = capacity;
        }

        public int[] CreateRandomChromosome(Random random)
        {
            var chromosome = new int[weights.Length];

            for (var i = 0; i < chromosome.Length; i++)
                chromosome[i] = random.Next(2);

            return chromosome;
        }

        public int[] CloneChromosome(int[] chromosome)
        {
            return (int[])chromosome.Clone();
        }

        public double EvaluateFitness(int[] chromosome)
        {
            var totalWeight = TotalWeight(chromosome);

            if (totalWeight > capacity)
                return 0;

            return TotalCost(chromosome);
        }

        public bool IsValid(int[] chromosome)
        {
            return TotalWeight(chromosome) <= capacity;
        }

        public bool AreEqual(int[] a, int[] b)
        {
            return a.SequenceEqual(b);
        }

        public int[] Mutate(int[] chromosome, Random random)
        {
            var mutated = CloneChromosome(chromosome);
            var index = random.Next(mutated.Length);

            mutated[index] = mutated[index] == 0 ? 1 : 0;

            return mutated;
        }

        public (int[] Child1, int[] Child2) Crossover(
            int[] parent1,
            int[] parent2,
            CrossoverType type,
            Random random)
        {
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
            return string.Concat(chromosome);
        }

        private (int[] Child1, int[] Child2) OnePoint(int[] parent1, int[] parent2, Random random)
        {
            var point = random.Next(1, parent1.Length - 1);

            var child1 = new int[parent1.Length];
            var child2 = new int[parent1.Length];

            for (var i = 0; i < parent1.Length; i++)
            {
                if (i < point)
                {
                    child1[i] = parent1[i];
                    child2[i] = parent2[i];
                }
                else
                {
                    child1[i] = parent2[i];
                    child2[i] = parent1[i];
                }
            }

            return (child1, child2);
        }

        private (int[] Child1, int[] Child2) TwoPoint(int[] parent1, int[] parent2, Random random)
        {
            var left = random.Next(1, parent1.Length - 2);
            var right = random.Next(left + 1, parent1.Length - 1);

            var child1 = new int[parent1.Length];
            var child2 = new int[parent1.Length];

            for (var i = 0; i < parent1.Length; i++)
            {
                if (i < left || i >= right)
                {
                    child1[i] = parent1[i];
                    child2[i] = parent2[i];
                }
                else
                {
                    child1[i] = parent2[i];
                    child2[i] = parent1[i];
                }
            }

            return (child1, child2);
        }

        private (int[] Child1, int[] Child2) Uniform(int[] parent1, int[] parent2, Random random)
        {
            var child1 = new int[parent1.Length];
            var child2 = new int[parent1.Length];

            for (var i = 0; i < parent1.Length; i++)
            {
                var maskBit = random.Next(2);

                if (maskBit == 1)
                {
                    child1[i] = parent1[i];
                    child2[i] = parent2[i];
                }
                else
                {
                    child1[i] = parent2[i];
                    child2[i] = parent1[i];
                }
            }

            return (child1, child2);
        }

        private int TotalWeight(int[] chromosome)
        {
            var sum = 0;

            for (var i = 0; i < chromosome.Length; i++)
            {
                if (chromosome[i] == 1)
                    sum += weights[i];
            }

            return sum;
        }

        private int TotalCost(int[] chromosome)
        {
            var sum = 0;

            for (var i = 0; i < chromosome.Length; i++)
            {
                if (chromosome[i] == 1)
                    sum += costs[i];
            }

            return sum;
        }
    }
}
