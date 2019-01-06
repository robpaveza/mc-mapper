using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MCBEWorld.Worlds;
using MCBERenderer.Configuration;
using MCBERenderer.Explorer;

namespace MCBERenderer
{
    internal sealed class ImageSharpRenderer : IRenderer
    {
        private static readonly byte[] EMPTY_BIOME_MAP = Enumerable.Range(0, 256).Select(_ => (byte)255).ToArray();
        private Dictionary<int, BiomeRenderConfiguration> biomeConfig_;

        public ImageSharpRenderer(Dictionary<int, BiomeRenderConfiguration> biomeConfig)
        {
            biomeConfig_ = biomeConfig;
        }

        private sealed class ImageSharpResult : IRenderResult
        {
            private Image<Rgba32> result_;

            public ImageSharpResult(Image<Rgba32> source)
            {
                Debug.Assert(source != null);
                result_ = source;
            }

            public void WriteToFile(string path)
            {
                using (var os = File.OpenWrite(path))
                {
                    WriteToStream(os);
                }
            }

            public void WriteToStream(Stream destination)
            {
                result_.SaveAsPng(destination);
            }

            void IDisposable.Dispose()
            {
                if (result_ != null)
                {
                    result_.Dispose();
                    result_ = null;
                }
            }
        }

        public IRenderResult RenderBiomesTile(ChunkView view)
        {
            if (!view.HasBiomeData)
            {
                throw new ArgumentException("Chunk view must be loaded with biomes data.", nameof(view));
            }

            var pixels = view.GetBiomesArea().Select(biome => biomeConfig_[biome].Rgba32Color).ToArray();
            var image = Image.LoadPixelData(pixels, 16, 16);

            return new ImageSharpResult(image);
        }

        public IRenderResult RenderBiomesRegionTile(ChunkView[] views, int dimension)
        {
            var listOfViews = views.Where(v => (v != null && v.HasBiomeData && v.GetBiomesArea().ToList().IndexOf(47) >= 0)).ToList();
            byte[] biomesAsRegion = BiomeTileCompositor.CompositeChunksIntoRegion(views.Select(v => (v != null && v.HasBiomeData) ? v.GetBiomesArea() : null).ToArray());
            var pixels = biomesAsRegion.Select(biome => biomeConfig_[biome].Rgba32Color).ToArray();
            var image = Image.LoadPixelData(pixels, dimension * 16, dimension * 16);

            return new ImageSharpResult(image);
        }
    }
}
