using System;
using System.Collections.Generic;
using System.Text;

namespace MCBEWorld.Worlds
{
    public class Dimension
    {
        private static readonly Dictionary<int, Dimension> known_ = new Dictionary<int, Dimension>();

        public const int DIMENSION_ID_OVERWORLD = 0;
        public const int DIMENSION_ID_NETHER = 1;
        public const int DIMENSION_ID_THE_END = 2;

        public static readonly Dimension OVERWORLD = new Dimension(DIMENSION_ID_OVERWORLD, "Overworld");
        public static readonly Dimension NETHER = new Dimension(DIMENSION_ID_NETHER, "The Nether");
        public static readonly Dimension END = new Dimension(DIMENSION_ID_THE_END, "The End");

        public static Dimension From(int id)
        {
            if (known_.TryGetValue(id, out Dimension result))
            {
                return result;
            }

            return new Dimension(id, $"Unknown dimension #{id}");
        }

        public Dimension(int value, string name)
        {
            Value = value;
            Name = name;

            known_.Add(value, this);
        }

        public int Value { get; }
        public string Name { get; }
    }
}
