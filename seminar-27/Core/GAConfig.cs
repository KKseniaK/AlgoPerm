using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace seminar_27.Core
{
    public sealed class GAConfig
    {
        public int PopulationSize { get; init; } = 6;
        public int ParentCount { get; init; } = 4;
        public int EliteCount { get; init; } = 3;
        public int ChildCount { get; init; } = 3;

        public int MaxGenerations { get; init; } = 50;
        public int StagnationLimit { get; init; } = 5;

        public int TournamentSize { get; init; } = 2;

        public SelectionMethod SelectionMethod { get; init; } = SelectionMethod.Elite;
    }
}
