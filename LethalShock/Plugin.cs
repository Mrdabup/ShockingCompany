﻿using BepInEx;
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
        private const string modGUID = "MrdTika.ShockingCompany";
        private const string modName = "ShockingCompany";

        private const string modVersion = "0.2.0";


        private readonly Harmony harmony = new Harmony(modGUID);
        private static LethalShockBase Instance;
        private const string Name = "ShockingCompany_CMD";

        public int ShockerMode;
        public int Intensity;
        public int Duration;
        private static ConfigEntry<string> Username;
        private static ConfigEntry<string> ApiKey;
        private static ConfigEntry<string> Code;

        internal static new ConfigFile Config { get; set; }
        public new static BepInEx.Logging.ManualLogSource Logger { get; private set; }
        private int previousHealth = -1;

        private async void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                Logger = base.Logger;
            }

            Config = new ConfigFile(Paths.ConfigPath + "\\ShockingCompany.cfg", true);
            Username = Config.Bind<string>("Settings", "Username", "JohnDoe", "Your username");
            ApiKey = Config.Bind<string>("Settings", "ApiKey", "5c678926-d19e-4f86-42ad-21f5a76126db", "Your API key");
            Code = Config.Bind<string>("Settings", "Code", "17519CD8GAP", "Your share code");

            ShockerMode = 0; // 0 is Shock, 1 is Vibrate, 2 is Beep
            Intensity = 100; // Placeholder numbers, Has to be between 1 and 100
            Duration = 5; // Placeholder numbers, Has to be between 1 and 15

            harmony.PatchAll(typeof(LethalShockBase));
            harmony.PatchAll(typeof(CheckPlayer));

            // Log when attaching to a player object
            Logger.LogInfo("Mod attached to a player object. Hierarchy:");

            // Log the hierarchy of the player object
            LogHierarchy(gameObject);

            Logger.LogInfo("Hello there:");

            Logger.LogInfo(Username.Value);

            await CallApiAsync(100, 3, 2); //Goes like this: Intensity, Duration, ShockerMode

            Logger.LogInfo("Please check if it beeped, if not and there is an error check config files.");
        }

        private void LogHierarchy(GameObject obj, int depth = 0)
        {
            if (obj == null)
            {
                return;
            }

            Logger.LogInfo($"{new string('-', depth)} {obj.name}");

            foreach (Transform child in obj.transform)
            {
                LogHierarchy(child.gameObject, depth + 1);
            }
        }

        private async Task CallApiAsync(int intensity, int duration, int mode)
        {
            using (HttpClient client = new HttpClient())
            {
                string jsonPayload =
                    $"{{\"Username\":\"{Username.Value}\",\"Name\":\"{Name}\",\"Code\":\"{Code.Value}\",\"Intensity\":\"{intensity}\",\"Duration\":\"{duration}\",\"Apikey\":\"{ApiKey.Value}\",\"Op\":\"{mode}\"}}";
                StringContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                try
                {
                    HttpResponseMessage response =
                        await client.PostAsync("https://do.pishock.com/api/apioperate", content);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();
                        Logger.LogInfo($"API call successful. Response: {responseBody}");
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
            private static int storedDamageNumber = 0;

            [HarmonyPostfix]
            [HarmonyPatch("DamagePlayer")]
            static void DamagePlayerPostfix(PlayerControllerB __instance, int damageNumber)
            {
                    int currentHealth = __instance.health;

                    storedDamageNumber = damageNumber;

                    Logger.LogInfo($"Lol is this our script? {damageNumber} damage: {currentHealth}");

                    Logger.LogInfo($"Health Difference: {damageNumber}");

                    if (damageNumber != 0 || Instance.previousHealth == -1)
                    {
                        Instance.Intensity = damageNumber;

                        try
                        {
                            Instance.CallApiAsync(damageNumber, 1, 0); // Uncomment when ready to call the API
                            Logger.LogInfo($"Player Zapped with value {damageNumber}");
                        }
                        catch (Exception ex)
                        {
                            Logger.LogInfo($"Error in CallApiAsync: {ex.Message}");
                        }
                    }
                Instance.previousHealth = currentHealth; // Update previous health for the instance
            }

            [HarmonyPostfix]
            [HarmonyPatch("KillPlayer")]
            static void KillPlayerPostFix(PlayerControllerB __instance)
            {
                if(storedDamageNumber != 0 )
                {
                    Logger.LogError("Cannot double shock for user safety!");
                }
                else
                {
                    Logger.LogInfo("Player Killed by Unkown Cause!");
                    Instance.CallApiAsync(100, 1, 0); // Uncomment when ready to call the API
                    Logger.LogInfo($"Player Zapped with no mercy, player was killed");
                }
                if(storedDamageNumber != 0 || Instance.previousHealth == -1) // If the Damage number is greater than 0 & previousHealth is -1 of that value then execute
                {
                    Logger.LogInfo("Player Killed by Unkown Cause!");
                    Instance.CallApiAsync(100, 1, 0); // Uncomment when ready to call the API
                    Logger.LogInfo($"Player Zapped with no mercy, player was killed");
                }
            }

            static bool IsPlayerValid(PlayerControllerB player)
            {
                // Check if the GameObject is a child of PlayersContainer
                if (IsChildOfPlayersContainer(player.gameObject))
                {
                    return false; // Skip processing for instances in PlayersContainer
                }

                NetworkObject networkObject = player.gameObject.GetComponent<NetworkObject>();

                // Check if the player has the ownership
                return networkObject != null && networkObject.IsOwner;
            }

            // Helper method to check if the GameObject is a child of PlayersContainer
            static bool IsChildOfPlayersContainer(GameObject gameObject)
            {
                Transform parent = gameObject.transform.parent;
                while (parent != null)
                {
                    if (parent.name == "PlayersContainer")
                    {
                        return true; // Found PlayersContainer in the hierarchy
                    }

                    parent = parent.parent;
                }

                return false; // PlayersContainer not found in the hierarchy
            }
        }
    }
}