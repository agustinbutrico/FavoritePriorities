using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace FavoritePriorities
{
    [BepInPlugin("com.agustinbutrico.FavoritePriorities", "FavoritePriorities", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        // Instancia estática pública
        public static Plugin Instance { get; private set; }
        // Log
        public static BepInEx.Logging.ManualLogSource Log { get; private set; }

        // Array para guardar tus 5 presets
        private ConfigEntry<string>[] presets = new ConfigEntry<string>[5];

        // Exponemos los presets mediante una propiedad de sólo lectura
        public ConfigEntry<string>[] Presets => presets;

        private void Awake()
        {
            // 1. Guardar la referencia a esta instancia
            Instance = this;
            Log = base.Logger;

            // 2. Valores por defecto personalizados para cada preset
            string[] defaultPresets = new string[]
            {
                "NearDeath,MostHealth,Progress",
                "MostArmor,LeastShield,Slowest",
                "Fastest,MostArmor,LeastShield",
                "Fastest,MostShield,Progress",
                "MostHealth,MostArmor,MostShield"
            };

            // 3. Registrar presets en el cfg usando los valores por defecto
            for (int i = 0; i < presets.Length; i++)
            {
                presets[i] = Config.Bind(
                    section: "FavoritePriorities",                   // sección en el config
                    key: $"Preset{i + 1}",                           // clave: Preset1 .. Preset5
                    defaultValue: defaultPresets[i],                 // valor por defecto específico
                    description: $"Combinación de prioridades para el botón {i + 1}" // descripción
                );
            }

            // 4. Crear e instalar los parches Harmony
            var harmony = new Harmony("com.agustinbutrico.FavoritePriorities");
            harmony.PatchAll();

            Logger.LogInfo("FavoritePriorities cargado correctamente.");
        }
    }
}
