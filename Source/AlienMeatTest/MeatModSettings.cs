using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Verse;
using RimWorld;
using UnityEngine;

namespace AlienMeatTest
{
    public class MeatModSettings : ModSettings
    {
        //public static bool OptimizationAnimalMeat = true;
        //public static bool OptimizationAlienMeat = false;
        //public static bool OptimizationFishMeat = false;
        public static bool MeatSmallTexture = false;
        public static bool DebugMode = false;

        public override void ExposeData()
        {
            //Scribe_Values.Look(ref OptimizationAnimalMeat, "OM_animal", true);
            //Scribe_Values.Look(ref OptimizationAlienMeat, "OM_alien", false);
            //Scribe_Values.Look(ref OptimizationFishMeat, "OM_fish", false);
            Scribe_Values.Look(ref MeatSmallTexture, "OM_smallTexture", false);
            Scribe_Values.Look(ref DebugMode, "OM_debugMode", false);
            base.ExposeData();
        }

        public void DoWindowContents(Rect inRect)
        {
            Listing_Standard ls = new Listing_Standard();
            ls.Begin(inRect);
            //ls.Label("OM_restart_required".Translate());
            var cachedMeatSmallTexture = MeatSmallTexture;
            ls.CheckboxLabeled("OM_smallTexture_setting".Translate(), ref MeatSmallTexture);
            if (cachedMeatSmallTexture != MeatSmallTexture)
            {
                DefDatabase<ThingDef>.GetNamed("Meat_Cow").graphicData.
                        texPath = MeatSmallTexture
                        ? "Things/Item/Resource/MeatFoodRaw/Meat_Small"
                        : "Things/Item/Resource/MeatFoodRaw/Meat_Big";
            }
            //ls.CheckboxLabeled("OM_animal_setting".Translate(), ref OptimizationAnimalMeat);
            //ls.CheckboxLabeled("OM_alien_setting".Translate(), ref OptimizationAlienMeat);
            //if (Compatibility.VFECompatibility.Detect())
            //{
            //    ls.CheckboxLabeled("OM_fish_setting".Translate(), ref OptimizationFishMeat);
            //}
            ls.CheckboxLabeled("OM_debug_setting".Translate(), ref DebugMode);


            ls.End();
        }

    }
}
