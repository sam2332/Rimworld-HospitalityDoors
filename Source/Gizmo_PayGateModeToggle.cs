using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace HospitalityDoors
{
    public class Gizmo_PayGateModeToggle : Command
    {
        private readonly Building_Door[] doors;
        private CompPayGate comp;

        public Gizmo_PayGateModeToggle(Building_Door[] doors)
        {
            this.doors = doors;
            this.comp = doors.FirstOrDefault()?.GetComp<CompPayGate>();
            
            // Set up the gizmo appearance
            this.Order = -100f; // Show before other gizmos
            
            UpdateGizmoAppearance();
        }

        private void UpdateGizmoAppearance()
        {
            if (comp == null) return;
            
            if (comp.PayPerEntry)
            {
                this.defaultLabel = "Pay Each";
                this.defaultDesc = "Guests must pay every time they pass through this door.\n\nClick to change to 'Pay Once' mode.";
                this.icon = ContentFinder<Texture2D>.Get("UI/Icons/PayEach", false) ?? TexCommand.Attack;
            }
            else
            {
                this.defaultLabel = "Pay Once";
                this.defaultDesc = "Guests only need to pay once, then can pass freely.\n\nClick to change to 'Pay Each' mode.";
                this.icon = ContentFinder<Texture2D>.Get("UI/Icons/PayOnce", false) ?? TexCommand.ForbidOff;
            }
        }

        public override bool GroupsWith(Gizmo other)
        {
            return other is Gizmo_PayGateModeToggle;
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            // Refresh component reference in case it changed
            comp = doors.FirstOrDefault()?.GetComp<CompPayGate>();
            if (comp == null)
            {
                return new GizmoResult(GizmoState.Clear);
            }

            UpdateGizmoAppearance();

            Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
            
            bool mouseOver = Mouse.IsOver(rect);
            if (mouseOver && !disabled)
            {
                GUI.color = GenUI.MouseoverColor;
            }
            
            GUI.DrawTexture(rect, Command.BGTex);
            
            // Draw the icon
            Rect iconRect = new Rect(rect.x + 5f, rect.y + 5f, rect.width - 10f, rect.height - 25f);
            GUI.color = disabled ? Color.grey : Color.white;
            GUI.DrawTexture(iconRect, icon);
            
            // Draw the label at the bottom
            Text.Font = GameFont.Tiny;
            Rect labelRect = new Rect(rect.x + 2f, rect.y + rect.height - 20f, rect.width - 4f, 18f);
            Text.Anchor = TextAnchor.MiddleCenter;
            
            // Use different colors for different modes to make it more obvious
            GUI.color = comp.PayPerEntry ? new Color(1f, 0.8f, 0.2f) : new Color(0.2f, 0.8f, 0.2f); // Orange for "Pay Each", Green for "Pay Once"
            
            Widgets.Label(labelRect, defaultLabel);
            
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;
            GUI.color = Color.white;
            
            // Handle tooltip
            if (mouseOver)
            {
                TooltipHandler.TipRegion(rect, defaultDesc);
            }
            
            // Handle click
            if (Widgets.ButtonInvisible(rect, true) && !disabled)
            {
                TogglePaymentMode();
                return new GizmoResult(GizmoState.Interacted, Event.current);
            }
            
            return new GizmoResult(mouseOver ? GizmoState.Mouseover : GizmoState.Clear);
        }
        
        private void TogglePaymentMode()
        {
            foreach (var door in doors)
            {
                var doorComp = door.GetComp<CompPayGate>();
                if (doorComp != null)
                {
                    doorComp.PayPerEntry = !doorComp.PayPerEntry;
                }
            }
            
            // Show a message to confirm the change
            var newMode = comp?.PayPerEntry ?? false;
            string modeText = newMode ? "Pay Each Time" : "Pay Once";
            Messages.Message($"Payment mode changed to: {modeText}", MessageTypeDefOf.NeutralEvent, false);
        }

        public override float GetWidth(float maxWidth)
        {
            return 75f;
        }
    }
}
