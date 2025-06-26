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
            var canvas = __instance.transform.Find("Canvas");
            var prioritiesPanel = canvas.Find("PrioritiesPanel");

            // Botón de ejemplo para clonar (el de Health)
            var sampleBtn = canvas.Find("RightPanel/HealthButton").gameObject;
            var sampleRt = sampleBtn.GetComponent<RectTransform>();

            for (int i = 0; i < 5; i++)
            {
                var clone = UnityEngine.Object.Instantiate(sampleBtn, prioritiesPanel) as GameObject;
                clone.name = $"FavoriteBtn{i + 1}";

                // Reposicionamiento igual que antes…
                var rt = clone.GetComponent<RectTransform>();
                rt.sizeDelta = sampleRt.sizeDelta;
                rt.anchorMin = sampleRt.anchorMin;
                rt.anchorMax = sampleRt.anchorMax;
                rt.pivot = sampleRt.pivot;
                rt.anchoredPosition = sampleRt.anchoredPosition + new Vector2(-(sampleRt.sizeDelta.x + 10) * (i + 1), 0);

                // Texto con saltos de línea tal cual
                var txt = clone.GetComponentInChildren<Text>();
                txt.text = string.Join("\n", Plugin.Instance.Presets[i].Value.Split(','));

                // Aquí viene lo importante:
                var btn = clone.GetComponent<Button>();
                int idx = i;  // captura para el callback
                btn.onClick.AddListener(() =>
                {
                    // 1) Accedemos al Tower privado
                    var towerField = AccessTools.Field(typeof(TowerUI), "myTower");
                    var tower = (Tower)towerField.GetValue(__instance);

                    // 2) Leemos el preset
                    var parts = Plugin.Instance.Presets[idx].Value.Split(',');

                    // 3) Aplicamos al array real de prioridades
                    var s0 = parts.ElementAtOrDefault(0);
                    if (!string.IsNullOrEmpty(s0)
                        && Enum.TryParse<Tower.Priority>(s0, out var p0))
                    {
                        tower.priorities[0] = p0;
                    }

                    var s1 = parts.ElementAtOrDefault(1);
                    if (!string.IsNullOrEmpty(s1)
                        && Enum.TryParse<Tower.Priority>(s1, out var p1))
                    {
                        tower.priorities[1] = p1;
                    }

                    var s2 = parts.ElementAtOrDefault(2);
                    if (!string.IsNullOrEmpty(s2)
                        && Enum.TryParse<Tower.Priority>(s2, out var p2))
                    {
                        tower.priorities[2] = p2;
                    }

                    // 4) Refrescamos la UI para que se vean y apliquen de verdad
                    __instance.SetStats(tower);

                    // 5) (Opcional) loguear para depurar
                    Plugin.Instance.Logger.LogDebug(
                      $"[FavoritePriorities] Preset {idx + 1} aplicado: " +
                      string.Join(",", tower.priorities));
                });
            }
        }
    }

}
