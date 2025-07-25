using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace HospitalityDoors
{
    public class Gizmo_PayGateDoor : Gizmo_ModifyNumber<Building_Door>
    {
        private readonly TaggedString entryCost;
        private readonly TaggedString payMode;
        
        public Gizmo_PayGateDoor(Building_Door[] doors) : base(doors)
        {
            Log.Message($"[HospitalityDoors] Creating gizmo for {doors.Length} doors");
            
            var payGates = doors.Select(d => d.GetComp<CompPayGate>()).Where(c => c != null).ToArray();
            
            if (doors.Length == 1 && payGates.Length == 1)
            {
                var gate = payGates.First();
                Title = "Pay-to-Access Door";
                entryCost = ((float)gate.EntryCost).ToStringMoney();
                payMode = gate.PayPerEntry ? "Every time" : "Pay once";
            }
            else
            {
                Title = "Pay-to-Access Doors";
                entryCost = ToFromToString(d => d.GetComp<CompPayGate>()?.EntryCost ?? 0, i => ((float)i).ToStringMoney());
                payMode = ToFromToString(d => d.GetComp<CompPayGate>()?.PayPerEntry ?? false, b => b ? "Every time" : "Pay once");
            }
        }
        
        protected override Color ButtonColor => new Color(0.2f, 0.6f, 1f); // Nice blue color
        
        protected override string Title { get; }
        
        protected override void ButtonDown()
        {
            foreach (var door in selection)
            {
                var comp = door.GetComp<CompPayGate>();
                if (comp != null)
                    comp.EntryCost -= CompPayGate.CostStep;
            }
        }
        
        protected override void ButtonUp()
        {
            foreach (var door in selection)
            {
                var comp = door.GetComp<CompPayGate>();
                if (comp != null)
                    comp.EntryCost += CompPayGate.CostStep;
            }
        }
        
        protected override void ButtonCenter()
        {
            foreach (var door in selection)
            {
                var comp = door.GetComp<CompPayGate>();
                if (comp != null)
                    comp.EntryCost = CompPayGate.DefaultCost;
            }
        }
        
        protected override void DrawInfoRect(Rect rect)
        {
            LabelRow(ref rect, "Entry Cost:", entryCost);
            LabelRow(ref rect, "Mode:", payMode);
        }
        
        protected override void DrawTooltipBox(Rect totalRect)
        {
            if (!Mouse.IsOver(totalRect)) return;
            if (selection.Length != 1) return;
            
            var comp = selection[0].GetComp<CompPayGate>();
            if (comp == null) return;
            
            // Create detailed tooltip with safe access to comp data
            var door = selection[0];
            TooltipHandler.TipRegion(totalRect, () =>
            {
                // Safely get the component again inside the lambda to avoid null reference issues
                var safeComp = door?.GetComp<CompPayGate>();
                if (safeComp == null) return "Pay-to-Access Door (Error loading data)";
                
                var tip = "Pay-to-Access Door Settings\n\n";
                tip += $"Entry Cost: {((float)safeComp.EntryCost).ToStringMoney()}\n";
                tip += $"Payment Mode: {(safeComp.PayPerEntry ? "Pay every time" : "Pay once per guest")}\n\n";
                
                tip += "Exemptions:\n";
                if (safeComp.ExemptColonists) tip += "• Colonists\n";
                if (safeComp.ExemptAllies) tip += "• Allied pawns\n";
                if (safeComp.ExemptPrisoners) tip += "• Prisoners\n";
                tip += "• Animals (always exempt)\n";
                
                try
                {
                    if (!safeComp.PayPerEntry)
                    {
                        var inspectString = safeComp.CompInspectStringExtra();
                        if (!string.IsNullOrEmpty(inspectString) && inspectString.Contains("Paid guests:"))
                        {
                            var paidLine = inspectString.Split('\n').FirstOrDefault(s => s.Contains("Paid guests:"));
                            if (!string.IsNullOrEmpty(paidLine))
                            {
                                tip += $"\n{paidLine}";
                            }
                        }
                    }
                }
                catch
                {
                    // Ignore any errors when trying to get inspect string
                }
                
                tip += "\n\nLeft-click buttons to adjust cost\nShift+Left-click or Middle-click to toggle mode\nRight-click for advanced options";
                return tip;
            }, door.GetHashCode() * 7829);
        }
        
        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            var result = base.GizmoOnGUI(topLeft, maxWidth, parms);
            
            // Handle right-click for advanced options
            var rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
            if (Mouse.IsOver(rect) && Event.current.type == EventType.MouseDown && Event.current.button == 1)
            {
                ShowAdvancedOptions();
                Event.current.Use();
                return new GizmoResult(GizmoState.Interacted);
            }
            
            // Also handle middle-click or shift+left-click to toggle payment mode quickly
            if (Mouse.IsOver(rect) && Event.current.type == EventType.MouseDown && 
                (Event.current.button == 2 || (Event.current.button == 0 && Event.current.shift)))
            {
                TogglePaymentMode();
                Event.current.Use();
                return new GizmoResult(GizmoState.Interacted);
            }
            
            return result;
        }
        
        private void TogglePaymentMode()
        {
            foreach (var door in selection)
            {
                var comp = door.GetComp<CompPayGate>();
                if (comp != null)
                {
                    comp.PayPerEntry = !comp.PayPerEntry;
                }
            }
            
            var newMode = selection.FirstOrDefault()?.GetComp<CompPayGate>()?.PayPerEntry ?? false;
            Messages.Message($"Payment mode set to: {(newMode ? "Pay every time" : "Pay once")}", MessageTypeDefOf.NeutralEvent);
        }
        
        private void ShowAdvancedOptions()
        {
            var options = new List<FloatMenuOption>();
            
            // Payment mode toggle
            var currentMode = selection.FirstOrDefault()?.GetComp<CompPayGate>()?.PayPerEntry ?? false;
            var newMode = !currentMode;
            options.Add(new FloatMenuOption(
                $"Set mode: {(newMode ? "Pay every time" : "Pay once")}",
                () =>
                {
                    foreach (var door in selection)
                    {
                        var comp = door.GetComp<CompPayGate>();
                        if (comp != null) comp.PayPerEntry = newMode;
                    }
                }
            ));
            
            // Exemption toggles
            options.Add(new FloatMenuOption("Toggle colonist exemption", () => ToggleExemption(c => c.ExemptColonists = !c.ExemptColonists)));
            options.Add(new FloatMenuOption("Toggle ally exemption", () => ToggleExemption(c => c.ExemptAllies = !c.ExemptAllies)));
            options.Add(new FloatMenuOption("Toggle prisoner exemption", () => ToggleExemption(c => c.ExemptPrisoners = !c.ExemptPrisoners)));
            
            // Clear paid pawns (for pay-once mode)
            if (!currentMode)
            {
                options.Add(new FloatMenuOption("Clear paid guests list", () =>
                {
                    foreach (var door in selection)
                    {
                        var comp = door.GetComp<CompPayGate>();
                        comp?.ClearPaidPawns();
                    }
                    Messages.Message("Cleared paid guests list for selected doors.", MessageTypeDefOf.NeutralEvent);
                }));
            }
            
            Find.WindowStack.Add(new FloatMenu(options));
        }
        
        private void ToggleExemption(Action<CompPayGate> toggle)
        {
            foreach (var door in selection)
            {
                var comp = door.GetComp<CompPayGate>();
                if (comp != null) toggle(comp);
            }
        }
    }
}
