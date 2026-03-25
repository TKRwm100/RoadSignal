using BveEx.Extensions.MapStatements;
using BveEx.PluginHost;
using BveTypes.ClassWrappers;
using FastMember;
using SlimDX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace BveEx.Toukaitetudou.RoadSignal
{
    internal class SignalController 
    {
        public double Location { get; }
        private readonly bool AR;
        private readonly bool BR;

        private readonly TimeSpan AcG;
        private readonly int Apf;
        private readonly TimeSpan ApF;
        private readonly TimeSpan AcA;

        private readonly TimeSpan BcG;
        private readonly int Bpf;
        private readonly TimeSpan BpF;
        private readonly TimeSpan BcA;

        private readonly TimeSpan sd;

        private readonly TimeSpan AcR;
        private readonly TimeSpan BcR;

        private readonly TimeSpan AcY;
        private readonly TimeSpan BcY;

        private TimeSpan totalElapse;
        private bool isEnable;
        static private int AYSigOffset => 3;
        static private int BYSigOffset => 3;
        private readonly TimeSpan AcGb;
        private readonly TimeSpan ApGf;
        private readonly TimeSpan ApFf;
        private readonly TimeSpan AcYb;
        private readonly TimeSpan AcYf;
        private readonly TimeSpan AcAf;
        private readonly TimeSpan AcAYf;
        private readonly TimeSpan BcGb;
        private readonly TimeSpan BpGf;
        private readonly TimeSpan BpFf;
        private readonly TimeSpan BcYb;
        private readonly TimeSpan BcYf;
        private readonly TimeSpan BcAf;
        private readonly TimeSpan BcAYf;

        List<(BveTypes.ClassWrappers.Structure OnStructure, BveTypes.ClassWrappers.Structure OffStructure)> BaseStr;
        List<(BveTypes.ClassWrappers.Structure OnStructure, BveTypes.ClassWrappers.Structure OffStructure)> AcGStr;
        List<(BveTypes.ClassWrappers.Structure OnStructure, BveTypes.ClassWrappers.Structure OffStructure)> AcYStr;
        List<(BveTypes.ClassWrappers.Structure OnStructure, BveTypes.ClassWrappers.Structure OffStructure)> AcRStr;
        List<(BveTypes.ClassWrappers.Structure OnStructure, BveTypes.ClassWrappers.Structure OffStructure)> AcAStr;
        List<(BveTypes.ClassWrappers.Structure OnStructure, BveTypes.ClassWrappers.Structure OffStructure)> ApGStr;
        List<(BveTypes.ClassWrappers.Structure OnStructure, BveTypes.ClassWrappers.Structure OffStructure)> ApRStr;

        List<(BveTypes.ClassWrappers.Structure OnStructure, BveTypes.ClassWrappers.Structure OffStructure)> BcGStr;
        List<(BveTypes.ClassWrappers.Structure OnStructure, BveTypes.ClassWrappers.Structure OffStructure)> BcYStr;
        List<(BveTypes.ClassWrappers.Structure OnStructure, BveTypes.ClassWrappers.Structure OffStructure)> BcRStr;
        List<(BveTypes.ClassWrappers.Structure OnStructure, BveTypes.ClassWrappers.Structure OffStructure)> BcAStr;
        List<(BveTypes.ClassWrappers.Structure OnStructure, BveTypes.ClassWrappers.Structure OffStructure)> BpGStr;
        List<(BveTypes.ClassWrappers.Structure OnStructure, BveTypes.ClassWrappers.Structure OffStructure)> BpRStr;
        static IBveHacker BveHacker;
        static FastMethod DrawMethod;

        private readonly object[] args = new object[1];
        public static void Initialize(IBveHacker bveHacker, FastMethod drawMethod)
        {
            BveHacker = bveHacker;
            DrawMethod = drawMethod;
        }

        public static SignalController CreateController(Statement statement)
        {
            SignalController rt;
            Config config;
            double location = statement.Source.Location;
            string filePath = Path.Combine(Path.GetDirectoryName(statement.Source.FileName), statement.Source.Clauses[4].Args[0] as string);
            using (StreamReader sr = new StreamReader(filePath))
            {
                XmlSerializer xs = new XmlSerializer(typeof(Config));
                try
                {
                    config = (Config)xs.Deserialize(sr);
                    rt = new SignalController(config, Path.GetDirectoryName(filePath), location);
                }
                catch (Exception exp)
                {
                    _ = exp;
                    config = null;
                    rt = null;
                    return rt;
                }
            }

            var structure = config.SignalControler.ItemsElementName.Zip(config.SignalControler.Items, (x, y) => (x, y));

            rt.BaseStr = structure.Where(x => x.x == ItemsChoiceType.Base).Select(x => x.y.GetStructure(location, rt.FilePath)).ToList();
            rt.AcGStr = structure.Where(x => x.x == ItemsChoiceType.AcG).Select(x => x.y.GetStructure(location, rt.FilePath)).ToList();
            rt.AcYStr = structure.Where(x => x.x == ItemsChoiceType.AcY).Select(x => x.y.GetStructure(location, rt.FilePath)).ToList();
            rt.AcRStr = structure.Where(x => x.x == ItemsChoiceType.AcR).Select(x => x.y.GetStructure(location, rt.FilePath)).ToList();
            rt.AcAStr = structure.Where(x => x.x == ItemsChoiceType.AcA).Select(x => x.y.GetStructure(location, rt.FilePath)).ToList();
            rt.ApGStr = structure.Where(x => x.x == ItemsChoiceType.ApG).Select(x => x.y.GetStructure(location, rt.FilePath)).ToList();
            rt.ApRStr = structure.Where(x => x.x == ItemsChoiceType.ApR).Select(x => x.y.GetStructure(location, rt.FilePath)).ToList();

            rt.BcGStr = structure.Where(x => x.x == ItemsChoiceType.BcG).Select(x => x.y.GetStructure(location, rt.FilePath)).ToList();
            rt.BcYStr = structure.Where(x => x.x == ItemsChoiceType.BcY).Select(x => x.y.GetStructure(location, rt.FilePath)).ToList();
            rt.BcRStr = structure.Where(x => x.x == ItemsChoiceType.BcR).Select(x => x.y.GetStructure(location, rt.FilePath)).ToList();
            rt.BcAStr = structure.Where(x => x.x == ItemsChoiceType.BcA).Select(x => x.y.GetStructure(location, rt.FilePath)).ToList();
            rt.BpGStr = structure.Where(x => x.x == ItemsChoiceType.BpG).Select(x => x.y.GetStructure(location, rt.FilePath)).ToList();
            rt.BpRStr = structure.Where(x => x.x == ItemsChoiceType.BpR).Select(x => x.y.GetStructure(location, rt.FilePath)).ToList();

            return rt;
        }

        private readonly TimeSpan cycleSpan;
        public string FilePath { get; }
        private SignalController(Config config, string path, double location)
        {
            Location = location;
            FilePath = path;
            ConfigSignalControler sc = config.SignalControler;
            AR = sc.AR;
            BR = sc.BR;
            AcG = sc.AcG.GetTimeSpan();
            Apf = int.Parse(sc.Apf);
            AcA = sc.AcA.GetTimeSpan();
            BcG = sc.BcG.GetTimeSpan();
            Bpf = int.Parse(sc.Bpf);
            BcA = sc.BcA.GetTimeSpan();
            sd = sc.sd.GetTimeSpan();



            AcY = new TimeSpan(0, 0, 0, 3, 0);
            BcY = new TimeSpan(0, 0, 0, 3, 0);
            AcR = new TimeSpan(0, 0, 0, 3, 0);
            BcR = new TimeSpan(0, 0, 0, 3, 0);
            ApF = new TimeSpan(0, 0, 0, 0, 500);
            BpF = new TimeSpan(0, 0, 0, 0, 500);
            totalElapse = new TimeSpan(0);
            isEnable = false;
            cycleSpan = AcG + AcY + AcR + (AR ? AcA + AcY : new TimeSpan(0)) + BcG + BcY + BcR + (BR ? BcA + BcY : new TimeSpan(0));

            AcGb = new TimeSpan(0, 0, 0, 0);
            ApGf = new TimeSpan(0, 0, 0, (int)(AcG.TotalSeconds - Apf * ApF.TotalSeconds - AYSigOffset), 0);
            ApFf = new TimeSpan(0, 0, 0, (int)AcG.TotalSeconds - AYSigOffset);
            AcYb = new TimeSpan(0, 0, 0, (int)AcG.TotalSeconds);
            AcYf = AcYb + AcY;
            AcAf = AcYf + (AR ? AcA : new TimeSpan(0));
            AcAYf = AcAf + (AR ? AcY : new TimeSpan(0));

            BcGb = AcAYf + AcR;
            BpGf = new TimeSpan(0, 0, 0, (int)(BcGb.TotalSeconds + BcG.TotalSeconds - Bpf * BpF.TotalSeconds - BYSigOffset));
            BpFf = new TimeSpan(0, 0, 0, (int)(BcGb.TotalSeconds + BcG.TotalSeconds - BYSigOffset));
            BcYb = new TimeSpan(0, 0, 0, (int)(BcGb.TotalSeconds + BcG.TotalSeconds));
            BcYf = BcYb + BcY;
            BcAf = BcYf + (BR ? BcA : new TimeSpan(0));
            BcAYf = BcAf + (BR ? BcY : new TimeSpan(0));

        }
        public void Draw(Matrix viewMatrix)
        {
            int locationBlack = BveHacker.Scenario.VehicleLocation.BlockIndex * 25;
            args[0] = Direct3DProvider.Instance.Src;
            for (int i = 0; i < BaseStr.Count; ++i)
            {
                (var OnStructure, var OffStructure) = BaseStr[i];
                DrawStructure(OnStructure);
            }
            if (isEnable)
            {
                bool isAcG = (AcGb < totalElapse && totalElapse < AcYb);
                bool isAcY = ((AcYb < totalElapse && totalElapse < AcYf) || (AcAf < totalElapse && totalElapse < AcAYf));
                bool isAcR = (AcYf < totalElapse && totalElapse < AcAf) || (AcAYf < totalElapse || totalElapse < AcGb);
                bool isAcA = AR && (AcYf < totalElapse && totalElapse < AcAf);
                bool isApG = ((AcGb < totalElapse && totalElapse < ApGf) || (ApGf < totalElapse && totalElapse < ApFf && totalElapse.TotalSeconds % ApF.TotalSeconds < ApF.TotalSeconds / 2));
                bool isApR = (ApFf < totalElapse || totalElapse < AcGb);
                bool isBcG = (BcGb < totalElapse && totalElapse < BcYb);
                bool isBcY = ((BcYb < totalElapse && totalElapse < BcYf) || (BcAf < totalElapse && totalElapse < BcAYf));
                bool isBcR = (BcYf < totalElapse && totalElapse < BcAf) || (BcAYf < totalElapse || totalElapse < BcGb);
                bool isBcA = BR && (BcYf < totalElapse && totalElapse < BcAf);
                bool isBpG = ((BcGb < totalElapse && totalElapse < BpGf) || (BpGf < totalElapse && totalElapse < BpFf && totalElapse.TotalSeconds % BpF.TotalSeconds < BpF.TotalSeconds / 2));
                bool isBpR = (BpFf < totalElapse || totalElapse < BcGb);

                for (int i = 0; i < AcGStr.Count; ++i)
                {
                    (var OnStructure, var OffStructure) = AcGStr[i];
                    DrawStructure(isAcG ? OnStructure : OffStructure);
                }
                for (int i = 0; i < AcYStr.Count; ++i)
                {
                    (var OnStructure, var OffStructure) = AcYStr[i];
                    DrawStructure(isAcY ? OnStructure : OffStructure);
                }
                for (int i = 0; i < AcRStr.Count; ++i)
                {
                    (var OnStructure, var OffStructure) = AcRStr[i];
                    DrawStructure(isAcR ? OnStructure : OffStructure);
                }
                for (int i = 0; i < AcAStr.Count; ++i)
                {
                    (var OnStructure, var OffStructure) = AcAStr[i];
                    DrawStructure(isAcA ? OnStructure : OffStructure);
                }
                for (int i = 0; i < ApGStr.Count; ++i)
                {
                    (var OnStructure, var OffStructure) = ApGStr[i];
                    DrawStructure(isApG ? OnStructure : OffStructure);
                }
                for (int i = 0; i < ApRStr.Count; ++i)
                {
                    (var OnStructure, var OffStructure) = ApRStr[i];
                    DrawStructure(isApR ? OnStructure : OffStructure);
                }



                for (int i = 0; i < BcGStr.Count; ++i)
                {
                    (var OnStructure, var OffStructure) = BcGStr[i];
                    DrawStructure(isBcG ? OnStructure : OffStructure);
                }
                for (int i = 0; i < BcYStr.Count; ++i)
                {
                    (var OnStructure, var OffStructure) = BcYStr[i];
                    DrawStructure(isBcY ? OnStructure : OffStructure);
                }
                for (int i = 0; i < BcRStr.Count; ++i)
                {
                    (var OnStructure, var OffStructure) = BcRStr[i];
                    DrawStructure(isBcR ? OnStructure : OffStructure);
                }
                for (int i = 0; i < BcAStr.Count; ++i)
                {
                    (var OnStructure, var OffStructure) = BcAStr[i];
                    DrawStructure(isBcA ? OnStructure : OffStructure);
                }
                for (int i = 0; i < BpGStr.Count; ++i)
                {
                    (var OnStructure, var OffStructure) = BpGStr[i];
                    DrawStructure(isBpG ? OnStructure : OffStructure);
                }
                for (int i = 0; i < BpRStr.Count; ++i)
                {
                    (var OnStructure, var OffStructure) = BpRStr[i];
                    DrawStructure(isBpR ? OnStructure : OffStructure);
                }
            }
            else
            {
                for (int i = 0; i < AcGStr.Count; ++i)
                {
                    (_, var OffStructure) = AcGStr[i];
                    DrawStructure(OffStructure);
                }
                for (int i = 0; i < AcYStr.Count; ++i)
                {
                    (_, var OffStructure) = AcYStr[i];
                    DrawStructure(OffStructure);
                }
                for (int i = 0; i < AcRStr.Count; ++i)
                {
                    (var OnStructure, _) = AcRStr[i];
                    DrawStructure(OnStructure);
                }
                for (int i = 0; i < AcAStr.Count; ++i)
                {
                    (_, var OffStructure) = AcAStr[i];
                    DrawStructure(OffStructure);
                }
                for (int i = 0; i < ApGStr.Count; ++i)
                {
                    (_, var OffStructure) = ApGStr[i];
                    DrawStructure(OffStructure);
                }
                for (int i = 0; i < ApRStr.Count; ++i)
                {
                    (var OnStructure, _) = ApRStr[i];
                    DrawStructure(OnStructure);
                }

                for (int i = 0; i < BcGStr.Count; ++i)
                {
                    (_, var OffStructure) = BcGStr[i];
                    DrawStructure(OffStructure);
                }
                for (int i = 0; i < BcYStr.Count; ++i)
                {
                    (_, var OffStructure) = BcYStr[i];
                    DrawStructure(OffStructure);
                }
                for (int i = 0; i < BcRStr.Count; ++i)
                {
                    (var OnStructure, _) = BcRStr[i];
                    DrawStructure(OnStructure);
                }
                for (int i = 0; i < BcAStr.Count; ++i)
                {
                    (_, var OffStructure) = BcAStr[i];
                    DrawStructure(OffStructure);
                }
                for (int i = 0; i < BpGStr.Count; ++i)
                {
                    (_, var OffStructure) = BpGStr[i];
                    DrawStructure(OffStructure);
                }
                for (int i = 0; i < BpRStr.Count; ++i)
                {
                    (var OnStructure, _) = BpRStr[i];
                    DrawStructure(OnStructure);
                }
            }

            void DrawStructure(BveTypes.ClassWrappers.Structure structure)
            {
                Matrix matrix = BveHacker.Scenario.Map.GetTrackMatrix(structure, structure.Location, locationBlack) * viewMatrix;
                Direct3DProvider.Instance.Device.SetTransform(SlimDX.Direct3D9.TransformState.World, matrix);
                DrawMethod.Invoke(structure.Model.Src, args);
            }
        }
        public void Tick(TimeSpan elapse)
        {
            if (isEnable)
            {
                totalElapse += elapse;
                if (totalElapse >= cycleSpan)
                {
                    totalElapse -= cycleSpan;
                }
            }
            else
            {
                if (totalElapse + elapse >= sd)
                {
                    isEnable = true;
                    totalElapse = totalElapse + elapse - sd;
                    return;
                }
                totalElapse += elapse;
            }

        }

    }
}