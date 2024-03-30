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
        internal static List<string> DebugRecords = new List<string>();

        internal static string CowMeatLabelCached = null;
        internal static string CowMeatDescriptionCached = null;
        internal static string HumanMeatLabelCached = null;
        internal static string HumanMeatDescriptionCached = null;

        internal static HashSet<string> SourceRacesCached = new HashSet<string>();
        internal static MeatListDef WhiteList;
        internal static List<string> WhiteListMeatRecord = new List<string>();

        public static IEnumerable<ThingDef> Postfix(IEnumerable<ThingDef> values)
        {
            var lst = values.ToList();
            ThingDef cowMeat = null, humanMeat = null, insectMeat = null;

            var defsByName = (Dictionary<string, ThingDef>)AccessTools.Field(typeof(DefDatabase<ThingDef>), "defsByName").GetValue(null);
            WhiteList = DefDatabase<MeatListDef>.GetNamed("WhiteList");
            var meatPolicy = MeatModSettings.MeatPolicy;
            foreach (var thingDef in lst)
            {
                var s = thingDef.defName;
                switch (s)
                {
                    case "Meat_Cow":
                        cowMeat = thingDef;
                        CowMeatLabelCached = cowMeat.label;
                        CowMeatDescriptionCached = cowMeat.description;
                        break;
                    case "Meat_Human":
                        humanMeat = thingDef;
                        HumanMeatLabelCached = humanMeat.label;
                        HumanMeatDescriptionCached = humanMeat.description;
                        break;
                    case "Meat_Megaspider":
                        insectMeat = thingDef;
                        break;
                }
            }

            if (cowMeat == null || humanMeat == null || insectMeat == null)
            {
                Log.Error(MeatMod.LogPrefix +
                          "Cannot find base meat def of (Cow or Human or Insect). Did you removed these meats in some weird way?");
                foreach (var thingDef in lst)
                {
                    yield return thingDef;
                }
                yield break;
            }

            foreach (var thingDef in lst)
            {
                string debugRecord = thingDef.defName;
                if (thingDef.defName == cowMeat.defName || thingDef.defName == humanMeat.defName || thingDef.defName == insectMeat.defName)
                {
                    yield return thingDef;
                }
                else
                {
                    var sourceRace = thingDef.ingestible.sourceDef;
                    debugRecord += $"(from {sourceRace.defName})";
                    if (meatPolicy.TryGetValue(sourceRace.defName, out int policy) && policy > 0)
                    {
                        switch (policy)
                        {
                            case 1:
                                debugRecord += "-> Ignored by policy 1";
                                yield return thingDef;
                                break;
                            case 2:
                                debugRecord += "-> CowMeat by policy 2";
                                sourceRace.race.meatDef = cowMeat;
                                defsByName.Add(thingDef.defName, cowMeat);
                                break;
                            case 3:
                                debugRecord += "-> HumanMeat by policy 3";
                                sourceRace.race.meatDef = humanMeat;
                                defsByName.Add(thingDef.defName, humanMeat);
                                break;
                            case 4:
                                debugRecord += "-> InsectMeat by policy 4";
                                sourceRace.race.meatDef = insectMeat;
                                defsByName.Add(thingDef.defName, insectMeat);
                                break;
                        }
                    }
                    else if (WhiteList.meats.Contains(thingDef.defName))
                    {
                        debugRecord += "-> WhiteListed by meats";
                        WhiteListMeatRecord.Add(sourceRace.defName);
                        yield return thingDef;
                    }
                    else if (WhiteList.races.Contains(sourceRace.defName))
                    {
                        debugRecord += "-> WhiteListed by races";
                        yield return thingDef;
                    }
                    else if (sourceRace.race.Humanlike)
                    {
                        debugRecord += "-> HumanMeat";
                        sourceRace.race.meatDef = humanMeat;
                        defsByName.Add(thingDef.defName, humanMeat);
                    }
                    else if (sourceRace.race.FleshType == FleshTypeDefOf.Insectoid)
                    {
                        debugRecord += "-> InsectMeat";
                        sourceRace.race.meatDef = insectMeat;
                        defsByName.Add(thingDef.defName, insectMeat);
                    }
                    else
                    {
                        debugRecord += "-> CowMeat";
                        sourceRace.race.meatDef = cowMeat;
                        defsByName.Add(thingDef.defName, cowMeat);
                    }
                    SourceRacesCached.Add(sourceRace.defName);

                    DebugRecords.Add(debugRecord);
                }
            }
            cowMeat.ResolveReferences();
            insectMeat.ResolveReferences();
            humanMeat.ResolveReferences();
        }
    }
}
