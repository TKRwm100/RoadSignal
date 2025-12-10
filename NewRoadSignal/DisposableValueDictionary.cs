using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BveEx.Toukaitetudou.RoadSignal
{
    internal class DisposableDictionary<_TKey, _TValue> : Dictionary<_TKey, _TValue>, IDisposable where _TKey : IDisposable where _TValue : IDisposable
    {
        public void Dispose()
        {
            foreach(KeyValuePair<_TKey,_TValue> pair in this)
            {
                pair.Key?.Dispose();
                pair.Value?.Dispose();
            }
            Clear();
        }
    }
    internal class DisposableKeyDictionary<_TKey, _TValue> : Dictionary<_TKey, _TValue>, IDisposable where _TKey : IDisposable 
    {
        public void Dispose()
        {
            foreach(_TKey key in Keys)
            {
                key?.Dispose();
            }
            Clear() ;
        }
    }
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
