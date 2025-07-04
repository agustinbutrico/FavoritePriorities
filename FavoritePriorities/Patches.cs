﻿using HarmonyLib;
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace FavoritePriorities
{
    [HarmonyPatch(typeof(TowerUI), "Start")]
    public static class Patch_TowerUI_Start
    {
        static void Postfix(TowerUI __instance)
        {
            var canvas = __instance.transform.Find("Canvas");
            var favPanelTransform = canvas.Find("FavoritePrioritiesPanel");
            GameObject favPanel;

            if (favPanelTransform == null)
            {
                var template = canvas.Find("RightPanel");
                if (template == null)
                {
                    Plugin.Log.LogError("[FavoritePriorities] No se encontró RightPanel para clonar.");
                    return;
                }

                favPanel = UnityEngine.Object.Instantiate(template.gameObject, canvas);
                favPanel.name = "FavoritePrioritiesPanel";
                favPanelTransform = favPanel.transform;

                foreach (Transform child in favPanelTransform)
                    UnityEngine.Object.Destroy(child.gameObject);

                var favRt = favPanel.GetComponent<RectTransform>();
                favRt.anchorMin = new Vector2(0, 0.5f);
                favRt.anchorMax = new Vector2(0, 0.5f);
                favRt.pivot = new Vector2(1, 1);
                favRt.localPosition = new Vector3(-0.8f, -3.75f, 0);
                favRt.sizeDelta = new Vector2(5f, 3f); // tamaño más razonable
                favRt.localScale = Vector3.one;

                var img = favPanel.GetComponent<Image>();
                if (img != null)
                    img.color = new Color(1f, 1f, 0.7f, 0f);

                var layout = favPanel.AddComponent<VerticalLayoutGroup>();
                layout.padding = new RectOffset(0, 0, 0, 0); // Sin padding
                layout.spacing = 0.2f; // separación mínima real
                layout.childAlignment = TextAnchor.MiddleCenter;

                favPanelTransform.SetAsLastSibling();
                favPanel.SetActive(true);
            }
            else
            {
                favPanel = favPanelTransform.gameObject;
            }

            var sampleBtn = canvas.Find("Demolish Button").gameObject;
            var sampleText = sampleBtn.GetComponentInChildren<Text>();
            var sampleImage = sampleBtn.GetComponent<Image>();
            var sampleColors = sampleBtn.GetComponent<Button>().colors;

            for (int i = 0; i < 5; i++)
            {
                var clone = UnityEngine.Object.Instantiate(sampleBtn, favPanelTransform);
                clone.name = $"FavoriteBtn{i + 1}";

                // Ajustar tamaño manual
                var rt = clone.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(3.5f, 0.3f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.localScale = Vector3.one;
                rt.localPosition = Vector3.zero;

                // Reemplazar componente Button
                var oldBtn = clone.GetComponent<Button>();
                if (oldBtn != null) UnityEngine.Object.DestroyImmediate(oldBtn);
                var newBtn = clone.AddComponent<Button>();
                newBtn.colors = sampleColors;
                newBtn.transition = Selectable.Transition.ColorTint;

                // Mantener imagen
                var img = clone.GetComponent<Image>();
                img.sprite = sampleImage.sprite;
                img.type = sampleImage.type;
                img.color = sampleImage.color;

                // Configurar texto
                var txt = clone.GetComponentInChildren<Text>();
                txt.text = Plugin.Instance.Priorities[i].Value.Replace(",", ", ");
                txt.font = sampleText.font;
                txt.fontSize = sampleText.fontSize;
                txt.color = sampleText.color;
                txt.alignment = sampleText.alignment;
                txt.resizeTextForBestFit = sampleText.resizeTextForBestFit;
                txt.resizeTextMinSize = 6;
                txt.resizeTextMaxSize = 24;

                // Asignar funcionalidad real
                int idx = i;
                newBtn.onClick.AddListener(() => ApplyPreset(__instance, idx));
            }
            __instance.StartCoroutine(HandleShortcutsAndToggle(__instance, favPanel));
        }

        private static void ApplyPreset(TowerUI ui, int idx)
        {
            var towerField = AccessTools.Field(typeof(TowerUI), "myTower");
            var tower = (Tower)towerField.GetValue(ui);
            var parts = Plugin.Instance.Priorities[idx].Value.Split(',');

            if (parts.Length > 0 && Enum.TryParse(parts[0], out Tower.Priority p0)) tower.priorities[0] = p0;
            if (parts.Length > 1 && Enum.TryParse(parts[1], out Tower.Priority p1)) tower.priorities[1] = p1;
            if (parts.Length > 2 && Enum.TryParse(parts[2], out Tower.Priority p2)) tower.priorities[2] = p2;

            tower.CopyPriorities();
            tower.PastePriorities();

            ui.SetStats(tower);    // Update the UI to reflect the changes
        }

        private static IEnumerator HandleShortcutsAndToggle(TowerUI ui, GameObject panel)
        {
            // Inicializar visibilidad según el estado global
            panel.SetActive(Plugin.ShowFavoritePanel);

            while (ui != null && ui.isActiveAndEnabled)
            {
                // Toggle panel con tecla configurable
                if (UnityEngine.Input.GetKeyDown(Plugin.Instance.TogglePanelKey.Value))
                {
                    Plugin.ShowFavoritePanel = !Plugin.ShowFavoritePanel;
                    panel.SetActive(Plugin.ShowFavoritePanel);
                }

                // Usar keybinds configurables para cada prioridad
                for (int i = 0; i < Plugin.Instance.PriorityKeybinds.Length; i++)
                {
                    if (UnityEngine.Input.GetKeyDown(Plugin.Instance.PriorityKeybinds[i].Value))
                    {
                        ApplyPreset(ui, i);
                    }
                }

                yield return null;
            }
        }
    }
}
