using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AlienMeatTest
{
    /*
     * TODO:
     * 다른 모드와의 호환성 검사
     * ㄴ 바익 문제 없음
     * 바익 물고기 연동 기능 추가 필요
     * 직접 플레이 해보고 버그 검수
     * 필요 없는 고기를 완전히 지우는 기능 추가
     */
    [HarmonyPatch(typeof(ThingDefGenerator_Meat)), HarmonyPatch("ImpliedMeatDefs")]
    public class Patch_ThingDefGenerator_Meat
    {
        public static List<string> Records = new List<string>();
        internal static string cowMeatLabel = string.Empty;
        internal static List<string> SourceRacesCached = new List<string>();

        public static IEnumerable<ThingDef> Postfix(IEnumerable<ThingDef> values)
        {
            var lst = values.ToList();
            ThingDef cowMeat = null, humanMeat = null, insectMeat = null;

            var defsByName = (Dictionary<string, ThingDef>)AccessTools.Field(typeof(DefDatabase<ThingDef>), "defsByName").GetValue(null);
            var whiteList = DefDatabase<MeatListDef>.GetNamed("WhiteList");
            foreach (var thingDef in lst)
            {
                var s = thingDef.defName;
                switch (s)
                {
                    case "Meat_Cow":
                        cowMeat = thingDef;
                        cowMeatLabel = cowMeat.label;
                        break;
                    case "Meat_Human":
                        humanMeat = thingDef;
                        break;
                    case "Meat_Megaspider":
                        insectMeat = thingDef;
                        break;
                }
            }

            if (cowMeat == null || humanMeat == null || insectMeat == null)
            {
                foreach (var thingDef in lst)
                {
                    yield return thingDef;
                }
                yield break;
            }

            foreach (var thingDef in lst)
            {
                string record = thingDef.defName;
                if (thingDef.defName == cowMeat.defName || thingDef.defName == humanMeat.defName || thingDef.defName == insectMeat.defName)
                {
                    yield return thingDef;
                }
                else
                {
                    var sourceRace = thingDef.ingestible.sourceDef;
                    record += $"(from {sourceRace.defName})";
                    if (whiteList.meats.Contains(thingDef.defName))
                    {
                        record += "-> WhiteListed by meats";
                        yield return thingDef;
                    }
                    else if (whiteList.races.Contains(sourceRace.defName))
                    {
                        record += "-> WhiteListed by races";
                        yield return thingDef;
                    }
                    else if (sourceRace.race.Humanlike)
                    {
                        record += "-> HumanMeat";
                        sourceRace.race.meatDef = humanMeat;
                        defsByName.Add(thingDef.defName, humanMeat);
                    }
                    else if (sourceRace.race.FleshType == FleshTypeDefOf.Insectoid)
                    {
                        record += "-> InsectMeat";
                        sourceRace.race.meatDef = insectMeat;
                        defsByName.Add(thingDef.defName, insectMeat);
                    }
                    else
                    {
                        record += "-> CowMeat";
                        sourceRace.race.meatDef = cowMeat;
                        defsByName.Add(thingDef.defName, cowMeat);
                    }
                    SourceRacesCached.Add(sourceRace.defName);

                    Records.Add(record);
                }
            }
            cowMeat.ResolveReferences();
            insectMeat.ResolveReferences();
            humanMeat.ResolveReferences();
        }
    }
}
