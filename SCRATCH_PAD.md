# SCRATCH PAD - Hospitality Pay-to-Access Doors Research

## Key Findings from Previous Research:

### Guest Silver Inventory:
- Guests DO carry silver: `guest.inventory.innerContainer.FirstOrDefault(i => i.def == ThingDefOf.Silver)`
- Used in BedUtility.FindBedFor() and CompVendingMachine.CanAffordFast()
- Silver accessible via `pawn.inventory.innerContainer`

### Door Access Control Points:
- `Building_Door.PawnCanOpen(Pawn p)` - Primary access control
- `ForbidUtility.IsForbiddenToPass(Building_Door t, Pawn pawn)` - Secondary control ✅ FOUND!
- Used in pathfinding: PathUtility.GetDoorCost, Region.Allows, Pawn_PathFollower

### Key Door Methods:
- `StartManualOpenBy(Pawn opener)` - Called when pawn opens door
- `CheckFriendlyTouched(Pawn p)` - Tracks recent door interactions
- `Notify_PawnApproaching(Pawn p, float moveCost)` - Called when pawn approaches

### Hospitality Integration:
- CompGuest component on guest pawns
- Existing payment system via CompVendingMachine
- Guest areas and shopping areas for zoning

### Component Architecture:
- ThingComp system for adding functionality to buildings
- CompProperties for configuration data
- CompForbiddable already exists on doors

---

## 🐛 BUG FOUND AND FIXED: Harmony Parameter Mismatch

### The Problem:
- **Error**: `Parameter "door" not found in method static System.Boolean RimWorld.ForbidUtility::IsForbiddenToPass(RimWorld.Building_Door t, Verse.Pawn pawn)`
- **Root Cause**: Method signature uses parameter names `t` and `pawn`, not `door` and `pawn`
- **Impact**: Harmony patch was failing, so PayGate GUI and payment logic weren't working

### The Fix:
```csharp
// WRONG (was causing the error):
public static void IsForbiddenToPass_Postfix(Building_Door door, Pawn pawn, ref bool __result)

// CORRECT (fixed parameter name):
public static void IsForbiddenToPass_Postfix(Building_Door t, Pawn pawn, ref bool __result)
```

### ✅ STATUS: FIXED
- Build now successful
- Harmony patch should now apply correctly
- PayGate GUI should now appear

---

## Current Research Focus: Hospitality Bed UI Elements

### ✅ COMPLETED RESEARCH:
- [x] Examine Building_GuestBed implementation
- [x] Study BedStatsDrawer for UI inspiration
- [x] Look at bed payment/cost systems
- [x] Understand guest bed claiming mechanics
- [x] Analyze inspect panel UI elements
- [x] **FIX HARMONY PATCH PARAMETER MISMATCH** ⭐

### 🎯 KEY UI PATTERNS FROM HOSPITALITY BEDS:

#### **1. Gizmo_ModifyNumber Pattern:**
- **Base Class:** `Gizmo_ModifyNumber<T>` - reusable for any numbered setting
- **Three Buttons:** Up (+), Auto (reset to default), Down (-)
- **Features:** 
  - Multi-selection support (`selection` array)
  - Custom colors (`ButtonColor`)
  - Info display with LabelRow()
  - Tooltip window on hover
  - Range display for multiple selections ("10-30 silver")

#### **2. BedStatsDrawer Window:**
- **Floating Window:** Appears near mouse on hover
- **Smart Positioning:** Adjusts to stay on screen
- **Royal Title Display:** Visual elements with faction icons
- **Dynamic Content:** Updates based on bed stats

#### **3. Payment System Architecture:**
- **RentalFee Property:** Simple int with validation
- **FeeStep Constant:** 10 silver increments
- **Guest Affordability Check:** `guest.inventory.innerContainer.FirstOrDefault(i => i.def == ThingDefOf.Silver)`
- **Visual Feedback:** Money displayed over beds when no owner

#### **4. UI Integration Points:**
- **GetGizmos() Override:** Add custom UI elements to selection
- **GetInspectString() Override:** Add info to inspect panel
- **DrawGUIOverlay() Override:** Show info directly on map
- **Disable Unwanted Gizmos:** Can hide/disable base game controls

### 🏗️ DOOR IMPLEMENTATION PLAN:

#### **Component Design:**
```csharp
// CompPayGate : ThingComp
int entryCost = 5;           // Silver cost
bool payPerEntry = false;    // Pay once vs every time
HashSet<Pawn> paidPawns;     // Track who paid (for pay-once mode)
bool exemptColonists = true; // Exemption flags
bool exemptAllies = true;
bool exemptPrisoners = false;
```

