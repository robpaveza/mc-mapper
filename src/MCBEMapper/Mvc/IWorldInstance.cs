using MCBEMapper.Data;
using MCBERenderer;
using MCBEWorld.Worlds;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MCBEMapper.Mvc
{
    public interface IWorldInstance
    {
        Configuration Configuration { get; }
        World World { get; }
        IRenderer Renderer { get; }
    }

    public static class ServiceCollectionExtensions
    {
        public static void AddSingletonWorldInstance(this IServiceCollection services, string configurationPath)
        {
            var env = services.Where(sd => sd.ServiceType == typeof(IHostingEnvironment)).FirstOrDefault()?.ImplementationInstance as IHostingEnvironment;
            string jsonPath = Path.Combine(env.ContentRootPath, configurationPath);
            string json = File.ReadAllText(jsonPath);
            services.Add(new ServiceDescriptor(typeof(IWorldInstance), new WorldInstance(env, json)));
        }
    }
}
