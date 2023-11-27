using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using BepInEx;
using HarmonyLib;

namespace LethalShock
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class LethalShockBase : BaseUnityPlugin
    {
        private const string modGUID = "MrdTika.LethalShock";
        private const string modName = "Lethal Shock";
        private const string modVersion = "inDev 1";

        private readonly Harmony harmony = new Harmony(modGUID);

        private static LethalShockBase Instance;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }
    }
}
