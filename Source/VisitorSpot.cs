using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using Verse.AI.Group;
using Verse;
using Hospitality;
using UnityEngine;

namespace VisitorSpot
{
    public class VisitorSpot : Building
    {
        private int tickCount = 0;

        //Ensure only one visitor spot exists in the current map
        public VisitorSpot()
        {
            if (Current.Game.CurrentMap != null)
            {
                foreach (Building thisBuilding in Current.Game.CurrentMap.listerBuildings.allBuildingsColonist)
                {
                    if (thisBuilding.def.defName.Equals("GCBVS_VisitorSpot"))
                    {
                        thisBuilding.Destroy(DestroyMode.Vanish);
                        Messages.Message("GCBVS_VisitorSpot.AlreadyOnMap".Translate(), MessageTypeDefOf.NegativeEvent);
                        break;
                    }
                }
            }
        }

        public override void Tick()
        {
            base.Tick();
            ++this.tickCount;
            if (this.tickCount % 600 == 1)
            {
                //Check every 600 ticks in game, about 10 seconds in real world if game is set to run at 1x speed
                
                this.tickCount = 0;

                //Get all group behavior groups in the current map
                List<Lord> currentMapLords = base.Map.lordManager.lords;

                //Get this visitor spot's position
                IntVec3 spotPosition = base.Position;
                foreach (var thisLord in currentMapLords)
                {
                    //Process every group
                    if (this.CheckVisitor(thisLord.LordJob))
                    {
                        //Continue only if this group of visitors isn't Hospitality's visitor
                        FieldInfo chillSpotFI = thisLord.LordJob.GetType().GetField("chillSpot", BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                        chillSpotFI.SetValue(thisLord.LordJob, spotPosition);
                        LordToil currentLordToil = thisLord.CurLordToil;
                        if (currentLordToil is LordToil_Travel lordToil_Travel)
                        {
                            if (lordToil_Travel.FlagLoc != spotPosition)
                            {
                                lordToil_Travel.SetDestination(spotPosition);
                                lordToil_Travel.UpdateAllDuties();
                            }
                        }
                        else if (currentLordToil is LordToil_DefendPoint lordToil_DefendPoint)
                        {
                            if (lordToil_DefendPoint.FlagLoc != spotPosition)
                            {
                                lordToil_DefendPoint.SetDefendPoint(spotPosition);
                                lordToil_DefendPoint.UpdateAllDuties();
                            }
                        }
                    }
                }
            }
        }

        private bool CheckVisitor(LordJob lordJob)
        {
            //Only set common visitors to visitor spot if they are not hospitality's visitors.
            if (lordJob is Hospitality.LordJob_VisitColony)
                return false;
            else if (lordJob is RimWorld.LordJob_VisitColony)
                return true;
            return false;
        }
    }
}