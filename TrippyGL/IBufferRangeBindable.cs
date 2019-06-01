using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrippyGL
{
    public interface IBufferRangeBindable
    {
        void Bind();
        void EnsureBound();

        void BindRange(int bindingIndex, int elementIndex);
        void EnsureBoundRange(int bindingIndex, int elementIndex);
    }
}
