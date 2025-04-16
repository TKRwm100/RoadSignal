using BveEx.Extensions.MapStatements;
using BveTypes.ClassWrappers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace BveEx.Toukaitetudou.RoadSignal
{
    internal class ConfigData : IDisposable
    {
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


        public static ConfigData CreateConfigdata(Statement statement)
        {
            ConfigData rt;
            Config config;
            double location = statement.Source.Location;
            string filePath = statement.Source.FileName;
            using (StreamReader sr = new StreamReader(Path.Combine(Path.GetDirectoryName(statement.Source.FileName), statement.Source.Clauses[4].Args[0] as string)))
            {
                XmlSerializer xs = new XmlSerializer(typeof(Config));
                try
                {
                    config = (Config)xs.Deserialize(sr);
                    rt = new ConfigData(config,filePath);
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

            rt.BaseStr = structure.Where(x=>x.x==ItemsChoiceType.Base).Select(x => x.y.GetStructure(location, filePath)).ToList();
            rt.AcGStr = structure.Where(x=>x.x==ItemsChoiceType.AcG).Select(x => x.y.GetStructure(location, filePath)).ToList();
            rt.AcYStr = structure.Where(x=>x.x==ItemsChoiceType.AcY).Select(x => x.y.GetStructure(location, filePath)).ToList();
            rt.AcRStr = structure.Where(x=>x.x==ItemsChoiceType.AcR).Select(x => x.y.GetStructure(location, filePath)).ToList();
            rt.AcAStr = structure.Where(x=>x.x==ItemsChoiceType.AcA).Select(x => x.y.GetStructure(location, filePath)).ToList();
            rt.ApGStr = structure.Where(x=>x.x==ItemsChoiceType.ApG).Select(x => x.y.GetStructure(location, filePath)).ToList();
            rt.ApRStr = structure.Where(x=>x.x==ItemsChoiceType.ApR).Select(x => x.y.GetStructure(location, filePath)).ToList();

            rt.BcGStr = structure.Where(x=>x.x==ItemsChoiceType.BcG).Select(x => x.y.GetStructure(location, filePath)).ToList();
            rt.BcYStr = structure.Where(x=>x.x==ItemsChoiceType.BcY).Select(x => x.y.GetStructure(location, filePath)).ToList();
            rt.BcRStr = structure.Where(x=>x.x==ItemsChoiceType.BcR).Select(x => x.y.GetStructure(location, filePath)).ToList();
            rt.BcAStr = structure.Where(x=>x.x==ItemsChoiceType.BcA).Select(x => x.y.GetStructure(location, filePath)).ToList();
            rt.BpGStr = structure.Where(x=>x.x==ItemsChoiceType.BpG).Select(x => x.y.GetStructure(location, filePath)).ToList();
            rt.BpRStr = structure.Where(x=>x.x==ItemsChoiceType.BpR).Select(x => x.y.GetStructure(location, filePath)).ToList();

            return rt;
        }

        TimeSpan cycleSpan;
        public string FilePath { get; }
        private ConfigData(Config config,string path)
        {
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

        public IEnumerable<BveTypes.ClassWrappers.Structure> GetDrawStructure()
        {
            return new List<IEnumerable<BveTypes.ClassWrappers.Structure>>()
            {
                BaseStr.Select(x=>x.OnStructure),

                isEnable&&(AcGb<totalElapse&&totalElapse<AcYb)                                                                                                              ?AcGStr.Select(x=>x.OnStructure):   AcGStr.Select(x=>x.OffStructure),
                isEnable &&((AcYb<totalElapse&&totalElapse<AcYf) ||(AcAf<totalElapse&&totalElapse<AcAYf))                                                                   ?AcYStr.Select(x=>x.OnStructure):   AcYStr.Select(x=>x.OffStructure),
                !isEnable || (AcYf<totalElapse&&totalElapse<AcAf)|| (AcAYf<totalElapse||totalElapse<AcGb)                                                                   ?AcRStr.Select(x=>x.OnStructure):   AcRStr.Select(x=>x.OffStructure),
                isEnable && AR&& (AcYf<totalElapse&&totalElapse<AcAf)                                                                                                       ?AcAStr.Select(x=>x.OnStructure):   AcAStr.Select(x=>x.OffStructure),
                isEnable &&((AcGb<totalElapse&&totalElapse<ApGf) ||(ApGf<totalElapse&&totalElapse<ApFf&&totalElapse.TotalSeconds%ApF.TotalSeconds<ApF.TotalSeconds/2))      ?ApGStr.Select(x=>x.OnStructure):   ApGStr.Select(x=>x.OffStructure),
                !isEnable ||(ApFf<totalElapse||totalElapse<AcGb)                                                                                                            ?ApRStr.Select(x=>x.OnStructure):   ApRStr.Select(x=>x.OffStructure),
                isEnable &&(BcGb<totalElapse&&totalElapse<BcYb)                                                                                                             ?BcGStr.Select(x=>x.OnStructure):   BcGStr.Select(x=>x.OffStructure),
                isEnable && ((BcYb<totalElapse&&totalElapse<BcYf) ||(BcAf<totalElapse&&totalElapse<BcAYf))                                                                  ?BcYStr.Select(x=>x.OnStructure):   BcYStr.Select(x=>x.OffStructure),
                !isEnable || ((BcYf<totalElapse&&totalElapse<BcAf) ||(BcAYf<totalElapse||totalElapse<BcGb))                                                                 ?BcRStr.Select(x=>x.OnStructure):   BcRStr.Select(x=>x.OffStructure),
                isEnable &&BR&& (BcYf<totalElapse&&totalElapse<BcAf)                                                                                                        ?BcAStr.Select(x=>x.OnStructure):   BcAStr.Select(x=>x.OffStructure),
                isEnable &&((BcGb<totalElapse&&totalElapse<BpGf)||(BpGf<totalElapse&&totalElapse<BpFf&&totalElapse.TotalSeconds%BpF.TotalSeconds<BpF.TotalSeconds/2))       ?BpGStr.Select(x=>x.OnStructure):   BpGStr.Select(x=>x.OffStructure),
                !isEnable ||(BpFf<totalElapse||totalElapse<BcGb)                                                                                                            ?BpRStr.Select(x=>x.OnStructure):   BpRStr.Select(x=>x.OffStructure),
            }.SelectMany(x => x);
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
        {
            foreach (BveTypes.ClassWrappers.Structure structure in
            new List<IEnumerable<BveTypes.ClassWrappers.Structure>>()
            {
                BaseStr.Select(x=>x.OnStructure),
                AcGStr.Select(x=>x.OnStructure),
                AcYStr.Select(x=>x.OnStructure),
                AcRStr.Select(x=>x.OnStructure),
                AcAStr.Select(x=>x.OnStructure),
                ApGStr.Select(x=>x.OnStructure),
                ApRStr.Select(x=>x.OnStructure),
                BcGStr.Select(x=>x.OnStructure),
                BcYStr.Select(x=>x.OnStructure),
                BcRStr.Select(x=>x.OnStructure),
                BcAStr.Select(x=>x.OnStructure),
                BpGStr.Select(x=>x.OnStructure),
                BpRStr.Select(x=>x.OnStructure),

                AcGStr.Select(x=>x.OffStructure),
                AcYStr.Select(x=>x.OffStructure),
                AcRStr.Select(x=>x.OffStructure),
                AcAStr.Select(x=>x.OffStructure),
                ApGStr.Select(x=>x.OffStructure),
                ApRStr.Select(x=>x.OffStructure),
                BcGStr.Select(x=>x.OffStructure),
                BcYStr.Select(x=>x.OffStructure),
                BcRStr.Select(x=>x.OffStructure),
                BcAStr.Select(x=>x.OffStructure),
                BpGStr.Select(x=>x.OffStructure),
                BpRStr.Select(x=>x.OffStructure),
                BaseStr.Select(x=>x.OffStructure),
            }.SelectMany(x => x))
            {
                structure?.Model?.Dispose();
            }
        }
    }
}
