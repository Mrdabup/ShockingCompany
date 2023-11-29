using GameNetcodeStuff;
using HarmonyLib;

namespace LethalShock.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class CheckPlayer
    {
        private static int previousHealth;
        
        // Assuming you have an instance of YourPluginClass available
        

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        static void healthCheck(ref int ___health)
        {
            // Check if health has decreased
            if (___health < previousHealth)
            {
                // Calculate the difference
                int damageTaken = previousHealth - ___health;

               
                
            }

            // Update the previous health for the next check
            previousHealth = ___health;
        }
    }
}