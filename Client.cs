using HarmonyLib;

namespace RestrictiveHammer
{
    [HarmonyPatch]
    public class Client
    {
        /// <summary>
        ///     All requests, put in order to the corresponding events. These are "sent" to the server
        /// </summary>
        public static void RPC_RequestRestrictiveHammerAdminSync(long sender, ZPackage pkg)
        {
        }


        /// <summary>
        ///     All events, put in order to the corresponding requests. These are "received" from the server
        ///     put logic here that you want to happen on the client AFTER getting the information from the server.
        /// </summary>
        public static void RPC_EventRestrictiveHammerAdminSync(long sender, ZPackage pkg)
        {
            RestrictiveHammer.RHLogger.LogInfo("ADMIN");
            RestrictiveHammer._isAdmin = true;
        }
    }
}