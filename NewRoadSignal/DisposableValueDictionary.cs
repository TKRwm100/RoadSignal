using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BveEx.Toukaitetudou.RoadSignal
{
    internal class DisposableValueDictionary<_TKey, _TValue> : Dictionary<_TKey, _TValue>, IDisposable where _TValue : IDisposable
    {
        public void Dispose()
        {
            foreach(_TValue value in Values)
            {
                value?.Dispose();
            }
            Clear();
        }
    }
}
