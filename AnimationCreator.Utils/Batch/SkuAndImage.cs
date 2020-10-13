using Microsoft.Azure.Batch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnimationCreator.Utils.Batch
{
    public struct SkuAndImage
    {
        public SkuAndImage(NodeAgentSku sku, ImageReference image)
        {
            this.Sku = sku;
            this.Image = image;
        }

        public NodeAgentSku Sku { get; }
        public ImageReference Image { get; }
    }
}
