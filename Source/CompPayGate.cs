using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace HospitalityDoors
{
    public class CompPayGate : ThingComp
    {
        public const int DefaultCost = 5;
        public const int CostStep = 5;
        
        private int entryCost = DefaultCost;
        private bool payPerEntry = false;
        // Use Lists for both runtime and serialization
        private List<Pawn> paidPawnsList = new List<Pawn>();
        private int lifetimeEarnings = 0;
        // Track pawns with a one-time pass (allowed to exit for free)
        private List<Pawn> oneTimePassPawns = new List<Pawn>();
        
        // Exemption settings
        private bool exemptColonists = true;
        private bool exemptAllies = true;
        private bool exemptPrisoners = false;
        private bool exemptRobots = true;
        private bool exemptAnimals = true;
        
        public CompProperties_PayGate Props => (CompProperties_PayGate)props;
        
        public int EntryCost
        {
            get => entryCost;
            set => entryCost = Mathf.Clamp(value, 0, 9999);
        }
        
        public bool PayPerEntry
        {
            get => payPerEntry;
            set
            {
                payPerEntry = value;
                if (!value) // If switching to pay-once mode, clear the list
                    paidPawnsList.Clear();
            }
        }
        
        public bool ExemptColonists
        {
            get => exemptColonists;
            set => exemptColonists = value;
        }
        
        public bool ExemptAllies
        {
            get => exemptAllies;
            set => exemptAllies = value;
        }
        
        public bool ExemptPrisoners
        {
            get => exemptPrisoners;
            set => exemptPrisoners = value;
        }
        
        public bool ExemptRobots
        {
            get => exemptRobots;
            set => exemptRobots = value;
        }
        
        public int LifetimeEarnings => lifetimeEarnings;
        
        public bool IsEnabled => entryCost > 0;
        
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            
            // Debug logging to confirm component is loaded
            Log.Message($"[HospitalityDoors] CompPayGate loaded on {parent.def.defName} - Respawning: {respawningAfterLoad}");
            
            if (!respawningAfterLoad)
            {
                // Initialize with default values from Props if available
                if (Props != null)
                {
                    entryCost = Props.defaultCost;
                    payPerEntry = Props.defaultPayPerEntry;
                    exemptColonists = Props.defaultExemptColonists;
                    exemptAllies = Props.defaultExemptAllies;
                    exemptPrisoners = Props.defaultExemptPrisoners;
                    exemptRobots = Props.defaultExemptRobots;
                    
                    Log.Message($"[HospitalityDoors] Initialized with cost: {entryCost}, enabled: {IsEnabled}");
                }
                else
                {
                    Log.Warning("[HospitalityDoors] Props is null during initialization!");
                }
            }
        }
        
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref entryCost, "entryCost", DefaultCost);
            Scribe_Values.Look(ref payPerEntry, "payPerEntry", false);
            Scribe_Values.Look(ref exemptColonists, "exemptColonists", true);
            Scribe_Values.Look(ref exemptAllies, "exemptAllies", true);
            Scribe_Values.Look(ref exemptPrisoners, "exemptPrisoners", false);
            Scribe_Values.Look(ref exemptRobots, "exemptRobots", true);
            Scribe_Values.Look(ref lifetimeEarnings, "lifetimeEarnings", 0);
            
            // Use unique identifiers based on parent to avoid conflicts
            var paidPawnsKey = $"paidPawnsList_{parent?.thingIDNumber ?? 0}";
            var oneTimePassKey = $"oneTimePassPawns_{parent?.thingIDNumber ?? 0}";
            
            Scribe_Collections.Look(ref paidPawnsList, paidPawnsKey, LookMode.Reference);
            Scribe_Collections.Look(ref oneTimePassPawns, oneTimePassKey, LookMode.Reference);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                // Initialize lists if they're null after loading
                paidPawnsList = paidPawnsList ?? new List<Pawn>();
                oneTimePassPawns = oneTimePassPawns ?? new List<Pawn>();
                
                // Clean up null references and dead/despawned pawns
                paidPawnsList.RemoveAll(p => p == null || p.Dead || !p.Spawned);
                oneTimePassPawns.RemoveAll(p => p == null || p.Dead || !p.Spawned);
            }
        }
        
        /// <summary>
        /// Checks if a pawn is a robot using reflection to avoid hard dependencies
        /// </summary>
        private static bool IsRobot(Pawn pawn)
        {
            if (pawn?.def?.defName == null) return false;
            
            // Check for Misc. Robots++ robots using reflection
            try
            {
                var robotType = Type.GetType("AIRobot.X2_AIRobot, Assembly-CSharp");
                if (robotType != null && robotType.IsAssignableFrom(pawn.GetType()))
                    return true;
            }
            catch (Exception ex)
            {
                // Silently ignore reflection errors - mod probably not installed
                Log.WarningOnce($"[HospitalityDoors] Robot type check failed: {ex.Message}", pawn.def.defName.GetHashCode());
            }
            
            // Fallback: Check by def pattern for additional safety
            var defName = pawn.def.defName;
            return defName.Contains("Robot") || 
                   defName.StartsWith("AIRobot_") ||
                   defName.StartsWith("RPP_Bot_") ||
                   defName.Contains("_Bot_") ||
                   defName.EndsWith("Bot");
        }
        
        /// <summary>
        /// Checks if a pawn should be allowed through the door without payment
        /// </summary>
        public bool IsExempt(Pawn pawn)
        {
            if (!IsEnabled) return true;
            
            // Always exempt colonists if enabled (default)
            if (exemptColonists && pawn.IsColonist) return true;
            
            // Also exempt any player-controlled pawns (colonists, slaves under player control, etc.)
            if (exemptColonists && pawn.IsColonistPlayerControlled) return true;
            
            // Exempt allies (but don't check relation if it's the player faction itself)
            if (exemptAllies && pawn.Faction != null && pawn.Faction != Faction.OfPlayer && pawn.Faction.RelationKindWith(Faction.OfPlayer) == FactionRelationKind.Ally) return true;
            
            // Exempt prisoners if enabled
            if (exemptPrisoners && pawn.IsPrisoner) return true;
            
            // Exempt animals (they don't carry silver anyway)
            if (pawn.RaceProps.Animal && exemptAnimals) return true;
            
            // Exempt robots if enabled (default)
            if (exemptRobots && IsRobot(pawn)) return true;
            
            // Fire escape protocol: Allow visitors to leave the map without payment
            if (IsLeavingMap(pawn)) return true;
            
            return false;
        }
        
        /// <summary>
        /// Checks if a pawn is trying to leave the map (fire escape protocol)
        /// </summary>
        private bool IsLeavingMap(Pawn pawn)
        {
            if (pawn?.pather?.Moving != true) return false;
            
            // Check if the pawn's destination is near a map edge
            var destination = pawn.pather.Destination;
            if (!destination.IsValid) return false;
            
            var map = pawn.Map;
            if (map == null) return false;
            
            var destCell = destination.Cell;
            var mapSize = map.Size;
            
            // Consider a pawn as "leaving" if their destination is within 3 cells of any map edge
            const int exitZone = 3;
            bool nearEdge = destCell.x <= exitZone || 
                           destCell.z <= exitZone || 
                           destCell.x >= mapSize.x - exitZone || 
                           destCell.z >= mapSize.z - exitZone;
            
            if (!nearEdge) return false;
            
            // Additional check: is this a guest/visitor trying to leave?
            // This prevents colonists from abusing the fire escape to avoid payment
            if (pawn.IsColonist || pawn.IsColonistPlayerControlled) return false;
            
            return true;
        }
        
        /// <summary>
        /// Checks if a pawn can afford the entry cost
        /// </summary>
        public bool CanAfford(Pawn pawn, out Thing silver)
        {
            silver = pawn.inventory?.innerContainer?.FirstOrDefault(i => i.def == ThingDefOf.Silver);
            if (silver == null) return false;
            return silver.stackCount >= entryCost;
        }
        
        /// <summary>
        /// Checks if a pawn needs to pay (considering pay-once mode)
        /// </summary>
        public bool NeedsToPayFor(Pawn pawn)
        {
            if (!IsEnabled) return false;
            if (IsExempt(pawn)) return false;

            // If pawn has a one-time pass, allow free exit and remove from list
            if (oneTimePassPawns.Contains(pawn))
            {
                oneTimePassPawns.Remove(pawn);
                return false;
            }

            // If pay-per-entry mode, always need to pay
            if (payPerEntry) return true;

            // If pay-once mode, check if they've already paid
            return !paidPawnsList.Contains(pawn);
        }
        /// Attempts to charge the pawn for entry
        /// </summary>
        public bool TryChargeEntry(Pawn pawn)
        {
            if (!NeedsToPayFor(pawn)) return true;

            if (!CanAfford(pawn, out Thing silver))
            {
                // Show "can't afford" message
                MoteMaker.ThrowText(pawn.Position.ToVector3(), pawn.Map, "Can't afford entry", Color.red, 3.5f);
                return false;
            }

            // Deduct payment
            silver.stackCount -= entryCost;
            if (silver.stackCount <= 0)
                silver.Destroy();

            // Track lifetime earnings
            lifetimeEarnings += entryCost;

            // Track payment for pay-once mode
            if (!payPerEntry)
                paidPawnsList.Add(pawn);

            // Give pawn a one-time pass to exit for free
            if (!oneTimePassPawns.Contains(pawn))
                oneTimePassPawns.Add(pawn);

            // Drop payment as silver at the door location
            DropSilverAtDoor(entryCost);

            // Show payment confirmation
            MoteMaker.ThrowText(pawn.Position.ToVector3(), pawn.Map, $"Paid {((float)entryCost).ToStringMoney()}", Color.green, 3.5f);

            return true;
        }
        
        /// <summary>
        /// Drops the collected silver payment at the door location
        /// </summary>
        private void DropSilverAtDoor(int amount)
        {
            if (amount <= 0) return;
            
            var silverThing = ThingMaker.MakeThing(ThingDefOf.Silver);
            silverThing.stackCount = amount;
            
            // Try to drop the silver at the door's position
            IntVec3 dropPosition = parent.Position;
            
            // If the door position is blocked, find a nearby free spot
            if (!dropPosition.Standable(parent.Map))
            {
                // Look for adjacent cells that are standable
                var adjacentCells = GenAdj.CellsAdjacent8Way(parent.Position, parent.Rotation, parent.def.size);
                foreach (var cell in adjacentCells)
                {
                    if (cell.InBounds(parent.Map) && cell.Standable(parent.Map))
                    {
                        dropPosition = cell;
                        break;
                    }
                }
                
                // If no adjacent cells work, try a wider search
                if (!dropPosition.Standable(parent.Map))
                {
                    if (!CellFinder.TryFindRandomCellNear(parent.Position, parent.Map, 3, (IntVec3 c) => c.Standable(parent.Map), out dropPosition))
                    {
                        // Fallback: just use the door position even if blocked
                        dropPosition = parent.Position;
                    }
                }
            }
            
            // Drop the silver
            GenPlace.TryPlaceThing(silverThing, dropPosition, parent.Map, ThingPlaceMode.Near);
        }
        
        /// <summary>
        /// Clears the paid pawns list (useful for pay-once mode reset)
        /// </summary>
        public void ClearPaidPawns()
        {
            paidPawnsList.Clear();
            oneTimePassPawns.Clear();
        }
        
        /// <summary>
        /// Resets the lifetime earnings counter
        /// </summary>
        public void ResetLifetimeEarnings()
        {
            lifetimeEarnings = 0;
        }
        
        public override string CompInspectStringExtra()
        {
            if (!IsEnabled) return null;
            
            var parts = new List<string>();
            
            parts.Add($"Entry cost: {((float)entryCost).ToStringMoney()}");
            parts.Add($"Mode: {(payPerEntry ? "Pay every time" : "Pay once")}");
            
            if (!payPerEntry && paidPawnsList.Count > 0)
                parts.Add($"Paid guests: {paidPawnsList.Count}");
            
            // Always show lifetime earnings for enabled doors
            parts.Add($"Lifetime earnings: {((float)lifetimeEarnings).ToStringMoney()}");
                
            var exemptions = new List<string>();
            if (exemptColonists) exemptions.Add("colonists");
            if (exemptAllies) exemptions.Add("allies");
            if (exemptPrisoners) exemptions.Add("prisoners");
            if (exemptRobots) exemptions.Add("robots");
            exemptions.Add("animals"); // Always exempt
            exemptions.Add("visitors leaving map"); // Fire escape protocol
            
            if (exemptions.Count > 0)
                parts.Add($"Exempt: {string.Join(", ", exemptions)}");
            
            return string.Join("\n", parts);
        }
    }
}
