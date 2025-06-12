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
    /// <summary>
    /// This patch addes a warning message, when users load a existing save that doesn't loaded this mod.
    /// </summary>
    [HarmonyPatch(typeof(ScribeMetaHeaderUtility)), HarmonyPatch("TryCreateDialogsForVersionMismatchWarnings")]
    public static class Patch_ScribeMetaHeaderUtility
    {
        public static void Postfix(ref bool __result)
        {
            if (__result == false)
                return;
            if (!ScribeMetaHeaderUtility.loadedModNamesList.Contains("Optimization: Meats - C# Edition"))
            {
                Messages.Message("OM_warning_load_existing_saves".Translate(), MessageTypeDefOf.CautionInput);
            }
        }

    }
}
