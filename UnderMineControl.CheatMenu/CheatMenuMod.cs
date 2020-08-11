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

        private Dictionary<string, string> _spawners = new Dictionary<string, string>();
        private Dictionary<string, IMenuLabel> _spawnerLabels = new Dictionary<string, IMenuLabel>();

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
                    .AddButton("Print All Entities", (c) => PrintEntities());

            SetupSpawners();
        }

        private void SetupSpawners()
        {
            Spawner(GameInstance.Data.EnemyCollection, "Enemy");
            Spawner(GameInstance.Data.RelicCollection, "Relic");
            Spawner(GameInstance.Data.PotionCollection, "Potion");
        }

        private void Spawner(DataObjectCollection collection, string type)
        {
            _spawners.Add(type, "");
            MenuRenderer.AddTextBox($"{type} Name", (t, c) => _spawners[type] = t)
                        .AddButton($"Spawn {type}", (c) => Spawn(type, collection))
                        .AddLabel("", out IMenuLabel label);
            _spawnerLabels.Add(type, label);
        }

        private void Spawn(string type, DataObjectCollection collection)
        {
            var label = _spawnerLabels[type];
            var search = _spawners[type];

            Logger.Debug($"Attempting to find {search} ({type})");

            Spawnable(collection, search, label, type);
        }

        private void Spawnable(DataObjectCollection collection, string search, IMenuLabel label, string type)
        {
            try
            {
                search = search.ToLower().Trim();
                label.Text = "";
                DataObject data;

                if (int.TryParse(search, out int index))
                {
                    Logger.Debug("ID found: " + index);
                    data = GetByIndex(collection, index, label, type);
                    if (data == null)
                        return;
                }
                else
                {
                    Logger.Debug("Search text found: " + search);
                    data = GetByNameOrGuid(collection, search, label, type);
                    if (data == null)
                        return;
                }

                Entity result = null;
                switch (data)
                {
                    case ItemData item:
                        result = GameInstance.SpawnRelic(item);
                        break;
                    case EntityData enemy:
                        result = GameInstance.SpawnEnemy(enemy);
                        break;
                    case AchievementData achievement:
                        GameInstance.Game.AchievementManager.SetCompleted(achievement, !achievement.Completed);
                        label.Text = $"{achievement.name} has been set to: {achievement.Completed}";
                        return;
                    default:
                        label.Text = $"Invalid object! Cannot spawn a {data.name} ({data.GetType().Name})!";
                        return;
                }

                if (result == null)
                {
                    label.Text = $"{type} couldn't spawn!";
                    return;
                }

            }
            catch (Exception ex)
            {
                Logger.Error($"Error occurred when spawning {search} ({type}): {ex}");
                label.Text = "Something went wrong";
            }
        }

        private DataObject GetByIndex(DataObjectCollection collection, int index, IMenuLabel label, string type)
        {
            index -= 1;

            if (collection.Count <= index || index < 0)
            {
                label.Text = $"Invalid {type} number! It has to be between 1 and {collection.Count}";
                return null;
            }

            return collection[index];
        }

        private DataObject GetByNameOrGuid(DataObjectCollection collection, string search, IMenuLabel label, string type)
        {
            var entity = collection.FirstOrDefault(t =>
                                        t.guid.ToLower() == search.ToLower() ||
                                        t.name.ToLower() == search.ToLower());

            if (entity == null)
            {
                var suggestion = GetSuggestions(collection, search);
                label.Text = suggestion == null ? $"Couldn't find a {type} by that name or guid!" : $"Did you mean: {suggestion}";
                return null;
            }

            return entity;
        }

        private string GetSuggestions(DataObjectCollection collection, string search)
        {
            var entities = collection.Where(t => t.name.ToLower().Contains(search.ToLower()))
                                     .ToArray();

            if (entities == null || entities.Length <= 0)
                return null;

            var suggestion = string.Join(", ", entities.Select(t => t.name));

            return suggestion.Length > 64 ? suggestion.Substring(0, 64) + "..." : suggestion;
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
