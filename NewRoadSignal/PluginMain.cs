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
        List<SignalController> Controllers;
        public PluginMain(PluginBuilder builder) : base(builder)
        {
            Statements= Extensions.GetExtension<IStatementSet>();
            Statements.LoadingCompleted+=Statements_LoadingCompleted;
            SignalController.Initialize(BveHacker,FastMethod.Create( BveHacker.BveTypes.GetClassInfoOf<Model>().OriginalType.GetMethod("b",new Type[] { BveHacker.BveTypes.GetClassInfoOf<Direct3DProvider>().OriginalType})));

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
            
            foreach (SignalController controller in Controllers)
            {
                if (minDrawLocation<=controller.Location&&controller.Location<=maxDrawLocation)
                {
                    controller.Draw(viewMatrix);
                }

                
            }
            return new PatchInvokationResult(SkipModes.Continue);
        }

        private void Statements_LoadingCompleted(object sender, EventArgs e)
        {
            IEnumerable<Statement> statements= Statements.FindUserStatements(nameof(Toukaitetudou), ClauseFilter.Element(nameof(RoadSignal), 0), ClauseFilter.Function("Put",1));

            Controllers = statements.Select(x => SignalController.CreateController(x)).ToList();
            return;
        }


        public override void Dispose()
        { 
            Statements.LoadingCompleted -= Statements_LoadingCompleted;
            patch.Dispose();
            ModelManager.Dispose();
        }

        public override void Tick(TimeSpan elapsed)
        {
            foreach (SignalController ctrl in Controllers)
            {
                ctrl?.Tick(elapsed);
            }
        }
    }
}
