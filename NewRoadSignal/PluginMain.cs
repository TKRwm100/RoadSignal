using BveEx.Extensions.MapStatements;
using BveEx.PluginHost.Plugins;
using BveTypes.ClassWrappers;
using FastMember;
using ObjectiveHarmonyPatch;
using SlimDX;
using System;
using System.Collections.Generic;
using System.Linq;
using TypeWrapping;

namespace BveEx.Toukaitetudou.RoadSignal
{
    [Plugin(PluginType.MapPlugin)]
    public class PluginMain : AssemblyPluginBase
    {
        private readonly IStatementSet Statements;
        private readonly HarmonyPatch patch;
        List<ConfigData> ConfigDatas;
        FastMethod DrawMethod;
        public PluginMain(PluginBuilder builder) : base(builder)
        {
            Statements= Extensions.GetExtension<IStatementSet>();
            BveHacker.ScenarioCreated+=BveHacker_ScenarioCreated;
            Statements.LoadingCompleted+=Statements_LoadingCompleted;
            DrawMethod=FastMethod.Create( BveHacker.BveTypes.GetClassInfoOf<Model>().OriginalType.GetMethod("b",new Type[] { BveHacker.BveTypes.GetClassInfoOf<Direct3DProvider>().OriginalType}));

            ClassMemberSet members = BveHacker.BveTypes.GetClassInfoOf<StructureDrawer>();
            FastMethod DrawCarsMethod = members.GetSourceMethodOf(nameof(ObjectDrawer.StructureDrawer.Draw));
            patch=HarmonyPatch.Patch(Identifier, DrawCarsMethod.Source, PatchType.Prefix);
            patch.Invoked+=Patch_Invoked;
        }

        private PatchInvokationResult Patch_Invoked(object sender, PatchInvokedEventArgs e)
        {
            Direct3DProvider direct3DProvider = Direct3DProvider.FromSource(e.Args[0]);
            Matrix viewMatrix = (Matrix)e.Args[1];
            double minDrawLocation = BveHacker.Scenario.VehicleLocation.Location - BveHacker.Scenario.ObjectDrawer.DrawDistanceManager.BackDrawDistance;
            double maxDrawLocation = BveHacker.Scenario.VehicleLocation.Location + BveHacker.Scenario.ObjectDrawer.DrawDistanceManager.DrawDistance;
            direct3DProvider.Device.SetRenderState(SlimDX.Direct3D9.RenderState.ZWriteEnable, true);
            object[] args = new object[] { e.Args[0] };
            int locationBlack = BveHacker.Scenario.VehicleLocation.BlockIndex * 25;
            foreach (BveTypes.ClassWrappers.Structure structure in ConfigDatas.Where(x=>minDrawLocation<=x.Location&&x.Location<=maxDrawLocation).SelectMany(cd => cd?.GetDrawStructure()).Where(structure => !(structure is null)))
            {
                    Matrix matrix = BveHacker.Scenario.Map.GetTrackMatrix(structure, structure.Location, locationBlack) * viewMatrix;
                    direct3DProvider.Device.SetTransform(SlimDX.Direct3D9.TransformState.World,
                    matrix
                    ); 
                    DrawMethod.Invoke(structure.Model.Src,args);
                
            }
            return new PatchInvokationResult(SkipModes.Continue);
        }

        private void Statements_LoadingCompleted(object sender, EventArgs e)
        {
            IEnumerable<Statement> statements= Statements.FindUserStatements(nameof(Toukaitetudou), ClauseFilter.Element(nameof(RoadSignal), 0), ClauseFilter.Function("Put",1));

            ConfigDatas = statements.Select(x => ConfigData.CreateConfigdata(x)).ToList();
            return;
        }

        private void BveHacker_ScenarioCreated(PluginHost.ScenarioCreatedEventArgs e)
        {
        }

        public override void Dispose()
        {
            BveHacker.ScenarioCreated -= BveHacker_ScenarioCreated;
            Statements.LoadingCompleted -= Statements_LoadingCompleted;
            patch.Dispose();
            if (!(ConfigDatas is null))
            {
                foreach (ConfigData cd in ConfigDatas)
                {
                    cd?.Dispose();
                }
            }
            ModelManager.Dispose();
        }

        public override void Tick(TimeSpan elapsed)
        {
            foreach (ConfigData cd in ConfigDatas)
            {
                cd?.Tick(elapsed);
            }
        }
    }
}
