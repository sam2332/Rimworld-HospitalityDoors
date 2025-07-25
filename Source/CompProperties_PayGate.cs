using Verse;

namespace HospitalityDoors
{
    public class CompProperties_PayGate : CompProperties
    {
        public int defaultCost = 5;
        public bool defaultPayPerEntry = false;
        public bool defaultExemptColonists = true;
        public bool defaultExemptAllies = true;
        public bool defaultExemptPrisoners = false;
        
        public CompProperties_PayGate()
        {
            compClass = typeof(CompPayGate);
        }
    }
}
