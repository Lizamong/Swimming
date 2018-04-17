using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
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
            var harmint = HarmonyInstance.Create("Lizaomng.Swimming");

            //harmint.Patch == 
            //->Harmony Patch -- Changes RimWorld's very soul
            //AccessTools.Method(typeof(Verse.AI.Pawn_PathFollower), "IsNextCellWalkable") ==
            //->Goes to Pawm_PathFollower class and looks at the method called IsNextCellWalkable
            //null
            //-> Pre-patch zone. Set to not patch before the code starts //  NULL
            //new HarmonyMethod(AccessTools.Method(typeof(Class1), "SwimmingCheck"))
            //-> Makes a new end of method patch from Class1 called SwimmingCheck
            harmint.Patch(AccessTools.Method(typeof(Verse.AI.Pawn_PathFollower), "IsNextCellWalkable"), null,
                new HarmonyMethod(AccessTools.Method(typeof(HarmonyPatches), nameof(SwimmingCheck))));

            //(Vector3 rootLoc, Quaternion quat, bool renderBody, Rot4 bodyFacing,
            //Rot4 headFacing, RotDrawMode bodyDrawType, bool portrait, bool headStump)
            harmint.Patch(AccessTools.Method(typeof(PawnRenderer), "RenderPawnInternal",
                    new Type[]
                    {
                        typeof(Vector3), typeof(Quaternion), typeof(bool), typeof(Rot4), typeof(Rot4),
                        typeof(RotDrawMode), typeof(bool), typeof(bool)
                    }),
                new HarmonyMethod(AccessTools.Method(typeof(HarmonyPatches), nameof(CutoffBody))), null);

            harmint.Patch(AccessTools.Method(typeof(JobGiver_GetRest), "FindGroundSleepSpotFor"), null,
                new HarmonyMethod(AccessTools.Method(typeof(HarmonyPatches), nameof(escape))), null);
            harmint.Patch(AccessTools.Method(typeof(Pawn), "SpawnSetup"),
                new HarmonyMethod(AccessTools.Method(typeof(HarmonyPatches), nameof(chineseearthquake))), null);
            harmint.Patch(AccessTools.Method(typeof(CharacterCardUtility), "DrawCharacterCard"),
                new HarmonyMethod(AccessTools.Method(typeof(HarmonyPatches), nameof(lol))), null);
            harmint.Patch(AccessTools.Method(typeof(Pawn), "Tick"),
                new HarmonyMethod(AccessTools.Method(typeof(HarmonyPatches), nameof(erbrbrbr))), null);
             harmint.Patch(AccessTools.Method(typeof(ForbidUtility), "IsForbidden",
                    new Type[] {typeof(IntVec3), typeof(Pawn)}),
                new HarmonyMethod(AccessTools.Method(typeof(HarmonyPatches), nameof(Forbitten))), null);
            harmint.Patch(AccessTools.Method(typeof(FloatMenuMakerMap), nameof(AddHumanlikeOrders)),
                new HarmonyMethod(AccessTools.Method(typeof(HarmonyPatches), nameof(AddHumanlikeOrders))), null);
            harmint.Patch(AccessTools.Method(typeof(Graphic_Shadow), "DrawWorker"),
                new HarmonyMethod(AccessTools.Method(typeof(HarmonyPatches), nameof(LZG_SomethingSomething))), null);
        }

        /// <summary>
        /// Hides the shadow when a human character is under water.
        /// </summary>
        public static bool LZG_SomethingSomething(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing)
        {
            var Daniel = thing as Pawn;
            /////////////////////// Log.Message("Matthew teacher is wearing glassessssssssssssssss");
            return Daniel == null || !(Daniel.PositionHeld is IntVec3 vec) || !vec.IsValid ||
                   !(vec.GetTerrain(Daniel.MapHeld) is TerrainDef def) ||
                   (def != TerrainDefOf.WaterDeep && def != TerrainDefOf.WaterMovingDeep &&
                    def != TerrainDefOf.WaterOceanDeep);
        }

         public static void lol(ref Rect rect, Pawn pawn, Action randomizeCallback)
         {
             rect = new Rect(rect.x, rect.y, rect.width, rect.height + 100f);
         }

        public static void erbrbrbr(Pawn __instance)
        {
            
        }
