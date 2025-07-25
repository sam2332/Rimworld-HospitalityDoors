using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace HospitalityDoors
{
    [HarmonyPatch(typeof(Building_Door))]
    public static class Building_Door_Patches
    {
        // Patch IsForbiddenToPass to check for payment requirements
        [HarmonyPatch(typeof(ForbidUtility), nameof(ForbidUtility.IsForbiddenToPass))]
        [HarmonyPostfix]
        public static void IsForbiddenToPass_Postfix(Building_Door t, Pawn pawn, ref bool __result)
        {
            // If already forbidden by base game logic, don't override
            if (__result) return;
            
            var comp = t?.GetComp<CompPayGate>();
            if (comp == null || !comp.IsEnabled) return;
            
            // Check if this pawn needs to pay and can't afford it
            if (comp.NeedsToPayFor(pawn) && !comp.CanAfford(pawn, out _))
            {
                __result = true; // Forbid passage
            }
        }
        
        // Patch StartManualOpenBy to handle payment
        [HarmonyPatch(nameof(Building_Door.StartManualOpenBy))]
        [HarmonyPrefix]
        public static bool StartManualOpenBy_Prefix(Building_Door __instance, Pawn opener)
        {
            var comp = __instance.GetComp<CompPayGate>();
            if (comp == null || !comp.IsEnabled) return true; // Proceed normally
            
            // Try to charge the pawn for entry
            if (!comp.TryChargeEntry(opener))
            {
                // Payment failed - don't open the door
                return false;
            }
            
            // Payment successful - proceed with normal door opening
            return true;
        }
        
        // Add our gizmo to doors that have the PayGate component
        [HarmonyPatch(nameof(Building_Door.GetGizmos))]
        [HarmonyPostfix]
        public static void GetGizmos_Postfix(Building_Door __instance, ref System.Collections.Generic.IEnumerable<Gizmo> __result)
        {
            try
            {
                var comp = __instance.GetComp<CompPayGate>();
                if (comp == null) return;
                
                var gizmos = __result.ToList();
                
                // Add the single "Paid Door?" configuration gizmo
                var selectedDoors = new[] { __instance };
                gizmos.Add(new Gizmo_PaidDoor(selectedDoors));
                
                __result = gizmos;
            }
            catch (System.Exception ex)
            {
                Log.Error($"[HospitalityDoors] Error in GetGizmos_Postfix: {ex}");
            }
        }
    }
}
