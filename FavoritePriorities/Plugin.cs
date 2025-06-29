using System;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace FavoritePriorities
{
    [BepInPlugin("AgusBut.FavoritePriorities", "FavoritePriorities", "1.0.5")]
    public class Plugin : BaseUnityPlugin
    {
        public static bool ShowFavoritePanel = true;
        public static Plugin Instance { get; private set; }
        public static BepInEx.Logging.ManualLogSource Log { get; private set; }

        // Array para guardar tus 5 prioridades
        private ConfigEntry<string>[] priorities = new ConfigEntry<string>[5];
        public ConfigEntry<string>[] Priorities => priorities;

        // Keybinds configurables para cada prioridad
        private ConfigEntry<KeyCode>[] priorityKeybinds = new ConfigEntry<KeyCode>[5];
        public ConfigEntry<KeyCode>[] PriorityKeybinds => priorityKeybinds;

        // Keybind configurable para mostrar/ocultar el panel
        private ConfigEntry<KeyCode> togglePanelKey;
        public ConfigEntry<KeyCode> TogglePanelKey => togglePanelKey;

        private void Awake()
        {
            Instance = this;
            Log = base.Logger;

            // Valores por defecto personalizados para cada prioridad
            string[] defaultPriorities = new string[]
            {
                "NearDeath,MostHealth,Progress",
                "MostArmor,LeastShield,Slowest",
                "Fastest,MostArmor,LeastShield",
                "Fastest,MostShield,Progress",
                "MostHealth,MostArmor,MostShield"
            };

            // Configurar keybinds por defecto: 6-0
            KeyCode[] defaultKeys = new KeyCode[]
            {
                KeyCode.Alpha6,
                KeyCode.Alpha7,
                KeyCode.Alpha8,
                KeyCode.Alpha9,
                KeyCode.Alpha0
            };

            for (int i = 0; i < priorities.Length; i++)
            {
                priorities[i] = Config.Bind(
                    "FavoritePriorities",
                    $"Priority{i + 1}",
                    defaultPriorities[i],
                    $"Priority combination for button {i + 1}\nAcceptable values: Progress, NearDeath, MostHealth, MostArmor, MostShield, LeastHealth, LeastArmor, LeastShield, Fastest, Slowest, Marked"
                );
            }

            // Registrar keybinds configurables
            for (int i = 0; i < priorityKeybinds.Length; i++)
            {
                priorityKeybinds[i] = Config.Bind(
                    "FavoritePriorities.Keybinds",
                    $"PriorityKey{i + 1}",
                    defaultKeys[i],
                    $"Key to activate priority {i + 1}"
                );
            }

            // Registrar keybind para mostrar/ocultar panel
            togglePanelKey = Config.Bind(
                "FavoritePriorities.Keybinds",
                "TogglePanelKey",
                KeyCode.Tab,
                "Key to show/hide the priority panel"
            );

            // Crear e instalar los parches Harmony
            var harmony = new Harmony("AgusBut.FavoritePriorities");
            harmony.PatchAll();

            Logger.LogInfo("Loading [FavoritePriorities 1.0.5]");
        }
    }
}
