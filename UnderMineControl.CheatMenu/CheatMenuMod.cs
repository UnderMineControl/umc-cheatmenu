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

    public class CheatMenuMod : Mod
    {
        private Dictionary<KeyCode[], Action> _cheatOptions;
        private string Desktop => Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

        public override void Initialize()
        {
            Logger.Debug("Cheat Menu Mod is intializing...");
            _cheatOptions = new Dictionary<KeyCode[], Action>
            {
                [new[] { KeyCode.Alpha1, KeyCode.F1 }] = () => Logger.Debug("Alpha 1 + F1 pressed"),
                [new[] { KeyCode.F1 }] = OpenDoors,
                [new[] { KeyCode.F2 }] = CloseDoors,
                [new[] { KeyCode.F3 }] = () => Player.Invulnerable = !Player.Invulnerable,
                [new[] { KeyCode.F4 }] = () => Player.MaxHP = Player.CurrentHP = 1500,
                [new[] { KeyCode.F5 }] = () => Player.AddRandomBlessing(),
                [new[] { KeyCode.F6 }] = () => { Player.RemoveRandomCurse(out _); Player.RemoveRandomCurse(out _, HealthExt.CurseType.Major); },
                [new[] { KeyCode.F7 }] = () => Player.Bombs = Player.Keys = Player.Gold = Player.Thorium = 10000,
                [new[] { KeyCode.F8 }] = () => Player.Gold *= 2,
                [new[] { KeyCode.F9 }] = Print
            };
            Events.OnGameUpdated += OnGameUpdated;
        }

        private void OnGameUpdated(object sender, IGame e)
        {
            var ops = _cheatOptions.OrderByDescending(t => t.Key.Length);
            foreach (var op in ops)
            {
                if (!AllHeld(op.Key))
                    continue;

                op.Value();
                break;
            }
        }

        private bool AllHeld(params KeyCode[] keys)
        {
            foreach (var key in keys)
                if (!GameInstance.KeyDown(key))
                    return false;

            return true;
        }

        private void Print()
        {
            PrintIds();
            PrintEffects();
            PrintEnemies();
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

        private void OpenDoors()
        {
            GameInstance.Simulation.Zone.CurrentRoom.OpenDoors();
            Logger.Debug("Doors opened");
        }

        private void CloseDoors()
        {
            GameInstance.Simulation.Zone.CurrentRoom.CloseDoors();
            Logger.Debug("Doors closed");
        }
    }
}
