using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iPaas.Transformation.Interfaces
{
    public interface ITransformAEAsset
    {
        Task<bool> Transform(string myQueueItem, string corealtionId);
    }
}
