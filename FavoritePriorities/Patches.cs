using HarmonyLib;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace FavoritePriorities
{
    [HarmonyPatch(typeof(TowerUI), "Start")]
    public static class Patch_TowerUI_Start
    {
        static void Postfix(TowerUI __instance)
        {
            // 1) Obtener el canvas
            var canvas = __instance.transform.Find("Canvas");

            // 2) Crear (o reutilizar) un panel propio para favoritos
            var favPanelTransform = canvas.Find("FavoritePrioritiesPanel");
            if (favPanelTransform == null)
            {
                // Clonar el panel de ejemplo (RightPanel) para mantener el estilo
                var template = canvas.Find("RightPanel");
                if (template == null)
                {
                    Plugin.Log.LogError("[FavoritePriorities] No se encontró RightPanel para clonar.");
                    return;
                }

                var favPanel = UnityEngine.Object.Instantiate(template.gameObject, canvas) as GameObject;
                favPanel.name = "FavoritePrioritiesPanel";
                favPanelTransform = favPanel.transform;

                // Posicionar anclado a la izquierda
                var favRt = favPanel.GetComponent<RectTransform>();
                favRt.anchorMin = new Vector2(0, 0.5f);
                favRt.anchorMax = new Vector2(0, 0.5f);
                favRt.pivot = new Vector2(0, 0.5f);
                favRt.anchoredPosition = new Vector2(50, 0);

                // Fondo amarillo semitransparente
                var img = favPanel.GetComponent<Image>();
                if (img != null)
                    img.color = new Color(1f, 1f, 0f, 0.5f);

                // Añadir layout automático con alineación fully qualified
                var layout = favPanel.AddComponent<HorizontalLayoutGroup>();
                layout.padding = new RectOffset(5, 5, 5, 5);
                layout.spacing = 10;
                layout.childAlignment = UnityEngine.TextAnchor.MiddleCenter;
            }

            // 3) Botón de ejemplo para clonar (HealthButton)
            var sampleBtn = canvas.Find("RightPanel/HealthButton").gameObject;
            var sampleRt = sampleBtn.GetComponent<RectTransform>();

            // 4) Instanciar los botones de presets dentro del panel de favoritos
            for (int i = 0; i < 5; i++)
            {
                var clone = UnityEngine.Object.Instantiate(sampleBtn, favPanelTransform) as GameObject;
                clone.name = $"FavoriteBtn{i + 1}";

                // Ajustar tamaño y pivote igual al botón original
                var rt = clone.GetComponent<RectTransform>();
                rt.sizeDelta = sampleRt.sizeDelta;
                rt.anchorMin = sampleRt.anchorMin;
                rt.anchorMax = sampleRt.anchorMax;
                rt.pivot = sampleRt.pivot;

                // Texto multilínea
                var txt = clone.GetComponentInChildren<Text>();
                txt.text = string.Join("\n", Plugin.Instance.Presets[i].Value.Split(','));

                // Callback al hacer click
                var btn = clone.GetComponent<Button>();
                int idx = i;
                btn.onClick.AddListener(() =>
                {
                    // 1) Accedemos al Tower privado
                    var towerField = AccessTools.Field(typeof(TowerUI), "myTower");
                    var tower = (Tower)towerField.GetValue(__instance);

                    // 2) Leemos el preset
                    var parts = Plugin.Instance.Presets[idx].Value.Split(',');

                    // 3) Aplicamos al array real de prioridades
                    if (parts.Length > 0 && Enum.TryParse(parts[0], out Tower.Priority p0)) tower.priorities[0] = p0;
                    if (parts.Length > 1 && Enum.TryParse(parts[1], out Tower.Priority p1)) tower.priorities[1] = p1;
                    if (parts.Length > 2 && Enum.TryParse(parts[2], out Tower.Priority p2)) tower.priorities[2] = p2;

                    // 4) Refrescamos la UI para que se vean y apliquen de verdad
                    __instance.SetStats(tower);

                    // 5) (Opcional) loguear para depurar
                    Plugin.Log.LogDebug($"[FavoritePriorities] Preset aplicado: {string.Join(",", tower.priorities)}");
                });
            }
        }
    }
}
