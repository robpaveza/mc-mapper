using MCBEMapper.Mvc;
using MCBERenderer;
using MCBEWorld.Worlds;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MCBEMapper.Data
{
    public class WorldInstance : IWorldInstance, IDisposable
    {
        private Configuration config_;
        private Lazy<World> world_;
        private Lazy<IRenderer> renderer_;

        public WorldInstance(IHostingEnvironment serverContext, string configurationJson)
        {
            JObject configFile = JObject.Parse(configurationJson);
            string path = configFile["path"].Value<string>();
            string biomesJsonPath = configFile["biomes"].Value<string>();
            string biomesJson = File.ReadAllText(Path.Combine(serverContext.ContentRootPath, biomesJsonPath));

            config_ = new Configuration(path, biomesJson);
            world_ = new Lazy<World>(() => new World(path));
            renderer_ = new Lazy<IRenderer>(() => MCBERenderer.Renderer.Create(config_.BiomeRenderingConfiguration));
        }

        public World World => world_?.Value;
        public Configuration Configuration => config_;
        public IRenderer Renderer => renderer_?.Value;

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (world_.IsValueCreated)
                    {
                        world_.Value.Dispose();
                        world_ = null;
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~WorldInstance() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
