using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using MCBEMapper.Mvc;
using System.IO;
using MCBEWorld.Worlds;

namespace MCBEMapper.Controllers
{
    public class BiomeController : Controller
    {
        private IWorldInstance world_;
        public BiomeController(IWorldInstance world)
        {
            world_ = world;
        }

        public IActionResult Index()
        {
            return File("~/Data/biomes.json", "application/json");
        }

        public IActionResult Chunk(int dim, int chunkX, int chunkZ)
        {
            if (world_.World.TryGetChunkView(Dimension.From(dim), chunkX, chunkZ, out ChunkView view, getBiomeData: true))
            {
                using (var ms = new MemoryStream())
                using (var rendered = world_.Renderer.RenderBiomesTile(view))
                {
                    rendered.WriteToStream(ms);
                    return File(ms.ToArray(), "image/png");
                }
            }

            return NotFound();
        }

        public IActionResult Tile1024(int dim, int tileX, int tileZ)
        {
            if (world_.World.TryGetRegionView(Dimension.From(dim), tileX, tileZ, out ChunkView[] views, getBiomeData: true))
            {
                using (var ms = new MemoryStream())
                using (var rendered = world_.Renderer.RenderBiomesRegionTile(views, 64))
                {
                    rendered.WriteToStream(ms);
                    return File(ms.ToArray(), "image/png");
                }
            }

            return NotFound();
        }
    }
}