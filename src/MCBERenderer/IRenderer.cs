using MCBEWorld.Worlds;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MCBERenderer
{
    public interface IRenderer
    {
        IRenderResult RenderBiomesTile(ChunkView view);
        IRenderResult RenderBiomesRegionTile(ChunkView[] chunks, int dimension);
    }

    public interface IRenderResult : IDisposable
    {
        void WriteToStream(Stream destination);

        void WriteToFile(string path);
    }
}
