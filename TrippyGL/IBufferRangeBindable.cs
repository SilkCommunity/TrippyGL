using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrippyGL
{
    public interface IBufferRangeBindable
    {
        void EnsureBoundRange(int bindingIndex, int elementIndex);
    }
}
