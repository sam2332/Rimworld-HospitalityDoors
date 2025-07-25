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
        private HashSet<Pawn> paidPawns = new HashSet<Pawn>();
        
        // Exemption settings
        private bool exemptColonists = true;
        private bool exemptAllies = true;
        private bool exemptPrisoners = false;
        
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
                    paidPawns.Clear();
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
        
        public bool IsEnabled => entryCost > 0;
        
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
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
            Scribe_Collections.Look(ref paidPawns, "paidPawns", LookMode.Reference);
            
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                paidPawns ??= new HashSet<Pawn>();
                // Clean up any null references
                paidPawns.RemoveWhere(p => p == null || p.Dead || !p.Spawned);
            }
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
            
            // Exempt allies
            if (exemptAllies && pawn.Faction != null && pawn.Faction.RelationKindWith(Faction.OfPlayer) == FactionRelationKind.Ally) return true;
            
            // Exempt prisoners if enabled
            if (exemptPrisoners && pawn.IsPrisoner) return true;
            
            // Always exempt animals (they don't carry silver anyway)
            if (pawn.RaceProps.Animal) return true;
            
            return false;
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
            
            // If pay-per-entry mode, always need to pay
            if (payPerEntry) return true;
            
            // If pay-once mode, check if they've already paid
            return !paidPawns.Contains(pawn);
        }
        
        /// <summary>
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
            
            // Track payment for pay-once mode
            if (!payPerEntry)
                paidPawns.Add(pawn);
            
            // Show payment confirmation
            MoteMaker.ThrowText(pawn.Position.ToVector3(), pawn.Map, $"Paid {((float)entryCost).ToStringMoney()}", Color.green, 3.5f);
            
            return true;
        }
        
        /// <summary>
        /// Clears the paid pawns list (useful for pay-once mode reset)
        /// </summary>
        public void ClearPaidPawns()
        {
            paidPawns.Clear();
        }
        
        public override string CompInspectStringExtra()
        {
            if (!IsEnabled) return null;
            
            var parts = new List<string>();
            
            parts.Add($"Entry cost: {((float)entryCost).ToStringMoney()}");
            parts.Add($"Mode: {(payPerEntry ? "Pay every time" : "Pay once")}");
            
            if (!payPerEntry && paidPawns.Count > 0)
                parts.Add($"Paid guests: {paidPawns.Count}");
                
            var exemptions = new List<string>();
            if (exemptColonists) exemptions.Add("colonists");
            if (exemptAllies) exemptions.Add("allies");
            if (exemptPrisoners) exemptions.Add("prisoners");
            exemptions.Add("animals"); // Always exempt
            
            if (exemptions.Count > 0)
                parts.Add($"Exempt: {string.Join(", ", exemptions)}");
            
            return string.Join("\n", parts);
        }
    }
}
