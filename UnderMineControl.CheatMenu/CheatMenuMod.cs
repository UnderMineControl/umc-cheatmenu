using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using Thor;
using UnityEngine;

namespace UnderMineControl.CheatMenu
{
    using API;
    using UnderMineControl.API.MenuItems;

    public class CheatMenuMod : Mod
    {
        private string Desktop => Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        private bool _showWindow = false;

        private bool _previousCursor = false;
        private CursorLockMode _previousLockMode = CursorLockMode.None;

        private string _relicName = "";
        private string _enemyName = "";

        private IMenuLabel _enemyLabel;
        private IMenuLabel _relicLabel;
        

        public override void Initialize()
        {
            Logger.Debug("Cheat Menu Mod is intializing...");
            Events.OnGameUpdated += OnGameUpdated;

            MenuRenderer.Text = "Cheat Menu";

            MenuRenderer
                    .SetDefaultSkin()
                    .SetSize(400, 500)
                    //Basic Cheats
                    .AddCheckBox("Toggle Doors", (b, c) => this.ToggleDoors(c))
                    .AddCheckBox("God Mode", (b, c) => this.MakeGod(c))
                    .AddTextBox("Max Health", (t, c) => this.SetHealth(t, true))
                    .AddTextBox("Current Health", (t, c) => this.SetHealth(t, false))
                    .AddTextBox("Bombs", (t, c) => this.SetBombs(t))
                    .AddTextBox("Keys", (t, c) => this.SetKeys(t))
                    .AddTextBox("Gold", (t, c) => this.SetGold(t))
                    .AddTextBox("Thorium", (t, c) => this.SetThorium(t))
                    .AddButton("Give Curse", (c) => this.GiveCurse())
                    .AddButton("Give Blessing", (c) => this.GiveBlessing())
                    .AddButton("Remove Curse", (c) => this.RemoveCurse())
                    .AddButton("Spawn Relic", (c) => CheatMenuExtensions.SpawnRelic(this))
                    //Debug outputs
                    .AddButton("Print equipment", (c) => PrintActiveItems())
                    .AddButton("Print All Entities", (c) => PrintEntities())
                    //Specific item/enemy spawns
                    .AddTextBox("Enemy Name", (t, c) => { _enemyName = t; })
                    .AddButton("Spawn Enemy", (c) => SpawnEnemy())
                    .AddLabel("", out _enemyLabel)
                    
                    .AddTextBox("Relic Name", (t, c) => { _relicName = t; })
                    .AddButton("Spawn Relic", (c) => SpawnRelic())
                    .AddLabel("", out _relicLabel);
        }

