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
            var cowMeat = DefDatabase<ThingDef>.GetNamed("Meat_Cow");
            var humanMeat = DefDatabase<ThingDef>.GetNamed("Meat_Human");
            if (Patch_ThingDefGenerator_Meat.CowMeatLabelCached == null ||
                Patch_ThingDefGenerator_Meat.CowMeatDescriptionCached == null ||
                Patch_ThingDefGenerator_Meat.HumanMeatLabelCached == null ||
                Patch_ThingDefGenerator_Meat.HumanMeatDescriptionCached == null)
            {
                Log.Warning(MeatMod.LogPrefix + "cached strings not exists. maybe the patch wasn't executed?");
            }
            else
            {
                // Restore the original label and description, Because some modded races changes its label and description.
                cowMeat.label = Patch_ThingDefGenerator_Meat.CowMeatLabelCached;
                cowMeat.description = Patch_ThingDefGenerator_Meat.CowMeatDescriptionCached;
                humanMeat.label = Patch_ThingDefGenerator_Meat.HumanMeatLabelCached;
                humanMeat.description = Patch_ThingDefGenerator_Meat.HumanMeatDescriptionCached;
            }


            foreach (var thingDef in DefDatabase<ThingDef>.AllDefs)
            {
                if (Patch_ThingDefGenerator_Meat.SourceRacesCached.Contains(thingDef.defName))
                    thingDef.ResolveReferences();
            }

            if (MeatModSettings.DebugMode)
            {
                Patch_ThingDefGenerator_Meat.DebugRecords.Sort();
                Log.Message(string.Join("\n", Patch_ThingDefGenerator_Meat.DebugRecords));
            }
            Patch_ThingDefGenerator_Meat.DebugRecords.Clear();

            MeatModSettings.Init();
        }
    }
}
