using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MCBEMapper.Mvc;

namespace MCBEMapper.Controllers
{
    public class WorldController : Controller
    {
        public IWorldInstance world_;
        public WorldController(IWorldInstance world)
        {
            world_ = world;
        }

        public IActionResult Index()
        {
            var world = world_.World;
            return Json(new
            {
                world.AreCommandBlocksEnabled,
                world.AreCommandsEnabled,
                Difficulty = world.Difficulty.ToString(),
                GameType = world.GameType.ToString(),
                world.HasBeenLoadedInCreative,
                world.LastLoadVersion,
                world.Name,
                world.Seed,
                world.ServerChunkTickRange,
                world.SpawnX,
                world.SpawnZ,
            });
        }
    }
}