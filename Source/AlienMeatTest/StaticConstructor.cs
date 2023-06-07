using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AlienMeatTest
{
    [StaticConstructorOnStartup]
    public static class StaticConstructor
    {
        static StaticConstructor()
        {
            // Preserve the original label (raw meat)
            DefDatabase<ThingDef>.GetNamed("Meat_Cow").label = Patch_ThingDefGenerator_Meat.cowMeatLabel;

            if (MeatModSettings.DebugMode)
            {
                Patch_ThingDefGenerator_Meat.Records.Sort();
                Log.Message(string.Join("\n", Patch_ThingDefGenerator_Meat.Records));
            }
            Patch_ThingDefGenerator_Meat.Records = null;

            
        }
    }
}
