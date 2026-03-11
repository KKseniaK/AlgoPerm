using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace seminar_27.Core
{
    public sealed class GeneticAlgorithm<TChromosome>
    {
        private readonly IGeneticProblem<TChromosome> problem;
        private readonly GAConfig config;
        private readonly Random random;

        public GeneticAlgorithm(
            IGeneticProblem<TChromosome> problem,
            GAConfig config,
            int? seed = null)
        {
            this.problem = problem;
            this.config = config;
            random = seed.HasValue ? new Random(seed.Value) : new Random();
        }

        public GAResult<TChromosome> Run(bool log = true)
        {
            var population = InitPopulation();
            Evaluate(population);

            var history = new List<GenerationStats<TChromosome>>();
            var bestOverall = Clone(Best(population));
            var stagnation = 0;

            for (var gen = 1; gen <= config.MaxGenerations; gen++)
            {
                Evaluate(population);

                var best = Best(population);
                var avg = population.Average(x => x.Fitness);

                history.Add(new GenerationStats<TChromosome>
                {
                    GenerationIndex = gen,
                    BestFitness = best.Fitness,
                    AverageFitness = avg,
                    BestIndividual = Clone(best)
                });

                if (log)
                {
                    Console.WriteLine(
                        $"[{problem.Name}] Gen {gen}: best={best.Fitness}; avg={avg:F2}; chr={problem.ChromosomeToString(best.Chromosome)}");
                }

                if (Better(best.Fitness, bestOverall.Fitness))
                {
                    bestOverall = Clone(best);
                    stagnation = 0;
                }
                else
                {
                    stagnation++;
                }

                if (stagnation >= config.StagnationLimit)
                    break;

                var parents = PickParents(population);
                var children = MakeChildren(parents);

                population = NextGeneration(population, parents, children);
            }

            return new GAResult<TChromosome>
            {
                BestIndividual = bestOverall,
                History = history
            };
        }

        private List<Individual<TChromosome>> InitPopulation()
        {
            var population = new List<Individual<TChromosome>>();

            while (population.Count < config.PopulationSize)
            {
                var chr = problem.CreateRandomChromosome(random);

                if (!problem.IsValid(chr))
                    continue;

                if (population.Any(x => problem.AreEqual(x.Chromosome, chr)))
                    continue;

                population.Add(new Individual<TChromosome>(chr));
            }

            return population;
        }

        private void Evaluate(List<Individual<TChromosome>> population)
        {
            foreach (var ind in population)
                ind.Fitness = problem.EvaluateFitness(ind.Chromosome);
        }

        private List<Individual<TChromosome>> PickParents(List<Individual<TChromosome>> population)
        {
            return config.SelectionMethod switch
            {
                SelectionMethod.Elite => SelectElite(population, config.ParentCount),
                SelectionMethod.Tournament => SelectTournament(population, config.ParentCount),
                SelectionMethod.Roulette => SelectRoulette(population, config.ParentCount),
                _ => throw new InvalidOperationException()
            };
        }

        private List<Individual<TChromosome>> SelectElite(List<Individual<TChromosome>> pop, int count)
        {
            return Sort(pop)
                .Take(count)
                .Select(Clone)
                .ToList();
        }

        private List<Individual<TChromosome>> SelectTournament(List<Individual<TChromosome>> pop, int count)
        {
            var result = new List<Individual<TChromosome>>();

            while (result.Count < count)
            {
                var group = pop
                    .OrderBy(_ => random.Next())
                    .Take(config.TournamentSize)
                    .ToList();

                result.Add(Clone(Best(group)));
            }

            return result;
        }

        private List<Individual<TChromosome>> SelectRoulette(List<Individual<TChromosome>> pop, int count)
        {
            var result = new List<Individual<TChromosome>>();
            var weights = RouletteWeights(pop);
            var total = weights.Sum();

            if (total <= 0)
                return SelectTournament(pop, count);

            while (result.Count < count)
            {
                var r = random.NextDouble() * total;
                var acc = 0.0;

                for (var i = 0; i < pop.Count; i++)
                {
                    acc += weights[i];

                    if (r <= acc)
                    {
                        result.Add(Clone(pop[i]));
                        break;
                    }
                }
            }

            return result;
        }

        private List<double> RouletteWeights(List<Individual<TChromosome>> pop)
        {
            if (problem.Goal == OptimizationGoal.Maximize)
                return pop.Select(x => Math.Max(0.0, x.Fitness)).ToList();

            var max = pop.Max(x => x.Fitness);
            return pop.Select(x => (max - x.Fitness) + 1e-6).ToList();
        }

        private List<Individual<TChromosome>> MakeChildren(List<Individual<TChromosome>> parents)
        {
            var children = new List<Individual<TChromosome>>();
            var shuffled = parents.OrderBy(_ => random.Next()).ToList();

            for (var i = 0; i + 1 < shuffled.Count && children.Count < config.ChildCount; i += 2)
            {
                var p1 = shuffled[i];
                var p2 = shuffled[i + 1];

                if (problem.AreEqual(p1.Chromosome, p2.Chromosome))
                    continue;

                var (c1, c2) = problem.Crossover(
                    p1.Chromosome,
                    p2.Chromosome,
                    config.CrossoverType,
                    random
                );
                var produced = new[] { c1, c2 };

                foreach (var chr in produced)
                {
                    var candidate = problem.CloneChromosome(chr);

                    if (!problem.IsValid(candidate))
                        candidate = problem.Mutate(candidate, random);

                    if (!problem.IsValid(candidate))
                        continue;

                    if (children.Any(x => problem.AreEqual(x.Chromosome, candidate)) ||
                        parents.Any(x => problem.AreEqual(x.Chromosome, candidate)))
                    {
                        var mutated = problem.Mutate(candidate, random);

                        if (problem.IsValid(mutated) &&
                            !children.Any(x => problem.AreEqual(x.Chromosome, mutated)) &&
                            !parents.Any(x => problem.AreEqual(x.Chromosome, mutated)))
                        {
                            candidate = mutated;
                        }
                        else
                            continue;
                    }

                    children.Add(new Individual<TChromosome>(candidate));

                    if (children.Count >= config.ChildCount)
                        break;
                }
            }

            Evaluate(children);
            return children;
        }

        private List<Individual<TChromosome>> NextGeneration(
            List<Individual<TChromosome>> current,
            List<Individual<TChromosome>> parents,
            List<Individual<TChromosome>> children)
        {
            Evaluate(current);
            Evaluate(parents);
            Evaluate(children);

            var next = new List<Individual<TChromosome>>();

            next.AddRange(
                Sort(parents)
                .Take(config.EliteCount)
                .Select(Clone));

            next.AddRange(
                Sort(children)
                .Take(config.ChildCount)
                .Select(Clone));

            foreach (var ind in Sort(current))
            {
                if (next.Count >= config.PopulationSize)
                    break;

                if (next.Any(x => problem.AreEqual(x.Chromosome, ind.Chromosome)))
                    continue;

                next.Add(Clone(ind));
            }

            return next.Take(config.PopulationSize).ToList();
        }

        private List<Individual<TChromosome>> Sort(List<Individual<TChromosome>> pop)
        {
            return problem.Goal == OptimizationGoal.Maximize
                ? pop.OrderByDescending(x => x.Fitness).ToList()
                : pop.OrderBy(x => x.Fitness).ToList();
        }

        private Individual<TChromosome> Best(List<Individual<TChromosome>> pop)
        {
            return Sort(pop).First();
        }

        private bool Better(double a, double b)
        {
            return problem.Goal == OptimizationGoal.Maximize
                ? a > b
                : a < b;
        }

        private Individual<TChromosome> Clone(Individual<TChromosome> ind)
        {
            return new Individual<TChromosome>(
                problem.CloneChromosome(ind.Chromosome),
                ind.Fitness);
        }
    }
}
