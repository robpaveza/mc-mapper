using MCBERenderer.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCBERenderer
{
    public static class Renderer
    {
        public static IRenderer Create(Dictionary<int, BiomeRenderConfiguration> biomeConfig)
        {
            return new ImageSharpRenderer(biomeConfig);
        }
    }
}
