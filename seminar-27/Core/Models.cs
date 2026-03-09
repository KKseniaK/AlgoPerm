using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace seminar_27.Core
{
    public sealed class Individual<TChromosome>
    {
        public TChromosome Chromosome { get; set; }
        public double Fitness { get; set; }

        public Individual(TChromosome chromosome, double fitness = 0)
        {
            Chromosome = chromosome;
            Fitness = fitness;
        }
    }

    public sealed class GenerationStats<TChromosome>
    {
        public int GenerationIndex { get; init; }
        public double BestFitness { get; init; }
        public double AverageFitness { get; init; }
        public Individual<TChromosome> BestIndividual { get; init; } = null!;
    }

    public sealed class GAResult<TChromosome>
    {
        public Individual<TChromosome> BestIndividual { get; init; } = null!;
        public List<GenerationStats<TChromosome>> History { get; init; } = new();
    }
}
