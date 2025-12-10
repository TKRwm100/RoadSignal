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
        public PluginMain(PluginBuilder builder) : base(builder)
        {
            Statements= Extensions.GetExtension<IStatementSet>();
            BveHacker.ScenarioCreated+=BveHacker_ScenarioCreated;
            Statements.LoadingCompleted+=Statements_LoadingCompleted;

            ClassMemberSet members = BveHacker.BveTypes.GetClassInfoOf<StructureDrawer>();
            FastMethod DrawCarsMethod = members.GetSourceMethodOf(nameof(ObjectDrawer.StructureDrawer.Draw));
            patch=HarmonyPatch.Patch(Identifier, DrawCarsMethod.Source, PatchType.Prefix);
            patch.Invoked+=Patch_Invoked;

        }

        private PatchInvokationResult Patch_Invoked(object sender, PatchInvokedEventArgs e)
        {
            Direct3DProvider direct3DProvider = Direct3DProvider.FromSource(e.Args[0]);
            Matrix viewMatrix = (Matrix)e.Args[1];

            foreach (ConfigData cd in ConfigDatas)
            {
                if (cd is null) continue;
                foreach (BveTypes.ClassWrappers.Structure structure in cd.GetDrawStructure())
                {
                    if (structure is null) continue;
                    if (
                    BveHacker.Scenario.VehicleLocation.Location-BveHacker.Scenario.ObjectDrawer.DrawDistanceManager.BackDrawDistance<=
                    structure.Location&&
                    structure.Location<=
                    BveHacker.Scenario.VehicleLocation.Location+BveHacker.Scenario.ObjectDrawer.DrawDistanceManager.DrawDistance
                    )
                    {
                        Matrix matrix = BveHacker.Scenario.Map.GetTrackMatrix(structure, structure.Location, BveHacker.Scenario.VehicleLocation.BlockIndex*25)*viewMatrix;


                        direct3DProvider.Device.SetTransform(SlimDX.Direct3D9.TransformState.World,
                        matrix
                        );
                        structure.Model.Draw(direct3DProvider, false);
                        structure.Model.Draw(direct3DProvider, true);
                    }
                }
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
