using System;
using HarmonyLib;
namespace RestrictiveHammer
{
    [HarmonyPatch]
    public class GamePatch
    {
        [HarmonyPatch(typeof(Game), nameof(Game.Start))]
        [HarmonyPrefix]
        public static void Prefix()
        {
            if (!ZNet.m_isServer) return;
            ZRoutedRpc.instance.Register("RequestRestrictiveHammerAdminSync",
                new Action<long, ZPackage>(Server.RPC_RequestRestrictiveHammerAdminSync));
            ZRoutedRpc.instance.Register("EventRestrictiveHammerAdminSync",
                new Action<long, ZPackage>(Server.RPC_EventRestrictiveHammerAdminSync));
        }
        
        [HarmonyPatch(typeof(Game), nameof(Game.Start))]
        [HarmonyPrefix]
        public static void GameStart_Prefix()
        {
            if (ZNet.m_isServer) return;
            RestrictiveHammer.RHLogger.LogInfo("Registering ZRPCs");
            //admin requests
            ZRoutedRpc.instance.Register("RequestRestrictiveHammerAdminSync",
                new Action<long, ZPackage>(Client.RPC_RequestRestrictiveHammerAdminSync));
            ZRoutedRpc.instance.Register("EventRestrictiveHammerAdminSync",
                new Action<long, ZPackage>(Client.RPC_EventRestrictiveHammerAdminSync));
        }
        
        [HarmonyPatch(typeof (Game), nameof(Game._RequestRespawn))]
        private class FetchAdmins
        {
            [HarmonyPriority(0)]
            private static void Postfix(Player __instance)
            {
                RestrictiveHammer.RHLogger.LogMessage("Requesting Admin");
                if (!RestrictiveHammer._isAdmin)
                    ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.instance.GetServerPeerID(), "RequestRestrictiveHammerAdminSync",
                        new ZPackage());
                
               
            }
        }
    }
}