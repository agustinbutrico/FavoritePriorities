using HarmonyLib;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace FavoritePriorities
{
    [HarmonyPatch(typeof(TowerUI), "Start")]
    public static class Patch_TowerUI_Start
    {
        static void Postfix(TowerUI __instance)
        {
            Plugin.Log.LogDebug("[FavoritePriorities] Postfix TowerUI.Start iniciado");

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
                favRt.pivot = new Vector2(0, 0.5f);
                favRt.localPosition = new Vector3(-9.4f, 0.2f, 0);
                favRt.sizeDelta = new Vector2(300, favRt.sizeDelta.y); // más ancho
                favRt.localScale = Vector3.one;

                var img = favPanel.GetComponent<Image>();
                if (img != null)
                    img.color = new Color(1f, 1f, 0f, 0.5f);

                var layout = favPanel.AddComponent<VerticalLayoutGroup>();
                layout.padding = new RectOffset(1, 1, 1, 1); // casi sin padding
                layout.spacing = 2; // separación mínima
                layout.childAlignment = TextAnchor.MiddleCenter;

                favPanelTransform.SetAsLastSibling();
                favPanel.SetActive(true);
            }
            else
            {
                favPanel = favPanelTransform.gameObject;
            }

            var sampleBtn = canvas.Find("Demolish Button").gameObject;
            var sampleRt = sampleBtn.GetComponent<RectTransform>();
            var sampleText = sampleBtn.GetComponentInChildren<Text>();
            var sampleImage = sampleBtn.GetComponent<Image>();
            var sampleColors = sampleBtn.GetComponent<Button>().colors;

            for (int i = 0; i < 5; i++)
            {
                var clone = UnityEngine.Object.Instantiate(sampleBtn, favPanelTransform);
                clone.name = $"FavoriteBtn{i + 1}";

                var rt = clone.GetComponent<RectTransform>();
                rt.sizeDelta = sampleRt.sizeDelta;
                rt.anchorMin = sampleRt.anchorMin;
                rt.anchorMax = sampleRt.anchorMax;
                rt.pivot = sampleRt.pivot;
                rt.localScale = sampleRt.localScale;
                rt.localPosition = Vector3.zero;

                var img = clone.GetComponent<Image>();
                img.sprite = sampleImage.sprite;
                img.type = sampleImage.type;
                img.color = sampleImage.color;

                var btn = clone.GetComponent<Button>();
                btn.colors = sampleColors;

                var txt = clone.GetComponentInChildren<Text>();
                txt.text = Plugin.Instance.Presets[i].Value.Replace(",", ", ");
                txt.font = sampleText.font;
                txt.fontSize = sampleText.fontSize;
                txt.color = sampleText.color;
                txt.alignment = sampleText.alignment;
                txt.resizeTextForBestFit = sampleText.resizeTextForBestFit;

                int idx = i;
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() =>
                {
                    var towerField = AccessTools.Field(typeof(TowerUI), "myTower");
                    var tower = (Tower)towerField.GetValue(__instance);
                    var parts = Plugin.Instance.Presets[idx].Value.Split(',');

                    if (parts.Length > 0 && Enum.TryParse(parts[0], out Tower.Priority p0)) tower.priorities[0] = p0;
                    if (parts.Length > 1 && Enum.TryParse(parts[1], out Tower.Priority p1)) tower.priorities[1] = p1;
                    if (parts.Length > 2 && Enum.TryParse(parts[2], out Tower.Priority p2)) tower.priorities[2] = p2;

                    __instance.SetStats(tower);
                });
            }
        }
    }
}
