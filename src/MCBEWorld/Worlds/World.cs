using MCBEWorld.Nbt;
using MCBEWorld.Utility;
using MCBEWorld.Worlds.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MCBEWorld.Worlds
{
    public sealed class World : IDisposable
    {
        private const int OFFSET_OF_CHUNK_BIOMES_DATA = 512;
        private const int NUMBER_OF_DATA_VALUES_IN_CHUNK = 256;

        private string path_;
        private DbBroker db_;
        private INbtDictionary levelDat_;
        private Dictionary<int, DimensionBitmap> boundaries_;
        private List<string> discoveredKeys_;

        private int spawnX_;
        private int spawnZ_;
        private string name_;
        private long seed_;
        private Difficulty difficulty_;
        private Version lastLoadVersion_;
        private bool commandBlocksEnabled_;
        private bool commandsEnabled_;
        private bool hasBeenLoadedInCreative_;
        private int chunkTickRange_;
        private GameType mode_;

        public World(string path)
        {
            path_ = path;
            
            using (var s = File.OpenRead(Path.Combine(path, "level.dat")))
            {
                s.Seek(8, SeekOrigin.Begin);
                var nbtStream = new NbtStream(s);
                if (!nbtStream.MoveNext())
                {
                    throw new InvalidDataException("Unable to read level.dat.");
                }
                levelDat_ = nbtStream.CurrentValue.AsDictionary();
            }

            spawnX_ = levelDat_[LevelDatConstants.SPAWN_X_KEY].As<int>().Value;
            spawnZ_ = levelDat_[LevelDatConstants.SPAWN_Z_KEY].As<int>().Value;
            difficulty_ = (Difficulty)levelDat_[LevelDatConstants.DIFFICULTY_KEY].As<int>().Value;
            var versionTag = levelDat_[LevelDatConstants.VERSION_KEY].AsArray<int>();
            lastLoadVersion_ = new Version(
                versionTag[0],
                versionTag[1],
                versionTag[2],
                versionTag[3]
                );
            commandBlocksEnabled_ = levelDat_.Contains(LevelDatConstants.COMMAND_BLOCKS_ENABLED_KEY) ? levelDat_[LevelDatConstants.COMMAND_BLOCKS_ENABLED_KEY].As<byte>().Value == 1 : false;
            commandsEnabled_ = levelDat_.Contains(LevelDatConstants.COMMANDS_ENABLED_KEY) ? levelDat_[LevelDatConstants.COMMANDS_ENABLED_KEY].As<byte>().Value == 1 : false;
            hasBeenLoadedInCreative_ = levelDat_[LevelDatConstants.LOADED_IN_CREATIVE_KEY].As<byte>().Value == 1;
            chunkTickRange_ = levelDat_.Contains(LevelDatConstants.TICK_RANGE_KEY) ? levelDat_[LevelDatConstants.TICK_RANGE_KEY].As<int>().Value : 4;
            name_ = levelDat_[LevelDatConstants.NAME_KEY].As<string>().Value;
            seed_ = levelDat_[LevelDatConstants.SEED_KEY].As<long>().Value;
            mode_ = (GameType)levelDat_[LevelDatConstants.GAME_MODE_KEY].As<int>().Value;
            db_ = new DbBroker(Path.Combine(path_, "db"));
        }

        #region simple getter properties
        public string Name => name_;
        public long Seed => seed_;
        public int SpawnX => spawnX_;
        public int SpawnZ => spawnZ_;
        public Difficulty Difficulty => difficulty_;
        public Version LastLoadVersion => lastLoadVersion_;
        public bool AreCommandBlocksEnabled => commandBlocksEnabled_;
        public bool AreCommandsEnabled => commandsEnabled_;
        public bool HasBeenLoadedInCreative => hasBeenLoadedInCreative_;
        public int ServerChunkTickRange => chunkTickRange_;
        public GameType GameType => mode_;
        #endregion

        public Task DiscoverBoundaries()
        {
            if (boundaries_ != null)
            {
                return Task.CompletedTask;
            }

            return Task.Run(() =>
            {
                var overworld = new DimensionBitmap(Dimension.OVERWORLD);
                var nether = new DimensionBitmap(Dimension.NETHER);
                var theEnd = new DimensionBitmap(Dimension.END);
                List<string> discoveredNamedKeys = new List<string>();

                int i = 0;
                foreach (var entry in db_.EnumerateDatabase())
                {
                    Console.WriteLine($"{i++}");
                    if (entry.IsCoordinates)
                    {
                        var coords = entry.Coordinates;
                        if (coords.Dimension.Value == Dimension.DIMENSION_ID_OVERWORLD)
                        {
                            overworld.SetChunk(coords.X, coords.Z);
                        }
                        else if (coords.Dimension.Value == Dimension.DIMENSION_ID_NETHER)
                        {
                            nether.SetChunk(coords.X, coords.Z);
                        }
                        else // TODO: More dimensions?
                        {
                            theEnd.SetChunk(coords.X, coords.Z);
                        }
                    }
                    else
                    {
                        discoveredNamedKeys.Add(entry.NamedKey);
                    }
                }

                discoveredKeys_ = discoveredNamedKeys;
                boundaries_ = new Dictionary<int, DimensionBitmap>()
                {
                    { Dimension.DIMENSION_ID_OVERWORLD, overworld },
                    { Dimension.DIMENSION_ID_NETHER, nether },
                    { Dimension.DIMENSION_ID_THE_END, theEnd },
                };
            });
        }

        public DimensionBitmap Overworld
        {
            get
            {
                if (boundaries_ == null)
                {
                    throw new NotExploredException();
                }

                return boundaries_[Dimension.DIMENSION_ID_OVERWORLD];
            }
        }

        public DimensionBitmap Nether
        {
            get
            {
                if (boundaries_ == null)
                {
                    throw new NotExploredException();
                }

                return boundaries_[Dimension.DIMENSION_ID_NETHER];
            }
        }

        public DimensionBitmap TheEnd
        {
            get
            {
                if (boundaries_ == null)
                {
                    throw new NotExploredException();
                }

                return boundaries_[Dimension.DIMENSION_ID_THE_END];
            }
        }

        public bool TryGetChunkView(
            Dimension dimension,
            int chunkX,
            int chunkZ,
            out ChunkView view,
            bool getBiomeData = false,
            bool getHeightmap = false,
            bool getSubchunkData = false,
            bool getVersion = false,
            bool getEntities = false,
            bool getBlockEntities = false,
            bool getExtraBlockData = false,
            bool getPendingTickList = false,
            bool getFinalizedState = false,
            bool getBorderBlocks = false,
            bool getHardcodedSpawners = false
        )
        {
            if (!CheckDimensionBitmapIfAvailable(dimension, chunkX, chunkZ))
            {
                view = null;
                return false;
            }

            byte[] biomeData = null, heightmap = null;
            if (getBiomeData || getHeightmap)
            {
                byte[] data2D = db_.Query(dimension, chunkX, chunkZ, DbKeyType.Data2D);
                if (data2D != null)
                {
                    heightmap = new byte[512];
                    biomeData = new byte[256];
                    Buffer.BlockCopy(data2D, 0, heightmap, 0, NUMBER_OF_DATA_VALUES_IN_CHUNK * 2);
                    Buffer.BlockCopy(data2D, OFFSET_OF_CHUNK_BIOMES_DATA, biomeData, 0, 256);
                }
            }

            byte[][] subchunkData = null;
            if (getSubchunkData)
            {
                List<byte[]> subchunks = new List<byte[]>();
                int maxYIndex = dimension.Value == Dimension.DIMENSION_ID_NETHER ? 8 : 16;
                for (int y = 0; y < maxYIndex; y++)
                {
                    var subchunk = db_.QuerySubchunkData(dimension, chunkX, y, chunkZ);
                    if (subchunk != null)
                    {
                        subchunks.Add(subchunk);
                    }
                    else
                    {
                        break;
                    }
                }

                if (subchunks.Count > 0)
                {
                    subchunkData = subchunks.ToArray();
                }
            }

            byte[] version = null;
            if (getVersion)
            {
                version = db_.Query(dimension, chunkX, chunkZ, DbKeyType.Version);
            }

            byte[] entities = null;
            if (getEntities)
            {
                entities = db_.Query(dimension, chunkX, chunkZ, DbKeyType.Entity);
            }

            byte[] blockEntities = null;
            if (getBlockEntities)
            {
                blockEntities = db_.Query(dimension, chunkX, chunkZ, DbKeyType.BlockEntity);
            }

            byte[] blockExtraData = null;
            if (getExtraBlockData)
            {
                blockExtraData = db_.Query(dimension, chunkX, chunkZ, DbKeyType.BlockExtraData);
            }

            byte[] pendingTicks = null;
            if (getPendingTickList)
            {
                pendingTicks = db_.Query(dimension, chunkX, chunkZ, DbKeyType.PendingTicks);
            }

            byte[] finalizedState = null;
            if (getFinalizedState)
            {
                finalizedState = db_.Query(dimension, chunkX, chunkZ, DbKeyType.FinalizedState);
            }

            byte[] borderBlocks = null;
            if (getBorderBlocks)
            {
                borderBlocks = db_.Query(dimension, chunkX, chunkZ, DbKeyType.BorderBlocks);
            }

            byte[] hcSpawners = null;
            if (getHardcodedSpawners)
            {
                hcSpawners = db_.Query(dimension, chunkX, chunkZ, DbKeyType.HardcodedSpawners);
            }

            if (biomeData == null && heightmap == null && subchunkData == null && version == null && entities == null &&
                blockEntities == null && blockExtraData == null && pendingTicks == null && finalizedState == null &&
                borderBlocks == null && hcSpawners == null)
            {
                view = null;
                return false;
            }

            view = new ChunkView(
                dimension,
                chunkX,
                chunkZ,
                biomeData,
                heightmap,
                subchunkData,
                version,
                entities,
                blockEntities,
                blockExtraData,
                pendingTicks,
                finalizedState,
                borderBlocks,
                hcSpawners
                );
            return true;
        }

        public bool TryGetRegionView(
            Dimension dimension,
            int regionX,
            int regionZ,
            out ChunkView[] views,
            bool getBiomeData = false,
            bool getHeightmapData = false,
            bool getSubchunkData = false
        )
        {
            if (!(getBiomeData || getHeightmapData || getSubchunkData))
            {
                throw new ArgumentException("Must request at least one of biome, height, and subchunk data.");
            }

            bool foundChunk = false;
            views = new ChunkView[64 * 64];
            int chunkX = regionX * 64;
            int chunkZ = regionZ * 64;
            int destinationIndex = 0;
            for (int z = 0; z < 64; z++)
            {
                for (int x = 0; x < 64; x++)
                {
                    if (TryGetChunkView(dimension, chunkX + x, chunkZ + z, out ChunkView chunk, getBiomeData: getBiomeData, getHeightmap: getHeightmapData, getSubchunkData: getSubchunkData))
                    {
                        foundChunk = true;
                        views[destinationIndex] = chunk;
                    }

                    destinationIndex++;
                }
            }

            if (!foundChunk)
            {
                views = null;
                return false;
            }

            return true;
        }

        /// <summary>
        /// If true, indicates that the database should be queried for the specified Dim/X/Z data. If false,
        /// it means we've already explored the boundaries and determined that the chunk is NOT in the 
        /// specified world.
        /// </summary>
        private bool CheckDimensionBitmapIfAvailable(Dimension dimension, int chunkX, int chunkZ)
        {
            if (boundaries_ == null || !boundaries_.TryGetValue(dimension.Value, out DimensionBitmap toCheck))
            {
                return true;
            }

            return toCheck.HasChunk(chunkX, chunkZ);
        }

        #region static methods
        public static bool TryLoad(string path, out World world)
        {
            try
            {
                world = new World(path);
                return true;
            }
            catch
            {
                world = null;
                return false;
            }
        }

        public static IEnumerable<World> DiscoverWorldsFromUwpInstall()
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Packages\Microsoft.MinecraftUWP_8wekyb3d8bbwe\LocalState\games\com.mojang\minecraftWorlds");
            foreach (string subdir in Directory.GetDirectories(path))
            {
                if (TryLoad(subdir, out World world))
                {
                    yield return world;
                }
                else
                {
                    Console.Error.WriteLine($"Unable to load: '{subdir}'");
                }
            }
        }
        #endregion

        #region IDisposable implementation

        public void Dispose()
        {
            Dispose(true);
        }

        ~World()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (db_ != null)
            {
                db_.Dispose();
                db_ = null;
            }
        }
        #endregion
    }
}
