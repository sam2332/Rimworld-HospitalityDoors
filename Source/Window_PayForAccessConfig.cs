using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace HospitalityDoors
{
    public class Window_PayForAccessConfig : Window
    {
        private readonly Building_Door[] doors;
        private readonly CompPayGate[] comps;
        
        private string costText = "";
        private Vector2 scrollPosition = Vector2.zero;
        
        public override Vector2 InitialSize => new Vector2(450f, 650f);
        
        public Window_PayForAccessConfig(Building_Door[] doors)
        {
            this.doors = doors;
            this.comps = doors.Select(d => d.GetComp<CompPayGate>()).Where(c => c != null).ToArray();
            
            // Initialize cost text with current value
            if (comps.Length > 0)
            {
                var firstCost = comps[0].EntryCost;
                if (comps.All(c => c.EntryCost == firstCost))
                {
                    costText = firstCost.ToString();
                }
                else
                {
                    costText = "Mixed";
                }
            }
            
            doCloseButton = true;
            doCloseX = true;
            forcePause = true;
            absorbInputAroundWindow = true;
        }
        
        public override void DoWindowContents(Rect inRect)
        {
            // Reserve space for the close button at the bottom
            var contentRect = new Rect(inRect.x, inRect.y, inRect.width, inRect.height - CloseButSize.y - 10f);
            
            // Calculate the height needed for all content
            var contentHeight = 800f; // Generous height to ensure everything fits
            
            var scrollViewRect = new Rect(0f, 0f, contentRect.width - 16f, contentHeight);
            
            Widgets.BeginScrollView(contentRect, ref scrollPosition, scrollViewRect);
            
            var listing = new Listing_Standard();
            listing.Begin(scrollViewRect);
            
            // Title
            Text.Font = GameFont.Medium;
            listing.Label("Pay-for-Access Door Configuration");
            Text.Font = GameFont.Small;
            
            listing.Gap();
            
            if (comps.Length == 0)
            {
                listing.Label("No valid doors selected.");
                listing.End();
                Widgets.EndScrollView();
                return;
            }
            
            // Door count info
            if (doors.Length == 1)
            {
                listing.Label($"Configuring: {doors[0].def.label}");
            }
            else
            {
                listing.Label($"Configuring {doors.Length} doors");
            }
            
            listing.Gap();
            
            // Enable/Disable Toggle
            var currentlyEnabled = comps.Any(c => c.IsEnabled);
            var enabledRect = listing.GetRect(24f);
            var wasEnabled = currentlyEnabled;
            
            Widgets.CheckboxLabeled(enabledRect, "Enable Pay-for-Access", ref currentlyEnabled);
            
            if (currentlyEnabled != wasEnabled)
            {
                foreach (var comp in comps)
                {
                    if (currentlyEnabled && comp.EntryCost == 0)
                    {
                        comp.EntryCost = 5; // Set reasonable default cost when enabling (since DefaultCost is 0 for disabled-by-default)
                    }
                    else if (!currentlyEnabled)
                    {
                        comp.EntryCost = 0; // Disable by setting cost to 0
                    }
                }
                
                // Update cost text when toggling
                if (currentlyEnabled && comps.Length > 0)
                {
                    var firstCost = comps[0].EntryCost;
                    if (comps.All(c => c.EntryCost == firstCost))
                    {
                        costText = firstCost.ToString();
                    }
                    else
                    {
                        costText = "Mixed";
                    }
                }
                else if (!currentlyEnabled)
                {
                    costText = "0";
                }
            }
            
            listing.Gap();
            
            // Only show other settings if enabled
            if (!currentlyEnabled)
            {
                listing.Label("Enable Pay-for-Access to configure settings.");
                listing.End();
                Widgets.EndScrollView();
                return;
            }
            
            // Entry Cost Section
            listing.Label("Entry Cost:");
            var costRect = listing.GetRect(30f);
            var costLabelRect = new Rect(costRect.x, costRect.y, 100f, costRect.height);
            var costFieldRect = new Rect(costRect.x + 105f, costRect.y, 80f, costRect.height);
            var costButtonRect = new Rect(costRect.x + 190f, costRect.y, 60f, costRect.height);
            
            Widgets.Label(costLabelRect, "Silver:");
            costText = Widgets.TextField(costFieldRect, costText);
            
            if (Widgets.ButtonText(costButtonRect, "Apply"))
            {
                if (int.TryParse(costText, out int newCost))
                {
                    foreach (var comp in comps)
                    {
                        comp.EntryCost = newCost;
                    }
                }
                else
                {
                    Messages.Message("Invalid cost value. Please enter a number.", MessageTypeDefOf.RejectInput);
                }
            }
            
            listing.Gap();
            
            // Quick cost buttons
            listing.Label("Quick set:");
            var quickButtonsRect = listing.GetRect(30f);
            var buttonWidth = 50f;
            var buttonSpacing = 55f;
            
            for (int i = 0; i < 5; i++)
            {
                var cost = (i + 1) * 5; // 5, 10, 15, 20, 25
                var buttonRect = new Rect(quickButtonsRect.x + i * buttonSpacing, quickButtonsRect.y, buttonWidth, quickButtonsRect.height);
                
                if (Widgets.ButtonText(buttonRect, cost.ToString()))
                {
                    foreach (var comp in comps)
                    {
                        comp.EntryCost = cost;
                    }
                    costText = cost.ToString();
                }
            }
            
            listing.Gap();
            listing.GapLine();
            
            // Payment Mode Section
            listing.Label("Payment Mode:");
            
            var payPerEntry = comps.FirstOrDefault()?.PayPerEntry ?? false;
            var mixedPayMode = !comps.All(c => c.PayPerEntry == payPerEntry);
            
            if (mixedPayMode)
            {
                listing.Label("Mixed payment modes selected");
            }
            
            var payOnceRect = listing.GetRect(24f);
            var payOnceChecked = !payPerEntry && !mixedPayMode;
            Widgets.CheckboxLabeled(payOnceRect, "Pay Once - Guests pay once, then access freely", ref payOnceChecked);
            
            if (payOnceChecked && (payPerEntry || mixedPayMode))
            {
                foreach (var comp in comps)
                {
                    comp.PayPerEntry = false;
                }
            }
            
            var payEachRect = listing.GetRect(24f);
            var payEachChecked = payPerEntry && !mixedPayMode;
            Widgets.CheckboxLabeled(payEachRect, "Pay Each Time - Guests pay every time they pass", ref payEachChecked);
            
            if (payEachChecked && (!payPerEntry || mixedPayMode))
            {
                foreach (var comp in comps)
                {
                    comp.PayPerEntry = true;
                }
            }
            
            listing.Gap();
            listing.GapLine();
            
            // Exemptions Section
            listing.Label("Exemptions (who doesn't need to pay):");
            
            var exemptColonists = comps.FirstOrDefault()?.ExemptColonists ?? true;
            var mixedColonists = !comps.All(c => c.ExemptColonists == exemptColonists);
            
            var colonistRect = listing.GetRect(24f);
            var colonistChecked = exemptColonists && !mixedColonists;
            Widgets.CheckboxLabeled(colonistRect, 
                "Exempt Colonists" + (mixedColonists ? " (Mixed)" : ""), 
                ref colonistChecked);
            
            if (colonistChecked != (exemptColonists && !mixedColonists))
            {
                foreach (var comp in comps)
                {
                    comp.ExemptColonists = colonistChecked;
                }
            }
            
            var exemptAllies = comps.FirstOrDefault()?.ExemptAllies ?? true;
            var mixedAllies = !comps.All(c => c.ExemptAllies == exemptAllies);
            
            var alliesRect = listing.GetRect(24f);
            var alliesChecked = exemptAllies && !mixedAllies;
            Widgets.CheckboxLabeled(alliesRect, 
                "Exempt Allied Factions" + (mixedAllies ? " (Mixed)" : ""), 
                ref alliesChecked);
            
            if (alliesChecked != (exemptAllies && !mixedAllies))
            {
                foreach (var comp in comps)
                {
                    comp.ExemptAllies = alliesChecked;
                }
            }
            
            var exemptPrisoners = comps.FirstOrDefault()?.ExemptPrisoners ?? false;
            var mixedPrisoners = !comps.All(c => c.ExemptPrisoners == exemptPrisoners);
            
            var prisonersRect = listing.GetRect(24f);
            var prisonersChecked = exemptPrisoners && !mixedPrisoners;
            Widgets.CheckboxLabeled(prisonersRect, 
                "Exempt Prisoners" + (mixedPrisoners ? " (Mixed)" : ""), 
                ref prisonersChecked);
            
            if (prisonersChecked != (exemptPrisoners && !mixedPrisoners))
            {
                foreach (var comp in comps)
                {
                    comp.ExemptPrisoners = prisonersChecked;
                }
            }
            
            var exemptRobots = comps.FirstOrDefault()?.ExemptRobots ?? true;
            var mixedRobots = !comps.All(c => c.ExemptRobots == exemptRobots);
            
            var robotsRect = listing.GetRect(24f);
            var robotsChecked = exemptRobots && !mixedRobots;
            Widgets.CheckboxLabeled(robotsRect, 
                "Exempt Robots" + (mixedRobots ? " (Mixed)" : ""), 
                ref robotsChecked);
            
            if (robotsChecked != (exemptRobots && !mixedRobots))
            {
                foreach (var comp in comps)
                {
                    comp.ExemptRobots = robotsChecked;
                }
            }
            
            // Always show animals as exempt (can't be changed)
            var animalRect = listing.GetRect(24f);
            var animalChecked = true;
            GUI.color = Color.gray;
            Widgets.CheckboxLabeled(animalRect, "Exempt Animals (always)", ref animalChecked);
            GUI.color = Color.white;
            
            listing.Gap();
            listing.GapLine();
            
            // Actions Section
            listing.Label("Actions:");
            
            // Clear paid guests (only show for pay-once mode)
            var anyPayOnce = comps.Any(c => !c.PayPerEntry);
            if (anyPayOnce)
            {
                if (listing.ButtonText("Clear Paid Guests List"))
                {
                    foreach (var comp in comps.Where(c => !c.PayPerEntry))
                    {
                        comp.ClearPaidPawns();
                    }
                    Messages.Message("Cleared paid guests list for selected doors.", MessageTypeDefOf.NeutralEvent);
                }
            }
            
            // Reset to defaults
            if (listing.ButtonText("Reset to Defaults"))
            {
                foreach (var comp in comps)
                {
                    comp.EntryCost = CompPayGate.DefaultCost == 0 ? 5 : CompPayGate.DefaultCost; // Use 5 if DefaultCost is 0 (disabled by default)
                    comp.PayPerEntry = false;
                    comp.ExemptColonists = true;
                    comp.ExemptAllies = true;
                    comp.ExemptPrisoners = false;
                    comp.ExemptRobots = true;
                    comp.ClearPaidPawns();
                }
                costText = (CompPayGate.DefaultCost == 0 ? 5 : CompPayGate.DefaultCost).ToString();
                Messages.Message("Reset all settings to defaults.", MessageTypeDefOf.NeutralEvent);
            }
            
            listing.Gap();
            
            // Lifetime Earnings Section
            listing.Label("Lifetime Earnings:");
            
            if (doors.Length == 1 && comps.Length == 1)
            {
                var comp = comps[0];
                var earnings = ((float)comp.LifetimeEarnings).ToStringMoney();
                listing.Label($"This door has earned: {earnings}");
                
                // Reset earnings button
                if (comp.LifetimeEarnings > 0)
                {
                    if (listing.ButtonText("Reset Earnings Counter"))
                    {
                        comp.ResetLifetimeEarnings();
                        Messages.Message("Lifetime earnings counter reset.", MessageTypeDefOf.NeutralEvent);
                    }
                }
                else
                {
                    listing.Label("No earnings yet.");
                }
            }
            else if (doors.Length > 1)
            {
                // Multiple doors - show total and individual
                var totalEarnings = comps.Sum(c => c.LifetimeEarnings);
                listing.Label($"Total earnings from all doors: {((float)totalEarnings).ToStringMoney()}");
                
                if (totalEarnings > 0)
                {
                    listing.Gap(6f);
                    listing.Label("Individual door earnings:");
                    for (int i = 0; i < doors.Length && i < comps.Length; i++)
                    {
                        var doorLabel = doors[i].def.label ?? "Door";
                        var earnings = ((float)comps[i].LifetimeEarnings).ToStringMoney();
                        listing.Label($"• {doorLabel}: {earnings}");
                    }
                    
                    listing.Gap(6f);
                    if (listing.ButtonText("Reset All Earnings Counters"))
                    {
                        foreach (var comp in comps)
                        {
                            comp.ResetLifetimeEarnings();
                        }
                        Messages.Message("All lifetime earnings counters reset.", MessageTypeDefOf.NeutralEvent);
                    }
                }
                else
                {
                    listing.Label("No earnings yet from any door.");
                }
            }
            
            listing.End();
            Widgets.EndScrollView();
        }
    }
}
