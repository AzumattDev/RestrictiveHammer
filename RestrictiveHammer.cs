using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using ServerSync;
using UnityEngine;

namespace RestrictiveHammer
{
    [BepInPlugin(PluginId, "RestrictiveHammer", _version)]
    public class RestrictiveHammer : BaseUnityPlugin
    {
        private const string _version = "1.0.0";
        private const string PluginId = "azumatt.RestrictiveHammer";
        public const string Author = "Azumatt";
        private const string PluginName = "RestrictiveHammer";
        private static GameObject? _fabName;
        private static PieceTable? _hammerThings;
        private static List<Piece>? _pieceListHolder;
        private static bool _deletedFromHammer;
        public static bool _isAdmin = false;

        public static readonly ManualLogSource RHLogger = BepInEx.Logging.Logger.CreateLogSource(PluginId);

        private readonly Harmony _harmony = new(PluginId);

        private readonly ConfigSync configSync = new(PluginId)
            { DisplayName = PluginName, CurrentVersion = _version, MinimumRequiredVersion = _version };

        private void Awake()
        {
            try
            {
                serverConfigLocked = config("General", "Force Server Config", true, "Force Server Config");
                configSync.AddLockingConfigEntry(serverConfigLocked);
                
                /* No-Craft */
                _noCraft = config("General", "Not Buildable", false,
                    "Makes the items listed not buildable (Admins are unaffected)");
                
                /* List of prefabs */
                _hammerFabs = config("General", "Prefabs", "fire_pit,bonfire,hearth,wood_stack,wood_fine_stack,wood_core_stack,stone_pile,coal_pile,piece_cookingstation,piece_cookingstation_iron,piece_cauldron,cauldron_ext1_spice,cauldron_ext3_butchertable,cauldron_ext4_pots,piece_oven,piece_workbench,piece_workbench_ext1,piece_workbench_ext2,piece_workbench_ext3,piece_workbench_ext4,piece_stonecutter,piece_artisanstation,forge,forge_ext1,forge_ext2,forge_ext3,forge_ext4,forge_ext5,forge_ext6,smelter,blastfurnace,charcoal_kiln,windmill,piece_spinningwheel,wood_floor_1x1,wood_floor,wood_stair,wood_stepladder,wood_wall_quarter,wood_wall_half,woodwall,wood_wall_roof,wood_wall_roof_upsidedown,wood_wall_roof_top,wood_wall_roof_45,wood_wall_roof_45_upsidedown,wood_wall_roof_top_45,wood_roof,wood_roof_top,wood_roof_ocorner,wood_roof_icorner,wood_roof_45,wood_roof_top_45,wood_roof_ocorner_45,wood_roof_icorner_45,wood_pole,wood_pole2,wood_beam_1,wood_beam,wood_beam_26,wood_beam_45,wood_dragon1,wood_door,wood_gate,darkwood_gate,wood_window,wood_pole_log,wood_pole_log_4,wood_wall_log,wood_wall_log_4x0.5,wood_log_26,wood_log_45,darkwood_roof,darkwood_roof_top,darkwood_roof_ocorner,darkwood_roof_icorner,darkwood_roof_45,darkwood_roof_top_45,darkwood_roof_ocorner_45,darkwood_roof_icorner_45,darkwood_pole,darkwood_pole4,darkwood_beam,darkwood_beam4x4,darkwood_decowall,darkwood_arch,darkwood_raven,darkwood_wolf,wood_fence,stake_wall,piece_sharpstakes,stone_wall_1x1,stone_wall_2x1,stone_wall_4x2,stone_pillar,stone_arch,stone_floor_2x2,stone_stair,iron_floor_1x1,iron_floor_2x2,iron_wall_1x1,iron_wall_2x2,woodiron_pole,woodiron_beam,iron_grate,bed,piece_bed02,piece_chest_wood,piece_chest,piece_chest_private,piece_chest_blackmetal,piece_chair,piece_chair02,piece_chair03,piece_bench01,piece_logbench01,piece_throne01,piece_throne02,piece_table,piece_table_round,piece_table_oak,piece_walltorch,piece_groundtorch,piece_groundtorch_wood,piece_groundtorch_green,piece_groundtorch_blue,piece_brazierceiling01,portal_wood,guard_stone,Cart,VikingShip,Raft,Karve,itemstand,itemstandh,sign,rug_fur,rug_wolf,rug_deer,piece_banner01,piece_banner02,piece_banner03,piece_banner04,piece_banner05,piece_banner06,piece_banner07,piece_beehive,fermenter,piece_gift1,piece_gift2,piece_gift3,piece_xmastree,piece_maypole,piece_jackoturnip,piece_chest_treasure,treasure_pile,treasure_stack,piece_bathtub,crystal_wall_1x1,piece_cartographytable,incinerator,BetterWard,BetterWard_Type2,BetterWard_Type3,BetterWard_Type4",
                    "List of prefabs that are in the hammer, that you wish to remove from the hammer.", true);
            }
            catch (Exception exception)
            {
                RHLogger.LogError($"Error in config bind {exception}");
            }

            try
            {
                _harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
            catch (Exception ex)
            {
                RHLogger.LogError($"Error in PATCH{ex}");
            }
            
        }

        private void Update()
        {
            try
            {
                if (!Player.m_localPlayer) return;
                if (_noCraft.Value && !_deletedFromHammer)
                    DisablePrefabCraft();
                else
                    ReEnablePrefabCrafting();
            }
            catch (Exception ex)
            {
                RHLogger.LogError($"Error in Update Patch: {ex}");
            }
        }

        private void OnDestroy()
        {
            _harmony.UnpatchSelf();
        }

        private static void DisablePrefabCraft()
        {
            if (_isAdmin) return;
            _hammerThings = ObjectDB.instance.GetItemPrefab("Hammer").GetComponent<ItemDrop>().m_itemData.m_shared
                .m_buildPieces;
            /*foreach (var piece in _hammerThings.m_pieces)
            {
                RHLogger.LogError(piece.gameObject.name);
            }*/
            List<string> fabNames = _hammerFabs.Value.Trim().Split(',').ToList();
            foreach (var fab in fabNames)
            {
                var test =  ZNetScene.instance.GetPrefab(fab).GetComponent<Piece>();
                test.m_enabled = false;
                if(_hammerThings.m_pieces.Contains(test.gameObject))
                    _hammerThings.m_pieces.Remove(test.gameObject);
                //ObjectDB.instance.m_items.Remove(test.gameObject);
                        
            }
            _deletedFromHammer = true;
        }

        private static void ReEnablePrefabCrafting()
        {
            if (!_deletedFromHammer || _noCraft.Value) return;
            List<string> fabNames = _hammerFabs.Value.Trim().Split(',').ToList();
            foreach (var fab in fabNames)
            {
                var test =  ZNetScene.instance.GetPrefab(fab).GetComponent<Piece>();
                test.m_enabled = true;
                if(!_hammerThings.m_pieces.Contains(test.gameObject))
                    _hammerThings.m_pieces.Add(test.gameObject);
                //ObjectDB.instance.m_items.Remove(test.gameObject);

            }
            _deletedFromHammer = false;
        }

        /*[HarmonyPatch(typeof (ZNetScene), nameof(ZNetScene.Awake))]
        private class FetchPieces
        {
            [HarmonyPriority(0)]
            private static void Postfix()
            {
                foreach (GameObject gameObject in ZNetScene.instance.m_prefabs.FindAll((Predicate<GameObject>) (p =>
                {
                    Piece component = p.GetComponent<Piece>();
                    return component != null;
                })))
                {
                    Piece piece = gameObject.GetComponent<Piece>();
                    RHLogger.LogError("Removing better wards");
                    List<string> fabNames = _hammerFabs.Value.Trim().Split(',').ToList();
                    foreach (var fab in fabNames)
                    {
                       var test =  ZNetScene.instance.GetPrefab(fab).GetComponent<Piece>();
                       _pieceListHolder?.Add(test);
                            //test.enabled = false;
                            
                            //ObjectDB.instance.m_items.Remove(test.gameObject);
                        
                    }
                    
                    _noCraft.SettingChanged += (EventHandler) ((config, e) =>
                    {
                        foreach (var piece in _pieceListHolder)
                        {
                        // This doesn't work for some reason
                            RHLogger.LogError("YEP");
                            FindObjectsOfType<Piece>().Where((Func<Piece, bool>) (p => p.m_name == piece.m_name)).ToList().ForEach((Action<Piece>) (p => p.enabled = _noCraft.Value));
                            if(_noCraft.Value) {ObjectDB.instance.m_items.Remove(piece.gameObject);}
                            if(!_noCraft.Value && !ObjectDB.instance.m_items.Contains(piece.gameObject)) {ObjectDB.instance.m_items.Add(piece.gameObject);}
                        }
                        
                    });
                }
            }
        }*/

        #region Configs

        public static ConfigEntry<bool> serverConfigLocked;

        private static ConfigEntry<bool>? _noCraft;
        private static ConfigEntry<string>? _hammerFabs;

        private ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description,
            bool synchronizedSetting = true)
        {
            var configEntry = Config.Bind(group, name, value, description);

            var syncedConfigEntry = configSync.AddConfigEntry(configEntry);
            syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

            return configEntry;
        }

        private ConfigEntry<T> config<T>(string group, string name, T value, string description,
            bool synchronizedSetting = true)
        {
            return config(group, name, value, new ConfigDescription(description), synchronizedSetting);
        }

        private class ConfigurationManagerAttributes
        {
            public bool? Browsable = false;
        }

        #endregion
    }
}