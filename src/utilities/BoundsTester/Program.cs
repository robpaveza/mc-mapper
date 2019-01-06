using MCBEWorld.Utility;
using MCBEWorld.Worlds;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BoundsTester
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync().Wait();
            Console.ReadLine();
        }

        private static async Task MainAsync()
        {
            var worlds = World.DiscoverWorldsFromUwpInstall().ToList();
            worlds.Sort((a, b) =>
            {
                return a.Name.CompareTo(b.Name);
            });

            for (int i = 0; i < worlds.Count; i++)
            {
                Console.WriteLine($"[{i}]: {worlds[i].Name} ({worlds[i].GameType}, {worlds[i].Difficulty}, v{worlds[i].LastLoadVersion})");
            }
            int index = await GetNumberAsync($"Enter a number to explore [0-{worlds.Count - 1}]: ", 0, worlds.Count);
            // await ExploreWorldWithoutBoundsAsync(worlds[index]);
            await ExploreWorldAsync(worlds[index]);
        }

        private static async Task ExploreWorldAsync(World w)
        {
            Console.WriteLine("Discovering boundaries...");
            await w.DiscoverBoundaries();

            var overworld = w.Overworld;
            foreach (var region in overworld.GetRegions())
            {
                Console.WriteLine($"Region at ({region.X}, {region.Z}:");
                Console.WriteLine(region.SerializeToString(BinarySerializationFormat.Base64));
            }

            if (!w.TryGetChunkView(Dimension.OVERWORLD, 0, 0, out ChunkView view, true, true, true, true, true, true, true, true, true, true, true))
            {
                Console.WriteLine("Oops!");
            }
            Console.WriteLine(view.ToString());
        }

        private static Task<int> GetNumberAsync(string prompt, int inclusiveLower, int exclusiveUpper)
        {
            return Task.Run(() =>
            {
                int result = -1;
                bool got = false;
                while (!got)
                {
                    Console.Write(prompt);
                    string response = Console.ReadLine();
                    got = int.TryParse(response, out result);
                    if (got && result >= inclusiveLower && result < exclusiveUpper)
                    {
                        return result;
                    }
                    Console.WriteLine("Invalid input.");
                }

                throw new Exception("Should never be reached.");
            });
        }

        private static Task<int> GetNumberAsync(string prompt)
        {
            return Task.Run(() =>
            {
                int result = -1;
                bool got = false;
                while (!got)
                {
                    Console.Write(prompt);
                    string response = Console.ReadLine();
                    got = int.TryParse(response, out result);
                    if (got)
                    {
                        return result;
                    }
                    Console.WriteLine("Invalid input.");
                }

                throw new Exception("Should never be reached.");
            });
        }
    }
}