        private void SpawnEnemy()
        {
            try
            {
                Logger.Debug(_enemyName);
                _enemyLabel.Text = "";

                var enemy = GameInstance.GetEnemy(_enemyName);
                if (enemy == null)
                {
                    var names = GameInstance.GetEnemyLike(_enemyName).ToArray();
                    if (names.Length <= 0)
                    {
                        _enemyLabel.Text = "Couldn't find an enemy with that name or id!";
                        return;
                    }

                    var strNames = string.Join(", ", names);
                    if (strNames.Length > 64)
                        strNames = strNames.Substring(0, 64) + "...";

                    _enemyLabel.Text = "Did you mean: " + strNames;
                    return;
                }

                var entity = GameInstance.SpawnEnemy(enemy);
                if (entity == null)
                {
                    _enemyLabel.Text = "Enemy couldn't spawn!";
                    return;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error occurred while spawning {_enemyName} (enemy): {ex}");
                _enemyLabel.Text = "Something went wrong!";
            }
        }

        private void SpawnRelic()
        {
            try
            {
                Logger.Debug(_relicName);
                _relicLabel.Text = "";

                var relic = GameInstance.GetRelic(_relicName);
                if (relic == null)
                {
                    _relicLabel.Text = "Couldn't find an relic with that name or id!";
                    return;
                }

                var entity = GameInstance.SpawnRelic(relic);
                if (entity == null)
                {
                    _relicLabel.Text = "Relic couldn't spawn!";
                    return;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error occurred while spawning {_enemyName} (relic): {ex}");
                _relicLabel.Text = "Something went wrong!";
            }
        }

        private void PrintActiveItems()
        {
            var avatars = Game.Instance?.Simulation?.Avatars;
            if (avatars == null || avatars.Count <= 0)
            {
                Logger.Debug("Unable to find any avatars!");
                return;
            }

            var items = new List<ItemData>();
            InventoryExt.GetItems(avatars[0], items, true);

            foreach (var item in items)
            {
                Logger.Debug($"{item.guid} - {item.Hint} - {item.name}");
            }

            foreach(var slot in Player.Inventory.EquipmentSlots)
            {
                var data = (ItemData)slot?.equipment?.Entity?.Data;
                if (data == null)
                    Logger.Debug($"{slot.slot} - empty");
                else
                    Logger.Debug($"{slot.slot} - {data.guid} - {data.Hint} - {data.name}");
            }
        }

        private void PrintEntities()
        {
            foreach(var entity in Game.Instance.Simulation.Entities.Entities)
            {
                var ext = entity.GetExtension<ItemExt>();
                if (ext == null)
                    continue;

                Logger.Debug(ext.Data?.name);
            }
        }

        private void OnGameUpdated(object sender, IGame e)
        {
            if (e.KeyDown(KeyCode.F1))
            {
                _showWindow = !_showWindow;
                Logger.Debug("Changing window mode: " + _showWindow);
                MenuRenderer.Show = _showWindow;
                if (_showWindow)
                {
                    _previousCursor = Cursor.visible;
                    _previousLockMode = Cursor.lockState;
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;
                }
                else
                {
                    Cursor.visible = _previousCursor;
                    Cursor.lockState = _previousLockMode;
                }
            }
        }

        private void Print()
        {
            //PrintIds();
            //PrintEffects();
            //PrintEnemies();
            PrintActiveItems();
        }


        private void PrintIds()
        {
            WriteCsv("Items", GameData.Instance.Items,
                (t) => t.guid,
                (t) => t.name,
                (t) => t.DisplayName.Id,
                (t) => t.DisplayName.Text,
                (t) => t.Description.Text,
                (t) => t.IsBlessing,
                (t) => t.IsCurse,
                (t) => t.IsMinor,
                (t) => t.IsHex,
                (t) => t.IsSpecialDrop,
                (t) => t.IsSpecialDiscovery,
                (t) => t.Audio,
                (t) => t.IsDeprecated,
                (t) => t.IsDefault,
                (t) => t.IsDefaultDiscovered,
                (t) => t.AllowOnAltar,
                (t) => t.Slot,
                (t) => t.Hint,
                (t) => t.Rarity);
        }

        private void PrintEffects()
        {
            WriteCsv("Effects", GameData.Instance.StatusEffects,
                (t) => t.Entity.Guid,
                (t) => t.name,
                (t) => t.IsBlessing,
                (t) => t.IsCurse,
                (t) => t.IsMinor,
                (t) => t.IsHex,
                (t) => t.Level,
                (t) => t.MaxDuration,
                (t) => t.DefaultDuration,
                (t) => t.Sticky,
                (t) => t.StackPolicy,
                (t) => t.Position);
        }

        private void PrintEnemies()
        {
            try
            {
                var vars = Patcher.GetField<List<ZoneData>>(GameData.Instance, "m_zones");
                foreach (var zone in vars)
                {
                    var boss = Patcher.GetField<bool>(zone, "m_isBossZone");
                    if (!boss)
                        continue;

                    foreach (var res in zone.Resources)
                    {
                        Logger.Debug(res.GetType().Name);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error writing out enemy data: " + ex);
            }
        }

        public void WriteCsv<T>(string fileName, IEnumerable<T> data, params Expression<Func<T, object>>[] properties)
        {
            try
            {
                var path = Path.Combine(Desktop, fileName + ".csv");
                Logger.Debug("Starting to write out all " + fileName);
                using (var io = File.Open(path, FileMode.Create))
                using (var sw = new StreamWriter(io))
                {
                    bool writtenHeader = false;
                    foreach (var entry in data)
                    {
                        if (!writtenHeader)
                        {
                            var names = GetHeaderNames(properties).ToArray();
                            WriteCsvLine(names, sw);
                            writtenHeader = true;
                        }

                        var members = new List<object>();
                        foreach (var prop in properties)
                        {
                            var value = prop.Compile()(entry);
                            members.Add(value);
                        }

                        WriteCsvLine(members.ToArray(), sw);
                    }

                    sw.Flush();
                }

                Logger.Debug("Finished writing out all " + fileName);
            }
            catch (Exception e)
            {
                Logger.Error($"Error writing out {fileName} data: {e}");
            }
        }

        private IEnumerable<string> GetHeaderNames<T>(Expression<Func<T, object>>[] properties)
        {
            foreach (var prop in properties)
            {
                if (prop.Body is MemberExpression me)
                {
                    yield return me.Member.Name;
                    continue;
                }

                if (prop.Body is UnaryExpression un)
                {
                    yield return (un.Operand as MemberExpression).Member.Name;
                    continue;
                }

                yield return prop.ReturnType.Name;
            }
        }

        private void WriteCsvLine(object[] data, StreamWriter writer)
        {
            var actData = data.Select(t => CsvEscape(t.ToString()));
            var line = string.Join(",", actData.ToArray());
            writer.WriteLine(line);
        }

        private string CsvEscape(string value)
        {
            var needsEscape = value.Contains("\"") ||
                value.Contains("\r") ||
                value.Contains("\n") ||
                value.Contains(",");

            if (!needsEscape)
                return value;

            value = value.Replace("\"", "\"\"");

            return $"\"{value}\"";
        }
    }
}
