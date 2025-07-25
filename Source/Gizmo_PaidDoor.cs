using RimWorld;
using UnityEngine;
using Verse;

namespace HospitalityDoors
{
    public class Gizmo_PaidDoor : Command
    {
        private readonly Building_Door[] doors;

        public Gizmo_PaidDoor(Building_Door[] doors)
        {
            this.doors = doors;
            this.defaultLabel = "Paid Door?";
            this.defaultDesc = "Configure pay-for-access settings for this door.\n\nClick to open configuration window.";
            this.icon = ContentFinder<Texture2D>.Get("UI/Commands/PaidDoor", false) ?? TexCommand.Install;
            this.Order = -90f;
        }

        public override void ProcessInput(Event ev)
        {
            base.ProcessInput(ev);
            
            // Open the configuration window
            var window = new Window_PayForAccessConfig(doors);
            Find.WindowStack.Add(window);
        }

        public override bool GroupsWith(Gizmo other)
        {
            return other is Gizmo_PaidDoor;
        }
    }
}
