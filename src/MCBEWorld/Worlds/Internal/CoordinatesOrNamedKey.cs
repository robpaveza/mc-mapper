using System;
using System.Collections.Generic;
using System.Text;

namespace MCBEWorld.Worlds.Internal
{
    internal struct CoordinatesOrNamedKey
    {
        public CoordinatesOrNamedKey(Coordinates c)
        {
            Coordinates = c;
            NamedKey = null;
            IsCoordinates = true;
        }

        public CoordinatesOrNamedKey(string key)
        {
            NamedKey = key;
            Coordinates = null;
            IsCoordinates = false;
        }

        public bool IsCoordinates { get; }
        public Coordinates Coordinates { get; }
        public string NamedKey { get; }
    }
}
