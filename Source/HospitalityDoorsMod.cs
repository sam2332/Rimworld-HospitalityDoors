using HarmonyLib;
using Verse;

namespace HospitalityDoors
{
    [StaticConstructorOnStartup]
    public static class HospitalityDoorsMod
    {
        static HospitalityDoorsMod()
        {
            var harmony = new Harmony("com.programlerlily.hospitalitydoors");
            harmony.PatchAll();
            
            Log.Message("[Hospitality Doors] Mod initialized successfully!");
        }
    }
}
