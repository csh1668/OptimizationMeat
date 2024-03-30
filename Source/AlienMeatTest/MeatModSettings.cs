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
        public static bool DebugMode = false;
        /// <summary>
        /// 0 = Default, 1 = No merge (Vanilla), 2 = To raw meat, 3 = To human meat, 4 = To insect meat
        /// </summary>
        public static Dictionary<string, int> MeatPolicy = new Dictionary<string, int>();

        public static List<ThingDef> AllRaces = new List<ThingDef>();
        

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref DebugMode, "OM_debugMode", false);
            Scribe_Collections.Look(ref MeatPolicy, "OM_MeatPolicy", LookMode.Value, LookMode.Value);
            if (MeatPolicy == null)
                MeatPolicy = new Dictionary<string, int>();
        }

        public static void Init()
        {
            AllRaces.Clear();
            AllRaces.AddRange(DefDatabase<ThingDef>.AllDefs.Where(x => x.race != null && !x.IsCorpse && !x.race.IsMechanoid));
            foreach (var sourceRace in AllRaces)
            {
                if (!MeatPolicy.TryGetValue(sourceRace.defName, out var value))
                {
                    MeatPolicy[sourceRace.defName] = 0;
                }
            }
        }

        public void DoWindowContents(Rect inRect)
        {
            var leftRect = inRect.LeftHalf();
            leftRect.width -= 10f;
            var listingLeft = new Listing_Standard();
            listingLeft.Begin(leftRect);
            if (listingLeft.ButtonText("OM_reset_policy".Translate()))
            {
                var keys = MeatPolicy.Keys.ToList();
                foreach (var key in keys)
                {
                    MeatPolicy[key] = 0;
                }
                Messages.Message("OM_reset_done".Translate(), MessageTypeDefOf.PositiveEvent);
            }
            listingLeft.CheckboxLabeled("OM_debug_setting".Translate(), ref DebugMode);
            listingLeft.End();

            var rightRect = inRect.RightHalf();
            rightRect.width -= 10f;
            var listingRight = new Listing_Standard();
            listingRight.Begin(rightRect);
            listingRight.Label("OM_settings_note".Translate());
            listingRight.Label("OM_restart_required".Translate());
            listingRight.End();

            var entryHeight = 40f;

            var matchingRaces = AllRaces.Where(x => Matches(FormmatedString(x), searchKeyword)).ToList();

            var cntEntry = matchingRaces.Count;
            var outRect = new Rect(0f, 120f, inRect.width - 10f, inRect.height - 120f);
            var listRect = new Rect(0f, 0f, outRect.width - 50f, entryHeight * cntEntry);
            var labelRect = new Rect(entryHeight + 10f, 0f, listRect.width / 2 - 100f, entryHeight);
            var div = (listRect.width - (listRect.width / 2 - 100f)) / 5;
            Widgets.Label(new Rect(0, outRect.y - 22f, "OM_search".Translate().GetWidthCached(), 22f), "OM_search".Translate());
            searchKeyword =
                Widgets.TextArea(
                    new Rect("OM_search".Translate().GetWidthCached(), outRect.y - 22f,
                        (outRect.width - labelRect.width - labelRect.x) / 2, 22f), searchKeyword);
            Widgets.Label(new Rect(labelRect.width + labelRect.x - "OM_column_automatic".Translate().GetWidthCached() / 2, outRect.y - 22f, div, 22f), "OM_column_automatic".Translate());
            Widgets.Label(new Rect(labelRect.width + labelRect.x + div - "OM_column_vanilla".Translate().GetWidthCached() / 2, outRect.y - 22f, div, 22f), "OM_column_vanilla".Translate());
            Widgets.Label(new Rect(labelRect.width + labelRect.x + div * 2 - "OM_column_raw_meat".Translate().GetWidthCached() / 2, outRect.y - 22f, div, 22f), "OM_column_raw_meat".Translate());
            Widgets.Label(new Rect(labelRect.width + labelRect.x + div * 3 - "OM_column_human_meat".Translate().GetWidthCached() / 2, outRect.y - 22f, div, 22f), "OM_column_human_meat".Translate());
            Widgets.Label(new Rect(labelRect.width + labelRect.x + div * 4 - "OM_column_insect_meat".Translate().GetWidthCached() / 2, outRect.y - 22f, div, 22f), "OM_column_insect_meat".Translate());
            Widgets.BeginScrollView(outRect, ref scrollbarVector, listRect, true);
            for (int i = 0; i < cntEntry; i++)
            {
                var curRace = AllRaces[i];

                var entryRect = new Rect(0f, i * entryHeight, inRect.width - 60f, entryHeight);
                if (i % 2 == 0)
                {
                    Widgets.DrawLightHighlight(entryRect);
                }

                GUI.BeginGroup(entryRect);
                Widgets.ButtonImage(new Rect(0f, 0f, entryHeight, entryHeight),
                    curRace.uiIcon != null ? curRace.uiIcon : BaseContent.BadTex);
                
                Widgets.Label(labelRect, FormmatedString(curRace));

                if (curRace.race.useMeatFrom != null)
                {
                    var newLabelRect = labelRect;
                    newLabelRect.x += labelRect.width;
                    newLabelRect.width += 50f;
                    Widgets.Label(newLabelRect,
                        $"OM_note_useMeatFrom".Translate(FormmatedString(curRace.race.useMeatFrom)));
                    GUI.EndGroup();
                    continue;
                }
                if (curRace.defName == "Human" || curRace.defName == "Cow" || curRace.defName == "Megaspider")
                {
                    var newLabelRect = labelRect;
                    newLabelRect.x += labelRect.width;
                    newLabelRect.width += 50f;
                    Widgets.Label(newLabelRect,
                        $"OM_note_specialMeat".Translate());
                    GUI.EndGroup();
                    continue;
                }

                if (Patch_ThingDefGenerator_Meat.WhiteList.races.Contains(curRace.defName))
                {
                    var newLabelRect = labelRect;
                    newLabelRect.x += labelRect.width;
                    newLabelRect.width += 50f;
                    Widgets.Label(newLabelRect,
                        $"OM_note_whitelist".Translate());
                    GUI.EndGroup();
                    continue;
                }

                if (Patch_ThingDefGenerator_Meat.WhiteListMeatRecord.Contains(curRace.defName))
                {
                    var newLabelRect = labelRect;
                    newLabelRect.x += labelRect.width;
                    newLabelRect.width += 50f;
                    Widgets.Label(newLabelRect,
                        $"OM_note_whitelist_meat".Translate());
                    GUI.EndGroup();
                    continue;
                }

                var curPolicy = MeatPolicy[curRace.defName];
                if (Widgets.RadioButton(labelRect.width + labelRect.x - Widgets.RadioButtonSize / 2, (entryHeight - Widgets.RadioButtonSize) / 2, curPolicy == 0))
                {
                    MeatPolicy[curRace.defName] = 0;
                }

                if (Widgets.RadioButton(labelRect.width + labelRect.x + div - Widgets.RadioButtonSize / 2, (entryHeight - Widgets.RadioButtonSize) / 2, curPolicy == 1))
                {
                    MeatPolicy[curRace.defName] = 1;
                }

                if (Widgets.RadioButton(labelRect.width + labelRect.x + div * 2 - Widgets.RadioButtonSize / 2, (entryHeight - Widgets.RadioButtonSize) / 2, curPolicy == 2))
                {
                    MeatPolicy[curRace.defName] = 2;
                }

                if (Widgets.RadioButton(labelRect.width + labelRect.x + div * 3 - Widgets.RadioButtonSize / 2 , (entryHeight - Widgets.RadioButtonSize) / 2, curPolicy == 3))
                {
                    MeatPolicy[curRace.defName] = 3;
                }

                if (Widgets.RadioButton(labelRect.width + labelRect.x + div * 4 - Widgets.RadioButtonSize / 2, (entryHeight - Widgets.RadioButtonSize) / 2, curPolicy == 4))
                {
                    MeatPolicy[curRace.defName] = 4;
                }
                GUI.EndGroup();
            }
            Widgets.EndScrollView();
        }

        private string FormmatedString(ThingDef def)
        {
            return $"{def.label}::{def.defName}::{def.modContentPack?.Name}";
        }

        private bool Matches(string str, string token)
        {
            return string.IsNullOrWhiteSpace(token) || str.ToLower().Contains(token.ToLower());
        }
        private Vector2 scrollbarVector = new Vector2(0f, 0f);
        private string searchKeyword = string.Empty;
    }
}
