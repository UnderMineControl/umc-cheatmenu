using System;
using System.Collections.Generic;
using System.Linq;
using Thor;
using UnityEngine;

namespace UnderMineControl.CheatMenu
{
    using API;
    using API.MenuItems;

    public static class CheatMenuExtensions
    {
        private static System.Random _rnd = new System.Random();

        public static void ToggleDoors(this Mod mod, IMenuCheckBox checkbox)
        {
            try
            {
                var room = mod?.GameInstance?.Simulation?.Zone?.CurrentRoom;
                if (room == null)
                    return;

                if (room.DoorState == Room.DoorStateType.Open)
                {
                    room.CloseDoors();
                    checkbox.Value = false;
                }
                else
                {
                    room.OpenDoors();
                    checkbox.Value = true;
                }
            }
            catch (Exception ex)
            {
                mod.Logger.Error("Error ocurred while attempting to open doors: " + ex);
            }
        }

        public static void MakeGod(this Mod mod, IMenuCheckBox checkbox)
        {
            try
            {
                if (mod.Player == null)
                    return;

                mod.Player.Invulnerable = !mod.Player.Invulnerable;
                checkbox.Value = mod.Player.Invulnerable;
            }
            catch (Exception ex)
            {
                mod.Logger.Error("Error occurred while toggling player invulnerability: " + ex);
            }
        }

        public static void GiveBlessing(this Mod mod)
        {
            try
            {
                if (mod.Player == null)
                    return;

                mod.Player.AddRandomBlessing();
            }
            catch (Exception ex)
            {
                mod.Logger.Error("Error while giving player a blessing: " + ex);
            }
        }

        public static void GiveCurse(this Mod mod)
        {
            try
            {
                if (mod.Player == null)
                    return;

                mod.Player.AddRandomCurse(Thor.HealthExt.CurseType.Major);
            }
            catch (Exception ex)
            {
                mod.Logger.Error("Error while giving player a curse: " + ex);
            }
        }

        public static void RemoveCurse(this Mod mod)
        {
            try
            {
                if (mod.Player == null)
                    return;

                mod.Player.RemoveRandomCurse(out _);
                mod.Player.RemoveRandomCurse(out _, Thor.HealthExt.CurseType.Major);
            }
            catch (Exception ex)
            {
                mod.Logger.Error("Error while giving player a curse: " + ex);
            }
        }

        public static void SetHealth(this Mod mod, string strHealth, bool max = true)
        {
            try
            {
                if (!int.TryParse(strHealth, out int health))
                    return;

                if (health <= 0)
                    health = 1;

                if (max)
                    mod.Player.MaxHP = health;
                else 
                    mod.Player.CurrentHP = health;
            }
            catch (Exception ex)
            {
                mod.Logger.Error("Error while setting bombs: " + ex);
            }
        }

        public static void SetBombs(this Mod mod, string strBombs)
        {
            try
            {
                if (!int.TryParse(strBombs, out int bombs))
                    return;

                if (bombs > 99)
                    bombs = 99;
                if (bombs < 0)
                    bombs = 0;

                mod.Player.Bombs = bombs;
            }
            catch (Exception ex)
            {
                mod.Logger.Error("Error while setting bombs: " + ex);
            }
        }

        public static void SetKeys(this Mod mod, string strKeys)
        {
            try
            {
                if (!int.TryParse(strKeys, out int keys))
                    return;

                if (keys > 99)
                    keys = 99;
                if (keys < 0)
                    keys = 0;

                mod.Player.Keys = keys;
            }
            catch (Exception ex)
            {
                mod.Logger.Error("Error while setting keys: " + ex);
            }
        }

        public static void SetGold(this Mod mod, string strgold)
        {
            try
            {
                if (!int.TryParse(strgold, out int gold))
                    return;

                if (gold < 0)
                    gold = 0;

                mod.Player.Gold = gold;
            }
            catch (Exception ex)
            {
                mod.Logger.Error("Error while setting gold: " + ex);
            }
        }

        public static void SetThorium(this Mod mod, string strThorium)
        {
            try
            {
                if (!int.TryParse(strThorium, out int thorium))
                    return;

                if (thorium > 999)
                    thorium = 999;
                if (thorium < 0)
                    thorium = 0;

                mod.Player.Thorium = thorium;
            }
            catch (Exception ex)
            {
                mod.Logger.Error("Error while setting thorium: " + ex);
            }
        }

        public static void SpawnRelic(this Mod mod)
        {
            try
            {
                var items = new List<ItemData>();
                InventoryExt.GetItems(Game.Instance.Simulation.Avatars[0], items, true);

                var entities = Game.Instance
                                   .Simulation
                                   .Entities
                                   .Entities
                                   .Select(t => t.GetExtension<ItemExt>())
                                   .Where(t => t != null)
                                   .Select(t => t.Data?.guid);

                var relics = mod.GameInstance
                                .Data
                                .RelicCollection
                                .Cast<ItemData>()
                                .Where(t => t.IsDiscovered && t.IsUnlocked &&
                                            !items.Any(a => a.guid == t.guid) &&
                                            !entities.Any(a => a == t.guid))
                                .ToArray();

                if (relics == null || relics.Length <= 0)
                {
                    mod.Logger.Warn("No relics exist to spawn!");
                    return;
                }

                var relic = relics[_rnd.Next(0, relics.Length)];

                mod.GameInstance.SpawnRelic(relic);

                mod.Logger.Debug("Spawned random relic");
            }
            catch (Exception ex)
            {
                mod.Logger.Error("Error occurred while spawning relic: " + ex);
            }
        }
    }
}