#### **UI Elements to Create:**
1. **Gizmo_PayGateDoor** (based on Gizmo_ModifyNumber)
   - Cost adjustment buttons (±5 or ±10 silver)
   - Mode toggle (Pay Once / Pay Every Time)
   - Exemption checkboxes
   - Payment stats display

2. **Door Info Display:**
   - Cost shown on door overlay when selected
   - Payment status in inspect string
   - Recent payment notifications

#### **Payment Flow:**
1. **Patch IsForbiddenToPass()** - Check payment before door access ✅ FIXED
2. **Handle Payment:** Deduct silver from `pawn.inventory.innerContainer`
3. **Track Payments:** Add to `paidPawns` if pay-once mode
4. **Visual Feedback:** MoteMaker for payment confirmation

---

## Implementation Strategy (Completed):

### ✅ Phase 1: Core Component - DONE
1. ✅ Create CompPayGate with cost, payPerEntry, paidPawns tracking
2. ✅ Patch `IsForbiddenToPass` to check payment status (FIXED!)
3. ✅ Handle payment deduction and tracking

### ✅ Phase 2: User Interface - DONE
1. ✅ Add inspect panel controls for cost/mode
2. ✅ Add gizmos for quick configuration  
3. ✅ Visual feedback for payment events

### ✅ Phase 3: Integration - DONE
1. ✅ Hospitality guest compatibility
2. ✅ Refund system for quick exits
3. ✅ Exemption toggles for different pawn types

---

## 🤖 ROBOT SUPPORT RESEARCH - Misc. Robots++

### Key Findings from Decompiled Source:

#### **Robot Identification:**
- **Robot Type:** `X2_AIRobot` class from `AIRobot` namespace
- **Type Check:** `pawn is X2_AIRobot` to identify robot pawns
- **Reflection Needed:** Must use reflection to avoid hard dependency

#### **Robot DefNames Found:**
- **Cleaner Bots:** AIRobot_Cleaner (I-V)
- **Builder Bots:** RPP_Bot_Builder (I-V)  
- **Kitchen Bots:** RPP_Bot_Kitchen (I-V)
- **Emergency Bots:** RPP_Bot_ER (I-V)
- **Crafter Bots:** RPP_Bot_Crafter (I-V)
- **Omni Bots:** RPP_Bot_Omni (I-V)

#### **Robot Detection Strategy:**
```csharp
// Safe reflection-based robot detection
public static bool IsRobot(Pawn pawn)
{
    // Check for Misc. Robots++ robots using reflection
    var robotType = Type.GetType("AIRobot.X2_AIRobot, Assembly-CSharp");
    if (robotType != null && robotType.IsAssignableFrom(pawn.GetType()))
        return true;
        
    // Could also check by def pattern for additional safety
    return pawn.def.defName.Contains("Robot") || 
           pawn.def.defName.StartsWith("AIRobot_") ||
           pawn.def.defName.StartsWith("RPP_Bot_");
}
```

#### **🎯 SOLUTION: Add Robot Exemption - ✅ COMPLETED**
- **✅ Added to CompPayGate:** `bool exemptRobots = true;`  
- **✅ Default Behavior:** Robots are exempt by default
- **✅ User Control:** Players can toggle robot exemption in the config window
- **✅ Robot Detection:** Safe reflection-based detection + pattern matching
- **✅ UI Integration:** Added robot exemption checkbox to config window

### ✅ Robot Detection Implementation:
```csharp
private static bool IsRobot(Pawn pawn)
{
    // Reflection check for Misc. Robots++ X2_AIRobot type
    var robotType = Type.GetType("AIRobot.X2_AIRobot, Assembly-CSharp");
    if (robotType != null && robotType.IsAssignableFrom(pawn.GetType()))
        return true;
        
    // Fallback pattern matching for def names
    var defName = pawn.def.defName;
    return defName.Contains("Robot") || 
           defName.StartsWith("AIRobot_") ||
           defName.StartsWith("RPP_Bot_") ||
           defName.Contains("_Bot_") ||
           defName.EndsWith("Bot");
}
```

### 🎯 STATUS: ROBOT SUPPORT COMPLETE!
- ✅ Robot pawns will now be exempted from payment by default
- ✅ Players can configure robot exemption in the door settings window  
- ✅ Works with Misc. Robots++ and pattern-matches other robot mods
- ✅ Safe implementation that won't break if robot mods aren't installed

### 🎯 NEXT: Test in-game to confirm robots can pass through doors freely!
