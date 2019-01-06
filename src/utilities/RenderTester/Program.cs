using MCBERenderer;
using MCBERenderer.Configuration;
using MCBEWorld.Worlds;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RenderTester
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

            do
            {
                for (int i = 0; i < worlds.Count; i++)
                {
                    Console.WriteLine($"[{i}]: {worlds[i].Name} ({worlds[i].GameType}, {worlds[i].Difficulty}, v{worlds[i].LastLoadVersion})");
                }
                Console.WriteLine($"{worlds.Count} <exit>");
                int index = await GetNumberAsync($"Enter a number to explore [0-{worlds.Count - 1}]: ", 0, worlds.Count + 1);
                // await ExploreWorldWithoutBoundsAsync(worlds[index]);
                if (index == worlds.Count)
                {
                    break;
                }
                await VisitWorldAsync(worlds[index]);
            }
            while (true);
        }

        private static async Task VisitWorldAsync(World world)
        {
            do
            {
                Console.WriteLine($"Exploring {world.Name}");
                Console.WriteLine("Choose dimension:");
                Console.WriteLine("0. Overworld");
                Console.WriteLine("1. The Nether");
                Console.WriteLine("2. The End");
                Console.WriteLine("3. <choose another world>");

                int index = await GetNumberAsync("Choose one of the above options [0-3]: ", 0, 4);
                if (index == 3)
                {
                    break;
                }

                await VisitDimensionByRegionsAsync(world, index);
            } while (true);
        }

        private static async Task VisitDimensionByRegionsAsync(World world, int index)
        {
            var renderer = Renderer.Create(LoadBiomesConfig());
            var dim = Dimension.From(index);
            do
            {
                Console.WriteLine($"Exploring {world.Name}, {dim.Name}");
                int x = await GetNumberAsync($"Select a region X coordinate: ");
                int z = await GetNumberAsync($"Select a region Z coordinate: ");

                if (world.TryGetRegionView(dim, x, z, out ChunkView[] chunkViews, getBiomeData: true))
                {
                    var result = renderer.RenderBiomesRegionTile(chunkViews, 64);
                    if (result == null)
                    {
                        Console.WriteLine("Failed to render region.");
                    }
                    else
                    {
                        result.WriteToFile($"{world.Seed}-{dim.Name}-r{x},{z}.png");
                    }
                }
                else
                {
                    Console.WriteLine($"No chunks at region ({x}, {z}).");
                }

                Console.Write("Visit another region? [y/n] ");
                if (Console.ReadLine() != "y")
                {
                    break;
                }
            }
            while (true);
        }

        private static async Task VisitDimensionAsync(World world, int index)
        {
            var renderer = Renderer.Create(LoadBiomesConfig());
            var dim = Dimension.From(index);
            do
            {
                Console.WriteLine($"Exploring {world.Name}, {dim.Name}");
                int x = await GetNumberAsync($"Select a chunk X coordinate: ");
                int z = await GetNumberAsync($"Select a chunk Z coordinate: ");

                if (world.TryGetChunkView(dim, x, z, out ChunkView chunkView, getBiomeData: true))
                {
                    var result = renderer.RenderBiomesTile(chunkView);
                    if (result == null)
                    {
                        Console.WriteLine("Failed to render chunk.");
                    }
                    else
                    {
                        result.WriteToFile($"{world.Seed}-{dim.Name}-{x}-{z}.png");
                    }
                }
                else
                {
                    Console.WriteLine($"No chunk at ({x}, {z}).");
                }

                Console.Write("Visit another chunk? [y/n] ");
                if (Console.ReadLine() != "y")
                {
                    break;
                }
            }
            while (true);
        }

        private static Dictionary<int, BiomeRenderConfiguration> LoadBiomesConfig()
        {
            Dictionary<int, BiomeRenderConfiguration> result = new Dictionary<int, BiomeRenderConfiguration>();

            string json = File.ReadAllText("biomes.json");
            JArray biomes = JArray.Parse(json);
            foreach (var token in biomes)
            {
                var biome = token as JObject;
                var bc = BiomeRenderConfiguration.FromJson(biome);
                result.Add(bc.ID, bc);
            }

            return result;
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
