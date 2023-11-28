using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LethalShock.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class CheckPlayer
    {
        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        static void healthCheck(ref int ___health)
        {

        }
    }
}
