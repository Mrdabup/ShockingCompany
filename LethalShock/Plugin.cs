using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

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

        private static ConfigEntry<string> Username;
        private static ConfigEntry<string> ApiKey;
        private static ConfigEntry<string> Code;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            // Bind configuration values using separate ConfigEntry instances
            Username = Config.Bind("Settings", "Username", "JohnDoe", "Your username");
            ApiKey = Config.Bind("Settings", "ApiKey", "5c678926-d19e-4f86-42ad-21f5a76126db", "Your API key");
            Code = Config.Bind("Settings", "Code", "17519CD8GAP", "Your share code");

            harmony.PatchAll(typeof(LethalShockBase));
            // Commenting the line below assuming CheckPlayer is not defined in this script.
            // harmony.PatchAll(typeof(CheckPlayer));
            Logger.LogInfo("If you see this, hello!");

            
            //CallApiAsync().Wait();
        }

        private async Task CallApiAsync()
        {
            using (HttpClient client = new HttpClient())
            {
                // Prepare the JSON payload
                string jsonPayload = $"{{\"Username\":\"{Username.Value}\",\"Name\":\"TG_Bot_Script\",\"Code\":\"{Code.Value}\",\"Intensity\":\"6\",\"Duration\":\"1\",\"Apikey\":\"{ApiKey.Value}\",\"Op\":\"0\"}}";

                // Create a StringContent with the JSON payload and set content type
                StringContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                try
                {
                    // Send the POST request
                    HttpResponseMessage response = await client.PostAsync("https://do.pishock.com/api/apioperate", content);

                    // Check if the request was successful 
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
    }
}
