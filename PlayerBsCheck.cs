using System;
using HarmonyLib;
using GameNetcodeStuff;




namespace PlayerBsCheck.Patch
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerBSCheck
    {

    }
}