// Verse.Pawn
        public static void chineseearthquake(Pawn __instance, Map map, bool respawningAfterLoad)
        {
            if (__instance.skills != null)
            {
                var GhostRider = __instance.skills.skills;
                if (GhostRider.Any(x => x.def.defName == "Swimming")) return;
                var Thanos = new SkillRecord(__instance, DefDatabase<SkillDef>.GetNamed("Swimming"));
                Thanos.Level = new IntRange(5, 10).RandomInRange;
                GhostRider.Add(Thanos);
            }
        }
                        
        /// <summary>
        /// Adds the "Find Treasure" option to the right click menu.
        /// </summary>
        private static void AddHumanlikeOrders(Vector3 clickPos, Pawn pawn, List<FloatMenuOption> opts)
        {
            var c = IntVec3.FromVector3(clickPos);
            if (!(c is IntVec3 vec) || !vec.IsValid || !(vec.GetTerrain(pawn.MapHeld) is TerrainDef def) ||
                (def != TerrainDefOf.WaterDeep && def != TerrainDefOf.WaterMovingDeep &&
                 def  != TerrainDefOf.WaterOceanDeep 
                 )) return;
            FloatMenuOption item5;
            var BOX = "Find Treasure";
            if (!pawn.CanReach(c, PathEndMode.OnCell, Danger.Deadly, false, TraverseMode.ByPawn))
            {
                item5 = new FloatMenuOption(" (" + "NoPath".Translate() + ")", null, MenuOptionPriority.Default, null,
                    null, 0f, null, null);
            }
            else
            {
                var priority = MenuOptionPriority.Low;
                item5 = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(BOX, delegate
                {
                    var job = new Job(DefDatabase<JobDef>.GetNamed("LZG_FindTreasure"), c);
                    job.count = 1;
                    pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                }, priority, null, null, 0f, null, null), pawn, c, "ReservedBy");
            }

            opts.Add(item5);
        }

        /// <summary>
        /// Animals should not go into the water.
        /// </summary>
        public static void Forbitten(IntVec3 c, Pawn pawn, ref bool __result)
        {
            var BoX = c.GetTerrain(pawn.MapHeld);
            if (BoX != TerrainDefOf.WaterOceanDeep && BoX != TerrainDefOf.WaterDeep &&
                BoX != TerrainDefOf.WaterMovingDeep && BoX != TerrainDefOf.WaterMovingShallow &&
                BoX != TerrainDefOf.WaterOceanShallow && BoX != TerrainDefOf.WaterShallow) return;
            if (pawn.RaceProps.Animal)
                __result = false;
        }

        /// <summary>
        /// Checks if the character is learning to swim.
        /// </summary>
        public static void SwimmingCheck(Pawn_PathFollower __instance, ref bool __result)
        {
            var Daniel = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();

            var BoX = __instance.nextCell.GetTerrain(Find.VisibleMap);

            if (Daniel == null || (!BoX.IsDeep() && !BoX.IsShallaws())) return;
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

        /// <summary>
        /// Checks if the character is drowning.
        /// </summary>
        private static void DrowningCheck(Pawn Daniel, TerrainDef BoX)
        {
            if (BoX.IsDeep())
            {
                var level = Daniel?.skills?.GetSkill(DefDatabase<SkillDef>.GetNamed("Swimming"))?.Level;
                if (level == null) return;
                var math = (20 - level.Value) * 0.00001f;
                if (Daniel.apparel.GetDirectlyHeldThings().FirstOrDefault(x => x.TryGetComp<OxygenTank>() != null) is
                    ThingWithComps myTank)
                {
                    var nope = myTank.TryGetComp<OxygenTank>();
                    if (nope?.Air > 0)
                    {
                        if (Find.TickManager.TicksGame % 200 == 0)
                            nope.Air--;

                        return;
                    }
                }

                HealthUtility.AdjustSeverity(Daniel, HediffDef.Named("LZG_Drowning"), math);
            }

            else
            {
                HealthUtility.AdjustSeverity(Daniel, HediffDef.Named("LZG_Drowning"), -0.001f);
            }
        }

        //Checks if a place is shallow
        public static bool IsShallaws(this TerrainDef BoX)
        {
            return BoX == TerrainDefOf.WaterMovingShallow ||
                   BoX == TerrainDefOf.WaterOceanShallow ||
                   BoX == TerrainDefOf.WaterShallow;
        }

        //Checks if a place is deep.
        public static bool IsDeep(this TerrainDef BoX)
        {
            return BoX == TerrainDefOf.WaterMovingDeep ||
                   BoX == TerrainDefOf.WaterOceanDeep ||
                   BoX == TerrainDefOf.WaterDeep;
        }

        // Verse.PawnRenderer
        /// <summary>
        /// Render a character in a place.
        /// </summary>
        public static bool CutoffBody(PawnRenderer __instance, Vector3 rootLoc, Quaternion quat, bool renderBody,
            Rot4 bodyFacing, Rot4 headFacing, RotDrawMode bodyDrawType, bool portrait, bool headStump)
        {
            //Log.Message("1");
            var Daniel = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            //Log.Message("2");
            /////////////////////// Log.Message("Matthew teacher is wearing glassessssssssssssssss");
            if (Daniel == null || !(Daniel.PositionHeld is IntVec3 vec) || !vec.IsValid ||
                !(vec.GetTerrain(Daniel.MapHeld) is TerrainDef def)) return true;
            //Log.Message("3");
            try
            {
                if (!Find.TickManager.Paused)
                {
                    //Log.Message("4");
                    DrowningCheck(Daniel,
                        def); //j,hvkftyjdyfyfytetjyuhfdgtgvhjmb,nkjutvhrtrgtbhnmb,gu,j6yhdtgexwssr 5nhyj6ujhm,k.u,jhbyrhd4t3cy 7m6ki,l;kiu7yr5f43243rghy67ujklu;k'loi;i9hynt7g6rfvdcr4v3fr4tyu6i7kl8;9[p;kujhygtrfew2e45efr6ghbjnmkl,;.lkjnhgvf6cxd45fc6gv7bh8njukm0il,k0mjnhy8gvfr6gyvthyjuk0iujyhtgrvfcet5rgthyjukil,kujyhtgrvfecdwefrgyujnilmky애국가1.동해물과 백두산이 마르고 닳도록 하느님이 보우하 우리나라 만세 무궁화 삼천리 화려강산 대한사람 대한으로 길이 보전하세.
                    //Log.Message("5");
                }


                if (def.IsDeep())
                {
                    renderBody = false;
                    if (!__instance.graphics.AllResolved)
                    {
                        __instance.graphics.ResolveAllGraphics();
                    }

                    Mesh mesh = null;
                    Vector3 vector = rootLoc;
                    var a = rootLoc;
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
                        var b = quat * __instance.BaseHeadOffsetAt(headFacing);
                        var material = __instance.graphics.HeadMatAt(headFacing, bodyDrawType, headStump);
                        if (material != null)
                        {
                            var mesh2 = MeshPool.humanlikeHeadSet.MeshAt(headFacing);
                            GenDraw.DrawMeshNowOrLater(mesh2, a + b, quat, material, portrait);
                        }

                        var loc2 = rootLoc + b;
                        loc2.y += 0.0328125022f;
                        var flag = false;
                        if (!portrait || !Prefs.HatsOnlyOnMap)
                        {
                            var mesh3 = __instance.graphics.HairMeshSet.MeshAt(headFacing);
                            var apparelGraphics = __instance.graphics.apparelGraphics;
                            for (var j = 0; j < apparelGraphics.Count; j++)
                            {
                                if (apparelGraphics[j].sourceApparel.def.apparel.LastLayer != ApparelLayer.Overhead)
                                    continue;
                                flag = true;
                                var material2 = apparelGraphics[j].graphic.MatAt(bodyFacing, null);
                                material2 = __instance.graphics.flasher.GetDamagedMat(material2);
                                GenDraw.DrawMeshNowOrLater(mesh3, loc2, quat, material2, portrait);
                            }
                        }

                        if (!flag && bodyDrawType != RotDrawMode.Dessicated && !headStump)
                        {
                            var mesh4 = __instance.graphics.HairMeshSet.MeshAt(headFacing);
                            var mat = __instance.graphics.HairMatAt(headFacing);
                            GenDraw.DrawMeshNowOrLater(mesh4, loc2, quat, mat, portrait);
                        }
                    }

                    if (!portrait && Daniel.RaceProps.Animal && Daniel.inventory != null &&
                        Daniel.inventory.innerContainer.Count > 0)
                    {
                        Graphics.DrawMesh(mesh, vector, quat,
                            __instance.graphics.packGraphic.MatAt(Daniel.Rotation, null), 0);    
                    }
                    if (!portrait && Daniel.RaceProps.Animal)
                    {
                        var req = new MaterialRequest
                            (ContentFinder<Texture2D>.Get("WaterShadow"));
                        Vector3 s = new Vector3(2.0f, 1f, 2.0f);
                        Matrix4x4 matrix = default(Matrix4x4);
                        matrix.SetTRS(vector, Quaternion.identity, s);
                        Graphics.DrawMesh(MeshPool.plane10, matrix, MaterialPool.MatFrom(req), 0);
                        

                    }

                    if (portrait) return false;
                    AccessTools.Method(typeof(PawnRenderer), "DrawEquipment")
                        .Invoke(__instance, new object[] {rootLoc});
                    //__instance.DrawEquipment(rootLoc);
                    if (Daniel.apparel != null)
                    {
                        var wornApparel = Daniel.apparel.WornApparel;
                        for (var l = 0; l < wornApparel.Count; l++)
                        {
                            wornApparel[l].DrawWornExtras();
                        }
                    }

                    var bodyLoc = rootLoc;
                    bodyLoc.y += 0.0421875f;
                    var Tweety = Traverse.Create(__instance).Field("statusOverlays").GetValue<PawnHeadOverlays>();

                    Tweety.RenderStatusOverlays(bodyLoc, quat, MeshPool.humanlikeHeadSet.MeshAt(headFacing));
                    return false;
                }
            }
            catch
            {
            }

            return true;
        }

        private static void escape(Pawn pawn, IntVec3 __result)
        {
            Map map = pawn.Map;
            if (__result.IsValid &&
                __result.GetTerrain(map).IsDeep() ||
                __result.GetTerrain(map).IsShallaws())
            {

                for (int i = 0; i < 2; i++)
                {
                    int radius = (i != 0) ? 40 : 10;
                    IntVec3 result;
                    if (CellFinder.TryRandomClosewalkCellNear(pawn.Position, map, radius, out result,
                        (IntVec3 x) => !x.IsForbidden(pawn) && !x.GetTerrain(map).avoidWander &&!x.GetTerrain(map).IsShallaws()
                                       &&!x.GetTerrain(map).IsDeep()) 
                        )
                    {
                        __result = result;
                    }
                }

                __result = CellFinder.RandomClosewalkCellNearNotForbidden(pawn.Position, map, 40, pawn);
            }
        }
    }
}