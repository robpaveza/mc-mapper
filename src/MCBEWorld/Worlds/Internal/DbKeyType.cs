using System;
using System.Collections.Generic;
using System.Text;

namespace MCBEWorld.Worlds.Internal
{
    internal enum DbKeyType
    {
        Data2D = 45,
        BlockEntity = 49,
        Entity = 50,
        Version = 118,
        BiomeState = 53,
        BlockExtraData = 52,
        PendingTicks = 51,
        SubChunkPrefix = 47,
        FinalizedState = 54,
        ConversionData = 55,
        BorderBlocks = 56,
        HardcodedSpawners = 57,
    }

    internal static class DbKeyTypeExtensions
    {
        private static readonly HashSet<DbKeyType> valid_ = new HashSet<DbKeyType>(new[]
        {
            DbKeyType.Data2D,
            DbKeyType.BiomeState,
            DbKeyType.BlockEntity,
            DbKeyType.Entity,
            DbKeyType.Version,
            DbKeyType.HardcodedSpawners,
            DbKeyType.SubChunkPrefix,
            DbKeyType.FinalizedState,
            DbKeyType.PendingTicks,
            DbKeyType.ConversionData,
            DbKeyType.BorderBlocks,
            DbKeyType.BlockExtraData,
        });

        public static bool IsValid(this DbKeyType type)
        {
            return valid_.Contains(type);
        }
    }
}
