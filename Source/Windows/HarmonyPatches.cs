using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using Verse;
using RimWorld;
using Verse.AI;
using UnityEngine;

namespace Swimming
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {

            HarmonyInstance harmint = HarmonyInstance.Create("Lizaomng.Swimming");

            //harmint.Patch == 
            //->Harmony Patch -- Changes RimWorld's very soul
            //AccessTools.Method(typeof(Verse.AI.Pawn_PathFollower), "IsNextCellWalkable") ==
            //->Goes to Pawm_PathFollower class and looks at the method called IsNextCellWalkable
            //null
            //-> Pre-patch zone. Set to not patch before the code starts //  NULL
            //new HarmonyMethod(AccessTools.Method(typeof(Class1), "SwimmingCheck"))
            //-> Makes a new end of method patch from Class1 called SwimmingCheck
            harmint.Patch(AccessTools.Method(typeof(Verse.AI.Pawn_PathFollower), "IsNextCellWalkable"), null,
               new HarmonyMethod(AccessTools.Method(typeof(HarmonyPatches), nameof (SwimmingCheck))));

            //(Vector3 rootLoc, Quaternion quat, bool renderBody, Rot4 bodyFacing,
            //Rot4 headFacing, RotDrawMode bodyDrawType, bool portrait, bool headStump)
            harmint.Patch(AccessTools.Method(typeof(PawnRenderer), "RenderPawnInternal",
                new Type[] {typeof(Vector3), typeof(Quaternion), typeof(bool), typeof(Rot4), typeof(Rot4),
                typeof(RotDrawMode),typeof(bool), typeof(bool)}),
               new HarmonyMethod(AccessTools.Method(typeof(HarmonyPatches), "CutoffBody")), null);




            harmint.Patch(AccessTools.Method(typeof(ForbidUtility), "IsForbidden",
               new Type[] {typeof(IntVec3), typeof(Pawn)}),
              new HarmonyMethod(AccessTools.Method(typeof(HarmonyPatches), "Forbitten")), null);
            harmint.Patch(AccessTools.Method(typeof(FloatMenuMakerMap), "AddHumanlikeOrders"),
             new HarmonyMethod(AccessTools.Method(typeof(HarmonyPatches), "AddHumanlikeOrders")), null);
            harmint.Patch(AccessTools.Method(typeof(Graphic_Shadow), "DrawWorker"),
            new HarmonyMethod(AccessTools.Method(typeof(HarmonyPatches), "LZG_SomethingSomething")), null);
        }

        public static bool LZG_SomethingSomething(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing)
        {
            Pawn Daniel = thing as Pawn;
            /////////////////////// Log.Message("Matthew teacher is wearing glassessssssssssssssss");
            if (Daniel != null && Daniel.PositionHeld is IntVec3 vec && vec.IsValid &&
                vec.GetTerrain(Daniel.MapHeld) is TerrainDef def &&
              (def == TerrainDefOf.WaterDeep ||
                def == TerrainDefOf.WaterMovingDeep ||

                def == TerrainDefOf.WaterOceanDeep)

                )

            {
                return false;
            }
            return true;
                
        }





        // RimWorld.FloatMenuMakerMap
        private static void AddHumanlikeOrders(Vector3 clickPos, Pawn pawn, List<FloatMenuOption> opts)
        {
            IntVec3 c = IntVec3.FromVector3(clickPos);

            if (c is IntVec3 vec && vec.IsValid &&
                vec.GetTerrain(pawn.MapHeld) is TerrainDef def &&
              (def == TerrainDefOf.WaterDeep ||
                def == TerrainDefOf.WaterMovingDeep ||
                def == TerrainDefOf.WaterMovingShallow ||
                def == TerrainDefOf.WaterOceanDeep ||
                def == TerrainDefOf.WaterOceanShallow ||
                def == TerrainDefOf.WaterShallow))
            {
                FloatMenuOption item5;
                string BOX = "Find Treasure";
                if (!pawn.CanReach(c, PathEndMode.OnCell, Danger.Deadly, false, TraverseMode.ByPawn))
                {
                    item5 = new FloatMenuOption(" (" + "NoPath".Translate() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null);
                }
                else
                {
                    MenuOptionPriority priority = MenuOptionPriority.Low;
                    item5 = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(BOX, delegate
                    {

                        Job job = new Job(DefDatabase<JobDef>.GetNamed("LZG_FindTreasure"), c);
                        job.count = 1;
                        pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                    }, priority, null, null, 0f, null, null), pawn, c, "ReservedBy");
                }
                opts.Add(item5);
                
                
            }
        }

        public static void Forbitten(IntVec3 c, Pawn pawn,ref bool __result)
        {
        TerrainDef BoX = c.GetTerrain(pawn.MapHeld);
            if (BoX == TerrainDefOf.WaterOceanDeep ||
                BoX == TerrainDefOf.WaterDeep ||
               BoX == TerrainDefOf.WaterMovingDeep ||
                BoX == TerrainDefOf.WaterMovingShallow ||
                   BoX == TerrainDefOf.WaterOceanShallow ||
                   BoX == TerrainDefOf.WaterShallow)
            {
                if(pawn.RaceProps.Animal)
                __result = false;

            }


         //   retun ForbidUtility.CaresAboutForbidden(pawn, true) && (!c.InAllowedArea(pawn) ||
// (pawn.mindState.maxDistToSquadFlag > 0f && !c.InHorDistOf(pawn.DutyLocation(), pawn.mindState.maxDistToSquadFlag)));
        }


        public static void SwimmingCheck(Pawn_PathFollower __instance, ref bool __result)
        {
            Pawn Daniel = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();

            TerrainDef BoX = __instance.nextCell.GetTerrain(Find.VisibleMap);

            if (Daniel != null && (BoX.IsDeep() || BoX.IsShallaws()))
            {


                if (Daniel?.RaceProps?.Animal ?? false)
                {
                    __result = false;
                }
                else
                {
                    if (Daniel?.skills?.GetSkill(DefDatabase<SkillDef>.GetNamed("Swimming")) is SkillRecord swimSkill)
                    {
                        swimSkill.Learn(5f);
                    }
                }
            }

        }

        private static void DrowningCheck(Pawn Daniel, TerrainDef BoX)
        {
            if (BoX.IsDeep())
            {

                int? level = Daniel?.skills?.GetSkill(DefDatabase<SkillDef>.GetNamed("Swimming"))?.Level;
                if (level != null)
                {
                    float math = (20 - level.Value) * 0.00001f;
                  if (Daniel.inventory.GetDirectlyHeldThings().FirstOrDefault(x => x.TryGetComp<OxygenTank>() !=null )is ThingWithComps myTank)
                    {
                        var nope = myTank.TryGetComp<OxygenTank>();
                        if (nope != null)
                        {
                            if (nope.Air > 0)
                            {
                                nope.Air--;
                                return;

                           
                            }
                        }
                    }
                    HealthUtility.AdjustSeverity(Daniel, HediffDef.Named("LZG_Drowning"), math);


                }
            }
            else
            {
                HealthUtility.AdjustSeverity(Daniel, HediffDef.Named("LZG_Drowning"), -0.001f);
            }
        }

        public static bool IsShallaws(this TerrainDef BoX)
        {
            if (
       BoX == TerrainDefOf.WaterMovingShallow ||
       BoX == TerrainDefOf.WaterOceanShallow ||
       BoX == TerrainDefOf.WaterShallow)
            {
                return true;
            }
                return false;
        }
        public static bool IsDeep(this TerrainDef BoX)
        {
            if (
       BoX == TerrainDefOf.WaterMovingDeep ||
       BoX == TerrainDefOf.WaterOceanDeep ||
       BoX == TerrainDefOf.WaterDeep)
            {
                return true;
            }
            return false;

        }
        // Verse.PawnRenderer
        public static bool CutoffBody(PawnRenderer __instance,Vector3 rootLoc, Quaternion quat, bool renderBody, Rot4 bodyFacing, Rot4 headFacing, RotDrawMode bodyDrawType, bool portrait, bool headStump)
        {
           //Log.Message("1");
            Pawn Daniel = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            //Log.Message("2");
            /////////////////////// Log.Message("Matthew teacher is wearing glassessssssssssssssss");
            if (Daniel != null && Daniel.PositionHeld is IntVec3 vec && vec.IsValid &&
                vec.GetTerrain(Daniel.MapHeld) is TerrainDef def)
            {
                //Log.Message("3");
                try
                {
                    if (!Find.TickManager.Paused)
                    {
                        //Log.Message("4");
                        DrowningCheck(Daniel, def); //j,hvkftyjdyfyfytetjyuhfdgtgvhjmb,nkjutvhrtrgtbhnmb,gu,j6yhdtgexwssr 5nhyj6ujhm,k.u,jhbyrhd4t3cy 7m6ki,l;kiu7yr5f43243rghy67ujklu;k'loi;i9hynt7g6rfvdcr4v3fr4tyu6i7kl8;9[p;kujhygtrfew2e45efr6ghbjnmkl,;.lkjnhgvf6cxd45fc6gv7bh8njukm0il,k0mjnhy8gvfr6gyvthyjuk0iujyhtgrvfcet5rgthyjukil,kujyhtgrvfecdwefrgyujnilmky애국가1.동해물과 백두산이 마르고 닳도록 하느님이 보우하 우리나라 만세 무궁화 삼천리 화려강산 대한사람 대한으로 길이 보전하세.
                        //Log.Message("5");
                    }

                    if (def == TerrainDefOf.WaterDeep ||
                         def == TerrainDefOf.WaterMovingDeep ||
                         def == TerrainDefOf.WaterOceanDeep)
                    {

                        //Log.Message("6");

                        //if(Find.TickManager.TicksGame %60==0&& Find.Worl.)
                        //MoteMaker.MakeWaterSplash(vec.ToVector3(), Daniel.MapHeld, Mathf.Sqrt(Daniel.BodySize) * 2f, 1.5f);

                        // Log.Message("BlaBlablablablabalblablablalbal");
                        renderBody = false;
                        if (!__instance.graphics.AllResolved)
                        {
                            __instance.graphics.ResolveAllGraphics();
                            //Log.Message("7");
                        }
                        Mesh mesh = null;
                        if (renderBody)
                        {
                            //Log.Message("8");
                            Vector3 loc = rootLoc;
                            loc.y += 0.0046875f;
                            if (bodyDrawType == RotDrawMode.Dessicated && !Daniel.RaceProps.Humanlike && __instance.graphics.dessicatedGraphic != null && !portrait)
                            {
                                __instance.graphics.dessicatedGraphic.Draw(loc, bodyFacing, Daniel);
                                //Log.Message("9");
                            }
                            else
                            {
                                if (Daniel.RaceProps.Humanlike)
                                {
                                    mesh = MeshPool.humanlikeBodySet.MeshAt(bodyFacing);
                                    //Log.Message("10");
                                }
                                else
                                {
                                    mesh = __instance.graphics.nakedGraphic.MeshAt(bodyFacing);
                                }
                                List<Material> list = __instance.graphics.MatsBodyBaseAt(bodyFacing, bodyDrawType);
                                for (int i = 0; i < list.Count; i++)
                                {
                                    Material damagedMat = __instance.graphics.flasher.GetDamagedMat(list[i]);
                                    GenDraw.DrawMeshNowOrLater(mesh, loc, quat, damagedMat, portrait);
                                    loc.y += 0.0046875f;
                                }
                                if (bodyDrawType == RotDrawMode.Fresh)
                                {
                                    Vector3 drawLoc = rootLoc;
                                    drawLoc.y += 0.01875f;
                                    PawnWoundDrawer Matthew = Traverse.Create(__instance).Field("woundOverlays").GetValue<PawnWoundDrawer>();
                                    Matthew.RenderOverBody(drawLoc, mesh, quat, portrait);

                                }
                            }
                        }
                        Vector3 vector = rootLoc;
                        Vector3 a = rootLoc;
                        if (bodyFacing != Rot4.North)
                        {
                            a.y += 0.0281250011f;
                            vector.y += 0.0234375f;
                        }
                        else
                        {
                            a.y += 0.0234375f;
                            vector.y += 0.0281250011f;
                        }
                        if (__instance.graphics.headGraphic != null)
                        {
                            Vector3 b = quat * __instance.BaseHeadOffsetAt(headFacing);
                            Material material = __instance.graphics.HeadMatAt(headFacing, bodyDrawType, headStump);
                            if (material != null)
                            {
                                Mesh mesh2 = MeshPool.humanlikeHeadSet.MeshAt(headFacing);
                                GenDraw.DrawMeshNowOrLater(mesh2, a + b, quat, material, portrait);
                            }
                            Vector3 loc2 = rootLoc + b;
                            loc2.y += 0.0328125022f;
                            bool flag = false;
                            if (!portrait || !Prefs.HatsOnlyOnMap)
                            {
                                Mesh mesh3 = __instance.graphics.HairMeshSet.MeshAt(headFacing);
                                List<ApparelGraphicRecord> apparelGraphics = __instance.graphics.apparelGraphics;
                                for (int j = 0; j < apparelGraphics.Count; j++)
                                {
                                    if (apparelGraphics[j].sourceApparel.def.apparel.LastLayer == ApparelLayer.Overhead)
                                    {
                                        flag = true;
                                        Material material2 = apparelGraphics[j].graphic.MatAt(bodyFacing, null);
                                        material2 = __instance.graphics.flasher.GetDamagedMat(material2);
                                        GenDraw.DrawMeshNowOrLater(mesh3, loc2, quat, material2, portrait);
                                    }
                                }
                            }
                            if (!flag && bodyDrawType != RotDrawMode.Dessicated && !headStump)
                            {
                                Mesh mesh4 = __instance.graphics.HairMeshSet.MeshAt(headFacing);
                                Material mat = __instance.graphics.HairMatAt(headFacing);
                                GenDraw.DrawMeshNowOrLater(mesh4, loc2, quat, mat, portrait);
                            }
                        }
                        if (renderBody)
                        {
                            //for (int k = 0; k < __instance.graphics.apparelGraphics.Count; k++)
                            //{
                            //    ApparelGraphicRecord apparelGraphicRecord = __instance.graphics.apparelGraphics[k];
                            //    if (apparelGraphicRecord.sourceApparel.def.apparel.LastLayer == ApparelLayer.Shell)
                            //    {
                            //        Material material3 = apparelGraphicRecord.graphic.MatAt(bodyFacing, null);
                            //        material3 = __instance.graphics.flasher.GetDamagedMat(material3);
                            //        GenDraw.DrawMeshNowOrLater(mesh, vector, quat, material3, portrait);
                            //    }
                            //}
                        }
                        if (!portrait && Daniel.RaceProps.Animal && Daniel.inventory != null && Daniel.inventory.innerContainer.Count > 0)
                        {
                            Graphics.DrawMesh(mesh, vector, quat, __instance.graphics.packGraphic.MatAt(Daniel.Rotation, null), 0);
                        }
                        if (!portrait)
                        {
                            AccessTools.Method(typeof(PawnRenderer), "DrawEquipment").Invoke(__instance, new object[] { rootLoc });
                            //__instance.DrawEquipment(rootLoc);
                            if (Daniel.apparel != null)
                            {
                                List<Apparel> wornApparel = Daniel.apparel.WornApparel;
                                for (int l = 0; l < wornApparel.Count; l++)
                                {
                                    wornApparel[l].DrawWornExtras();
                                }
                            }
                            Vector3 bodyLoc = rootLoc;
                            bodyLoc.y += 0.0421875f;
                            PawnHeadOverlays Tweety = Traverse.Create(__instance).Field("statusOverlays").GetValue<PawnHeadOverlays>();

                            Tweety.RenderStatusOverlays(bodyLoc, quat, MeshPool.humanlikeHeadSet.MeshAt(headFacing));
                        }
                        return false;
                    } if (!Find.TickManager.Paused)
                {
                    DrowningCheck(Daniel, def); //j,hvkftyjdyfyfytetjyuhfdgtgvhjmb,nkjutvhrtrgtbhnmb,gu,j6yhdtgexwssr 5nhyj6ujhm,k.u,jhbyrhd4t3cy 7m6ki,l;kiu7yr5f43243rghy67ujklu;k'loi;i9hynt7g6rfvdcr4v3fr4tyu6i7kl8;9[p;kujhygtrfew2e45efr6ghbjnmkl,;.lkjnhgvf6cxd45fc6gv7bh8njukm0il,k0mjnhy8gvfr6gyvthyjuk0iujyhtgrvfcet5rgthyjukil,kujyhtgrvfecdwefrgyujnilmky애국가1.동해물과 백두산이 마르고 닳도록 하느님이 보우하 우리나라 만세 무궁화 삼천리 화려강산 대한사람 대한으로 길이 보전하세.

                }

                if (def == TerrainDefOf.WaterDeep ||
                     def == TerrainDefOf.WaterMovingDeep ||
                     def == TerrainDefOf.WaterOceanDeep) 
                 {
                    


                    //if(Find.TickManager.TicksGame %60==0&& Find.Worl.)
                    //MoteMaker.MakeWaterSplash(vec.ToVector3(), Daniel.MapHeld, Mathf.Sqrt(Daniel.BodySize) * 2f, 1.5f);

                    // Log.Message("BlaBlablablablabalblablablalbal");
                    renderBody = false;
                    if (!__instance.graphics.AllResolved)
                    {
                        __instance.graphics.ResolveAllGraphics();
                    }
                    Mesh mesh = null;
                    if (renderBody)
                    {
                        Vector3 loc = rootLoc;
                        loc.y += 0.0046875f;
                        if (bodyDrawType == RotDrawMode.Dessicated && !Daniel.RaceProps.Humanlike && __instance.graphics.dessicatedGraphic != null && !portrait)
                        {
                            __instance.graphics.dessicatedGraphic.Draw(loc, bodyFacing, Daniel);
                        }
                        else
                        {
                            if (Daniel.RaceProps.Humanlike)
                            {
                                mesh = MeshPool.humanlikeBodySet.MeshAt(bodyFacing);
                            }
                            else
                            {
                                mesh = __instance.graphics.nakedGraphic.MeshAt(bodyFacing);
                            }
                            List<Material> list = __instance.graphics.MatsBodyBaseAt(bodyFacing, bodyDrawType);
                            for (int i = 0; i < list.Count; i++)
                            {
                                Material damagedMat = __instance.graphics.flasher.GetDamagedMat(list[i]);
                                GenDraw.DrawMeshNowOrLater(mesh, loc, quat, damagedMat, portrait);
                                loc.y += 0.0046875f;
                            }
                            if (bodyDrawType == RotDrawMode.Fresh)
                            {
                                Vector3 drawLoc = rootLoc;
                                drawLoc.y += 0.01875f;
                                PawnWoundDrawer Matthew = Traverse.Create(__instance).Field("woundOverlays").GetValue<PawnWoundDrawer>();
                                Matthew.RenderOverBody(drawLoc, mesh, quat, portrait);

                            }
                        }
                    }
                    Vector3 vector = rootLoc;
                    Vector3 a = rootLoc;
                    if (bodyFacing != Rot4.North)
                    {
                        a.y += 0.0281250011f;
                        vector.y += 0.0234375f;
                    }
                    else
                    {
                        a.y += 0.0234375f;
                        vector.y += 0.0281250011f;
                    }
                    if (__instance.graphics.headGraphic != null)
                    {
                        Vector3 b = quat * __instance.BaseHeadOffsetAt(headFacing);
                        Material material = __instance.graphics.HeadMatAt(headFacing, bodyDrawType, headStump);
                        if (material != null)
                        {
                            Mesh mesh2 = MeshPool.humanlikeHeadSet.MeshAt(headFacing);
                            GenDraw.DrawMeshNowOrLater(mesh2, a + b, quat, material, portrait);
                        }
                        Vector3 loc2 = rootLoc + b;
                        loc2.y += 0.0328125022f;
                        bool flag = false;
                        if (!portrait || !Prefs.HatsOnlyOnMap)
                        {
                            Mesh mesh3 = __instance.graphics.HairMeshSet.MeshAt(headFacing);
                            List<ApparelGraphicRecord> apparelGraphics = __instance.graphics.apparelGraphics;
                            for (int j = 0; j < apparelGraphics.Count; j++)
                            {
                                if (apparelGraphics[j].sourceApparel.def.apparel.LastLayer == ApparelLayer.Overhead)
                                {
                                    flag = true;
                                    Material material2 = apparelGraphics[j].graphic.MatAt(bodyFacing, null);
                                    material2 = __instance.graphics.flasher.GetDamagedMat(material2);
                                    GenDraw.DrawMeshNowOrLater(mesh3, loc2, quat, material2, portrait);
                                }
                            }
                        }
                        if (!flag && bodyDrawType != RotDrawMode.Dessicated && !headStump)
                        {
                            Mesh mesh4 = __instance.graphics.HairMeshSet.MeshAt(headFacing);
                            Material mat = __instance.graphics.HairMatAt(headFacing);
                            GenDraw.DrawMeshNowOrLater(mesh4, loc2, quat, mat, portrait);
                        }
                    }
                    if (renderBody)
                    {
                        //for (int k = 0; k < __instance.graphics.apparelGraphics.Count; k++)
                        //{
                        //    ApparelGraphicRecord apparelGraphicRecord = __instance.graphics.apparelGraphics[k];
                        //    if (apparelGraphicRecord.sourceApparel.def.apparel.LastLayer == ApparelLayer.Shell)
                        //    {
                        //        Material material3 = apparelGraphicRecord.graphic.MatAt(bodyFacing, null);
                        //        material3 = __instance.graphics.flasher.GetDamagedMat(material3);
                        //        GenDraw.DrawMeshNowOrLater(mesh, vector, quat, material3, portrait);
                        //    }
                        //}
                    }
                    if (!portrait && Daniel.RaceProps.Animal && Daniel.inventory != null && Daniel.inventory.innerContainer.Count > 0)
                    {
                        Graphics.DrawMesh(mesh, vector, quat, __instance.graphics.packGraphic.MatAt(Daniel.Rotation, null), 0);
                    }
                    if (!portrait)
                    {
                        AccessTools.Method(typeof(PawnRenderer), "DrawEquipment").Invoke(__instance, new object[] { rootLoc });
                        //__instance.DrawEquipment(rootLoc);
                        if (Daniel.apparel != null)
                        {
                            List<Apparel> wornApparel = Daniel.apparel.WornApparel;
                            for (int l = 0; l < wornApparel.Count; l++)
                            {
                                wornApparel[l].DrawWornExtras();
                            }
                        }
                        Vector3 bodyLoc = rootLoc;
                        bodyLoc.y += 0.0421875f;
                        PawnHeadOverlays Tweety = Traverse.Create(__instance).Field("statusOverlays").GetValue<PawnHeadOverlays>();

                        Tweety.RenderStatusOverlays(bodyLoc, quat, MeshPool.humanlikeHeadSet.MeshAt(headFacing));
                    }
                    return false;
                }
                }
                catch
                {
                    
                    
                }

               
            }

            return true;
         
        }


    }


}