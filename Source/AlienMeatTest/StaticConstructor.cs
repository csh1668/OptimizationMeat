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
            // Restore the original label and description, Because some modded races changes its label and description.
            var stringsCached = Patch_ThingDefGenerator_Meat.StringsCached;
            var cowMeat = DefDatabase<ThingDef>.GetNamed("Meat_Cow");
            var humanMeat = DefDatabase<ThingDef>.GetNamed("Meat_Human");
            cowMeat.label = stringsCached[0, 0];
            cowMeat.description = stringsCached[0, 1];
            humanMeat.label = stringsCached[1, 0];
            humanMeat.description = stringsCached[1, 1];


            foreach (var thingDef in DefDatabase<ThingDef>.AllDefs)
            {
                if (Patch_ThingDefGenerator_Meat.SourceRacesCached.Contains(thingDef.defName))
                    thingDef.ResolveReferences();
            }
            Patch_ThingDefGenerator_Meat.SourceRacesCached.Clear();

            if (MeatModSettings.DebugMode)
            {
                Patch_ThingDefGenerator_Meat.Records.Sort();
                Log.Message(string.Join("\n", Patch_ThingDefGenerator_Meat.Records));
            }
            Patch_ThingDefGenerator_Meat.Records.Clear();

            
        }
    }
}
