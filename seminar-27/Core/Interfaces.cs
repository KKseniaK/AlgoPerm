using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace seminar_27.Core
{
    public enum OptimizationGoal
    {
        Maximize,
        Minimize
    }

    public enum SelectionMethod
    {
        Elite,
        Tournament,
        Roulette
    }

    public enum CrossoverType
    {
        OnePoint,
        TwoPoint,
        Uniform
    }

    public interface IGeneticProblem<TChromosome>
    {
        string Name { get; }
        OptimizationGoal Goal { get; }

        TChromosome CreateRandomChromosome(Random random);
        TChromosome CloneChromosome(TChromosome chromosome);

        double EvaluateFitness(TChromosome chromosome);
        bool IsValid(TChromosome chromosome);
        bool AreEqual(TChromosome left, TChromosome right);

        (TChromosome Child1, TChromosome Child2) Crossover(
            TChromosome parent1,
            TChromosome parent2,
            CrossoverType crossoverType,
            Random random);

        TChromosome Mutate(TChromosome chromosome, Random random);

        string ChromosomeToString(TChromosome chromosome);
    }
}
