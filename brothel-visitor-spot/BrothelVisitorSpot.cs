using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using Verse.AI.Group;
using Verse;
using Hospitality;
using UnityEngine;

namespace BrothelVisitorSpot
{
    public class BrothelVisitorSpot : Building
    {
        private int count = 0;

        public BrothelVisitorSpot()
        {
            if (Current.Game.CurrentMap != null)
            {
                foreach (Building thisBuilding in Current.Game.CurrentMap.listerBuildings.allBuildingsColonist)
                {
                    if (thisBuilding.def.defName.Equals("GCBVS_BrothelVisitorSpot"))
                    {
                        thisBuilding.Destroy(DestroyMode.Vanish);
                        Messages.Message("BrothelVisitorSpot.AlreadyOnMap".Translate(), MessageTypeDefOf.NegativeEvent);
                        break;
                    }
                }
            }
        }

        public override void Tick()
        {
            base.Tick();
            ++this.count;
            if (this.count % 600 == 1)
            {
                this.count = 0;
                List<Lord> currentMapLords = base.Map.lordManager.lords;
                IntVec3 spotPosition = base.Position;
                foreach (var thisLord in currentMapLords)
                {
                    if (this.CheckVisitor(thisLord.LordJob))
                    {
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
            //Only set visitors to brothel visitor spot if they are not hospitality's visitors
            if (lordJob is Hospitality.LordJob_VisitColony)
                return false;
            else if (lordJob is RimWorld.LordJob_VisitColony)
                return true;
            return false;
        }
    }
}