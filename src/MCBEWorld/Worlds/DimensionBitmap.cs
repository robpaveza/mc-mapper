using System;
using System.Collections.Generic;
using System.Text;

namespace MCBEWorld.Worlds
{
    public class DimensionBitmap
    {
        private readonly SortedList<int, Region> regions_;

        public DimensionBitmap(Dimension dimension)
        {
            Dimension = dimension;
            regions_ = new SortedList<int, Region>();
        }

        public Dimension Dimension { get; }

        /// <summary>
        /// Gets whether the chunk at the given Chunk-coordinates has data.
        /// </summary>
        /// <param name="chunkX"></param>
        /// <param name="chunkZ"></param>
        /// <returns></returns>
        public bool HasChunk(int chunkX, int chunkZ)
        {
            int regionX = chunkX >> 6;
            int regionZ = chunkZ >> 6;
            
            int key = ((regionX & 0x3fff) << 14) | (regionZ & 0x3fff);
            if (regions_.TryGetValue(key, out Region region))
            {
                int localX = chunkX & 0x3f;
                int localZ = chunkZ & 0x3f;

                return region.HasData(localX, localZ);
            }

            return false;
        }

        public IEnumerable<Region> GetRegions()
        {
            foreach (var item in regions_.Values)
            {
                yield return item;
            }
        }

        internal void AddRegion(Region region)
        {
            int key = region.Key;

            if (regions_.IndexOfKey(key) != -1)
            {
                throw new ArgumentException("That region is already being tracked.", nameof(region));
            }

            regions_.Add(key, region);
        }

        internal void SetChunk(int chunkX, int chunkZ)
        {
            int regionKey = Region.CalculateKeyFromChunk(chunkX, chunkZ);
            if (regions_.TryGetValue(regionKey, out Region region))
            {
                region.SetHasData(chunkX, chunkZ);
            }
            else
            {
                var newRegion = Region.CreateContainingChunk(chunkX, chunkZ);
                newRegion.SetHasData(chunkX, chunkZ);
                AddRegion(newRegion);
            }
        }
    }
}
