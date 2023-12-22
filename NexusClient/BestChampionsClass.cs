using System.Collections.Generic;

namespace NexusClient;

public class BestChampionsClass
{
    public class Root
    {
        public List<Stat> stats { get; set; }
    }

    public class Stat
    {
        public string name { get; set; }
        public double winrate { get; set; }
    }
}