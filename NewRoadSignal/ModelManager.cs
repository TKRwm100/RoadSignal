using BveTypes.ClassWrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BveEx.Toukaitetudou.RoadSignal
{
    internal static class ModelManager
    {
        static DisposableValueDictionary<string, Model> Models=new DisposableValueDictionary<string, Model>();

        public static Model GetOrLoad(string fullPath)
        {
            if (Models.ContainsKey(fullPath))
            {
                return Models[fullPath];
            }
            Model model = Model.FromXFile(fullPath);
            Models.Add(fullPath, model);
            return model;
        }
        public static void Dispose()
        {
            Models.Dispose();
        }
    }
}
