using HarmonyLib;
  
using System.IO;
using System.Linq;
using BepInEx;
using HarmonyLib;

namespace RestrictiveHammer
{
    [HarmonyPatch]
    public class Server
    {
        /// <summary>
        ///     All requests, put in order to the corresponding events. These are "sent" to the server
        /// </summary>
        public static void RPC_RequestRestrictiveHammerAdminSync(long sender, ZPackage pkg)
        {
            var peer = ZNet.instance.GetPeer(sender);
            if (peer != null)
            {
                // grab steam ID
                var str = peer.m_rpc.m_socket.GetHostName();

                if (!ZNet.instance.m_adminList.Contains(str)) return;
                RestrictiveHammer.RHLogger.LogInfo("Admin found: " + str);
                ZRoutedRpc.instance.InvokeRoutedRPC(sender, "EventRestrictiveHammerAdminSync", pkg);
            }
        }


        /// <summary>
        ///     All events, put in order to the corresponding requests. These are "received" from the server
        ///     put logic here that you want to happen on the client AFTER getting the information from the server.
        /// </summary>
        public static void RPC_EventRestrictiveHammerAdminSync(long sender, ZPackage pkg)
        {
        }
    }
}