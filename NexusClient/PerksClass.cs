using System.Collections.Generic;

namespace NexusClient;

public class PerksClass
{
    public class Root
    {
        public int id { get; set; }
        public string key { get; set; }
        public string icon { get; set; }
        public string name { get; set; }
        public List<Slot> slots { get; set; }
    }

    public class Rune
    {
        public int id { get; set; }
        public string key { get; set; }
        public string icon { get; set; }
        public string name { get; set; }
        public string shortDesc { get; set; }
        public string longDesc { get; set; }
    }

    public class Slot
    {
        public List<Rune> runes { get; set; }
    }
}