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
    internal class SignalController : IDisposable
    {
        public double Location { get; }
        private readonly bool AR;
        private readonly bool BR;

        TimeSpan AcG;
        private readonly int Apf;
        TimeSpan ApF;
        TimeSpan AcA;

        TimeSpan BcG;
        private readonly int Bpf;
        TimeSpan BpF;
        TimeSpan BcA;

        TimeSpan sd;

        TimeSpan AcR;
        TimeSpan BcR;

        TimeSpan AcY;
        TimeSpan BcY;

        TimeSpan totalElapse;
        bool isEnable;
        static private int AYSigOffset => 3;
        static private int BYSigOffset => 3;
        TimeSpan AcGb;
        TimeSpan ApGf;
        TimeSpan ApFf;
        TimeSpan AcYb;
        TimeSpan AcYf;
        TimeSpan AcAf;
        TimeSpan AcAYf;

        TimeSpan BcGb;
        TimeSpan BpGf;
        TimeSpan BpFf;
        TimeSpan BcYb;
        TimeSpan BcYf;
        TimeSpan BcAf;
        TimeSpan BcAYf;

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

        object[] args = new object[1];
        public static void Initialize(IBveHacker bveHacker,FastMethod drawMethod)
        {
            BveHacker= bveHacker;
            DrawMethod= drawMethod;
        }

        public static SignalController CreateConfigdata(Statement statement)
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
                    rt = new SignalController(config,Path.GetDirectoryName(filePath),location);
                }
                catch (Exception exp)
                {
                    _ = exp;
                    config = null;
                    rt = null;
                    return rt;
                }
            }

            var structure=config.SignalControler.ItemsElementName.Zip(config.SignalControler.Items,(x,y)=>(x,y));

            rt.BaseStr = structure.Where(x=>x.x==ItemsChoiceType.Base).Select(x => x.y.GetStructure(location,rt.FilePath)).ToList();
            rt.AcGStr = structure.Where(x => x.x==ItemsChoiceType.AcG).Select(x => x.y.GetStructure(location, rt.FilePath)).ToList();
            rt.AcYStr = structure.Where(x=>x.x==ItemsChoiceType.AcY).Select(x => x.y.GetStructure(location, rt.FilePath)).ToList();
            rt.AcRStr = structure.Where(x=>x.x==ItemsChoiceType.AcR).Select(x => x.y.GetStructure(location, rt.FilePath)).ToList();
            rt.AcAStr = structure.Where(x=>x.x==ItemsChoiceType.AcA).Select(x => x.y.GetStructure(location, rt.FilePath)).ToList();
            rt.ApGStr = structure.Where(x=>x.x==ItemsChoiceType.ApG).Select(x => x.y.GetStructure(location, rt.FilePath)).ToList();
            rt.ApRStr = structure.Where(x=>x.x==ItemsChoiceType.ApR).Select(x => x.y.GetStructure(location, rt.FilePath)).ToList();

            rt.BcGStr = structure.Where(x=>x.x==ItemsChoiceType.BcG).Select(x => x.y.GetStructure(location, rt.FilePath)).ToList();
            rt.BcYStr = structure.Where(x=>x.x==ItemsChoiceType.BcY).Select(x => x.y.GetStructure(location, rt.FilePath)).ToList();
            rt.BcRStr = structure.Where(x=>x.x==ItemsChoiceType.BcR).Select(x => x.y.GetStructure(location, rt.FilePath)).ToList();
            rt.BcAStr = structure.Where(x=>x.x==ItemsChoiceType.BcA).Select(x => x.y.GetStructure(location, rt.FilePath)).ToList();
            rt.BpGStr = structure.Where(x=>x.x==ItemsChoiceType.BpG).Select(x => x.y.GetStructure(location, rt.FilePath)).ToList();
            rt.BpRStr = structure.Where(x=>x.x==ItemsChoiceType.BpR).Select(x => x.y.GetStructure(location, rt.FilePath)).ToList();

            return rt;
        }

        TimeSpan cycleSpan;
        public string FilePath { get; }
        private SignalController(Config config,string path,double location)
        {
            Location= location;
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
            BcR = new TimeSpan(0, 0, 0, 3,0);
            ApF = new TimeSpan(0, 0, 0, 0, 500);
            BpF = new TimeSpan(0, 0, 0, 0, 500);
            totalElapse = new TimeSpan(0);
            isEnable = false;
            cycleSpan = AcG + AcY + AcR + (AR ? AcA + AcY : new TimeSpan(0)) + BcG + BcY + BcR + (BR ? BcA + BcY : new TimeSpan(0));

            AcGb = new TimeSpan(0, 0, 0, 0);
            ApGf = new TimeSpan(0, 0, 0, (int)(AcG.TotalSeconds - Apf*ApF.TotalSeconds - AYSigOffset), 0);
            ApFf = new TimeSpan(0, 0, 0, (int)AcG.TotalSeconds - AYSigOffset);
            AcYb = new TimeSpan(0, 0, 0, (int)AcG.TotalSeconds);
            AcYf = AcYb + AcY;
            AcAf = AcYf + (AR ? AcA : new TimeSpan(0));
            AcAYf = AcAf + (AR ? AcY : new TimeSpan(0));

            BcGb = AcAYf + AcR;
            BpGf = new TimeSpan(0, 0, 0, (int)(BcGb.TotalSeconds + BcG.TotalSeconds - Bpf*BpF.TotalSeconds - BYSigOffset));
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
            for (int i= 0;i < BaseStr.Count;++i)
            {
                (BveTypes.ClassWrappers.Structure OnStructure, BveTypes.ClassWrappers.Structure OffStructure) strSet = BaseStr[i];
                DrawStructure(strSet.OnStructure);
            }
            if (isEnable)
            {
                foreach ((BveTypes.ClassWrappers.Structure OnStructure, BveTypes.ClassWrappers.Structure OffStructure) strSet in AcGStr)
                {
                    DrawStructure((AcGb < totalElapse && totalElapse < AcYb) ? strSet.OnStructure : strSet.OffStructure);
                }
                foreach ((BveTypes.ClassWrappers.Structure OnStructure, BveTypes.ClassWrappers.Structure OffStructure) strSet in AcYStr)
                {
                    DrawStructure(((AcYb < totalElapse && totalElapse < AcYf) || (AcAf < totalElapse && totalElapse < AcAYf)) ? strSet.OnStructure : strSet.OffStructure);
                }
                foreach ((BveTypes.ClassWrappers.Structure OnStructure, BveTypes.ClassWrappers.Structure OffStructure) strSet in AcRStr)
                {
                    DrawStructure((AcYf < totalElapse && totalElapse < AcAf) || (AcAYf < totalElapse || totalElapse < AcGb) ? strSet.OnStructure : strSet.OffStructure);
                }
                foreach ((BveTypes.ClassWrappers.Structure OnStructure, BveTypes.ClassWrappers.Structure OffStructure) strSet in AcAStr)
                {
                    DrawStructure(
                        AR && (AcYf < totalElapse && totalElapse < AcAf)
                        ? strSet.OnStructure : strSet.OffStructure);
                }
                foreach ((BveTypes.ClassWrappers.Structure OnStructure, BveTypes.ClassWrappers.Structure OffStructure) strSet in ApGStr)
                {
                    DrawStructure(
                        ((AcGb < totalElapse && totalElapse < ApGf) || (ApGf < totalElapse && totalElapse < ApFf && totalElapse.TotalSeconds % ApF.TotalSeconds < ApF.TotalSeconds / 2))
                        ? strSet.OnStructure : strSet.OffStructure);
                }
                foreach ((BveTypes.ClassWrappers.Structure OnStructure, BveTypes.ClassWrappers.Structure OffStructure) strSet in ApRStr)
                {
                    DrawStructure(
                        (ApFf < totalElapse || totalElapse < AcGb)
                        ? strSet.OnStructure : strSet.OffStructure);
                }


                foreach ((BveTypes.ClassWrappers.Structure OnStructure, BveTypes.ClassWrappers.Structure OffStructure) strSet in BcGStr)
                {
                    DrawStructure((BcGb < totalElapse && totalElapse < BcYb) ? strSet.OnStructure : strSet.OffStructure);
                }
                foreach ((BveTypes.ClassWrappers.Structure OnStructure, BveTypes.ClassWrappers.Structure OffStructure) strSet in BcYStr)
                {
                    DrawStructure(((BcYb < totalElapse && totalElapse < BcYf) || (BcAf < totalElapse && totalElapse < BcAYf)) ? strSet.OnStructure : strSet.OffStructure);
                }
                foreach ((BveTypes.ClassWrappers.Structure OnStructure, BveTypes.ClassWrappers.Structure OffStructure) strSet in BcRStr)
                {
                    DrawStructure((BcYf < totalElapse && totalElapse < BcAf) || (BcAYf < totalElapse || totalElapse < BcGb) ? strSet.OnStructure : strSet.OffStructure);
                }
                foreach ((BveTypes.ClassWrappers.Structure OnStructure, BveTypes.ClassWrappers.Structure OffStructure) strSet in BcAStr)
                {
                    DrawStructure(
                        BR && (BcYf < totalElapse && totalElapse < BcAf)
                        ? strSet.OnStructure : strSet.OffStructure);
                }
                foreach ((BveTypes.ClassWrappers.Structure OnStructure, BveTypes.ClassWrappers.Structure OffStructure) strSet in BpGStr)
                {
                    DrawStructure(
                        ((BcGb < totalElapse && totalElapse < BpGf) || (BpGf < totalElapse && totalElapse < BpFf && totalElapse.TotalSeconds % BpF.TotalSeconds < BpF.TotalSeconds / 2))
                        ? strSet.OnStructure : strSet.OffStructure);
                }
                foreach ((BveTypes.ClassWrappers.Structure OnStructure, BveTypes.ClassWrappers.Structure OffStructure) strSet in BpRStr)
                {
                    DrawStructure(
                        (BpFf < totalElapse || totalElapse < BcGb)
                        ? strSet.OnStructure : strSet.OffStructure);
                }
            }
            else
            {
                foreach ((BveTypes.ClassWrappers.Structure OnStructure, BveTypes.ClassWrappers.Structure OffStructure) strSet in AcGStr)
                {
                    DrawStructure(strSet.OffStructure);
                }
                foreach ((BveTypes.ClassWrappers.Structure OnStructure, BveTypes.ClassWrappers.Structure OffStructure) strSet in AcYStr)
                {
                    DrawStructure(strSet.OffStructure);
                }
                foreach ((BveTypes.ClassWrappers.Structure OnStructure, BveTypes.ClassWrappers.Structure OffStructure) strSet in AcRStr)
                {
                    DrawStructure(strSet.OnStructure);
                }
                foreach ((BveTypes.ClassWrappers.Structure OnStructure, BveTypes.ClassWrappers.Structure OffStructure) strSet in AcAStr)
                {
                    DrawStructure(strSet.OffStructure);
                }
                foreach ((BveTypes.ClassWrappers.Structure OnStructure, BveTypes.ClassWrappers.Structure OffStructure) strSet in ApGStr)
                {
                    DrawStructure(strSet.OffStructure);
                }
                foreach ((BveTypes.ClassWrappers.Structure OnStructure, BveTypes.ClassWrappers.Structure OffStructure) strSet in ApRStr)
                {
                    DrawStructure(strSet.OnStructure);
                }

                foreach ((BveTypes.ClassWrappers.Structure OnStructure, BveTypes.ClassWrappers.Structure OffStructure) strSet in BcGStr)
                {
                    DrawStructure(strSet.OffStructure);
                }
                foreach ((BveTypes.ClassWrappers.Structure OnStructure, BveTypes.ClassWrappers.Structure OffStructure) strSet in BcYStr)
                {
                    DrawStructure(strSet.OffStructure);
                }
                foreach ((BveTypes.ClassWrappers.Structure OnStructure, BveTypes.ClassWrappers.Structure OffStructure) strSet in BcRStr)
                {
                    DrawStructure(strSet.OnStructure);
                }
                foreach ((BveTypes.ClassWrappers.Structure OnStructure, BveTypes.ClassWrappers.Structure OffStructure) strSet in BcAStr)
                {
                    DrawStructure(strSet.OffStructure);
                }
                foreach ((BveTypes.ClassWrappers.Structure OnStructure, BveTypes.ClassWrappers.Structure OffStructure) strSet in BpGStr)
                {
                    DrawStructure(strSet.OffStructure);
                }
                foreach ((BveTypes.ClassWrappers.Structure OnStructure, BveTypes.ClassWrappers.Structure OffStructure) strSet in BpRStr)
                {
                    DrawStructure(strSet.OnStructure);
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

        public void Dispose()
        {/*
            foreach (BveTypes.ClassWrappers.Structure structure in
                BaseStr.Select(x=>x.OnStructure).Concat(
                AcGStr.Select(x=>x.OnStructure)).Concat(
                AcYStr.Select(x=>x.OnStructure)).Concat(
                AcRStr.Select(x=>x.OnStructure)).Concat(
                AcAStr.Select(x=>x.OnStructure)).Concat(
                ApGStr.Select(x=>x.OnStructure)).Concat(
                ApRStr.Select(x=>x.OnStructure)).Concat(
                BcGStr.Select(x=>x.OnStructure)).Concat(
                BcYStr.Select(x=>x.OnStructure)).Concat(
                BcRStr.Select(x=>x.OnStructure)).Concat(
                BcAStr.Select(x=>x.OnStructure)).Concat(
                BpGStr.Select(x=>x.OnStructure)).Concat(
                BpRStr.Select(x=>x.OnStructure)).Concat(

                AcGStr.Select(x=>x.OffStructure)).Concat(
                AcYStr.Select(x=>x.OffStructure)).Concat(
                AcRStr.Select(x=>x.OffStructure)).Concat(
                AcAStr.Select(x=>x.OffStructure)).Concat(
                ApGStr.Select(x=>x.OffStructure)).Concat(
                ApRStr.Select(x=>x.OffStructure)).Concat(
                BcGStr.Select(x=>x.OffStructure)).Concat(
                BcYStr.Select(x=>x.OffStructure)).Concat(
                BcRStr.Select(x=>x.OffStructure)).Concat(
                BcAStr.Select(x=>x.OffStructure)).Concat(
                BpGStr.Select(x=>x.OffStructure)).Concat(
                BpRStr.Select(x=>x.OffStructure)).Concat(
                BaseStr.Select(x=>x.OffStructure))
            )
            {
                structure?.Model?.Dispose();
            }*/
        }
    }
}
