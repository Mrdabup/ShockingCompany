using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using Unity.Netcode;
using System;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using GameNetcodeStuff;

namespace LethalShock
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class LethalShockBase : BaseUnityPlugin
    {
        private const string modGUID = "MrdTika.LethalShock";
        private const string modName = "Lethal Shock";
        private const string modVersion = "0.0.1";

        private readonly Harmony harmony = new Harmony(modGUID);
        private static LethalShockBase Instance;
        private const string Name = "Lethal_Shock_CMD";

        public int ShockerMode;
        public int Intensity;
        public int Duration;
        private static ConfigEntry<string> Username;
        private static ConfigEntry<string> ApiKey;
        private static ConfigEntry<string> Code;

        internal static new ConfigFile Config { get; set; }
        public static BepInEx.Logging.ManualLogSource Logger { get; private set; }
        private int previousHealth = -1;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                Logger = base.Logger;
            }

            Config = new ConfigFile(Paths.ConfigPath + "\\MrdTika.LethalShock.cfg", true);
            Username = Config.Bind<string>("Settings", "Username", "JohnDoe", "Your username");
            ApiKey = Config.Bind<string>("Settings", "ApiKey", "5c678926-d19e-4f86-42ad-21f5a76126db", "Your API key");
            Code = Config.Bind<string>("Settings", "Code", "17519CD8GAP", "Your share code");

            ShockerMode = 0; // 0 is Shock, 1 is Vibrate, 2 is Beep
            Intensity = 100; // Placeholder numbers, Has to be between 1 and 100
            Duration = 5; // Placeholder numbers, Has to be between 1 and 15

            harmony.PatchAll(typeof(LethalShockBase));
            harmony.PatchAll(typeof(CheckPlayer));
            Logger.LogInfo("If you see this, hello!");

            Logger.LogInfo(Username.Value);
            Logger.LogInfo(ApiKey.Value);
            Logger.LogInfo(Code.Value);

            //CallApiAsync();
        }

        private async Task CallApiAsync()
        {
            using (HttpClient client = new HttpClient())
            {
                string jsonPayload =
                    $"{{\"Username\":\"{Username.Value}\",\"Name\":\"{Name}\",\"Code\":\"{Code.Value}\",\"Intensity\":\"{Intensity}\",\"Duration\":\"{Duration}\",\"Apikey\":\"{ApiKey.Value}\",\"Op\":\"{ShockerMode}\"}}";
                StringContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                try
                {
                    HttpResponseMessage response =
                        await client.PostAsync("https://do.pishock.com/api/apioperate", content);

                    if (response.IsSuccessStatusCode)
                    {
                        Logger.LogInfo("API call successful");
                    }
                    else
                    {
                        Logger.LogError($"API call failed with status code: {response.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Error: {ex.Message}");
                }
            }
        }

        [HarmonyPatch(typeof(PlayerControllerB))]
        internal class CheckPlayer
        {
            private static int previousHealth = -1;
            private static int frameCounter = 0;

            private static void healthCheck(PlayerControllerB __instance, ref int ___health)
            {
                healthCheck(__instance, ref ___health);
            }

            [HarmonyPatch("Update")]
            [HarmonyPostfix]
            static void healthCheck(PlayerControllerB __instance, ref int ___health, LethalShockBase lethalShockBase)
            {
                // Assuming __instance.gameObject is the player GameObject
                NetworkObject networkObject = __instance.gameObject.GetComponent<NetworkObject>();

                if (networkObject != null && networkObject.IsOwner)
                {
                    frameCounter++;

                    if (frameCounter % 2 == 0)
                    {
                        int healthDifference = ___health - previousHealth;
                           
                        Logger.LogInfo($"Current Health: {___health}, Previous Health: {previousHealth}, Health Difference: {healthDifference}");

                        if (healthDifference != 0)
                        {
                            // Use healthDifference as the shock intensity and call the API here
                            Instance.Intensity = healthDifference;
                            // Instance.CallApiAsync(); Do not uncomment until you are sure its only calling this once then going back idle!!! you will dos the api
                        }
                        if(healthDifference > 2)
                        {
                            lethalShockBase.CallApiAsync();
                        }
                    }

                    previousHealth = ___health; // Update previous health for the instance
                }
            }
        }



    }
}
