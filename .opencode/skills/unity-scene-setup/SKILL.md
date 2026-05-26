---
name: unity-scene-setup
description: >-
  Use when creating or modifying scene setup, panels, or UI elements in
  SceneSetup.cs. Covers panel creation patterns, unique text naming,
  FindUITexts, button creation, topology example panels, and common
  mistakes. Essential for the architect and programmer agents when
  working on UI and scene initialization.
---

# Skill: Unity Scene Setup para Simulador de Redes

## Estructura de Panel TopologyInfoPanel

```csharp
// Crear panel en esquina superior derecha
GameObject panelObj = UIComp.CreateRoundedPanel(canvasTransform, new Vector2(380, 420), 20,
    UIColors.surfaceElevated,
    UIColors.borderAccent);
panelObj.name = "TopologyInfoPanel";

RectTransform panelRect = panelObj.GetComponent<RectTransform>();
panelRect.anchorMin = new Vector2(1f, 1f);
panelRect.anchorMax = new Vector2(1f, 1f);
panelRect.pivot = new Vector2(1f, 1f);
panelRect.anchoredPosition = new Vector2(-20, -20);
```

## Textos con Nombres Únicos

```csharp
var topologyTypeText = CreateInfoText(panelObj.transform, "Topologia: Sin topologia", ...);
topologyTypeText.gameObject.name = "TopologyTypeText";

var linkText = CreateInfoText(panelObj.transform, "Enlaces: 0", ...);
linkText.gameObject.name = "LinkCountText";

var routerText = CreateInfoText(panelObj.transform, "  Routers: 0", ...);
routerText.gameObject.name = "RouterCountText";

var switchText = CreateInfoText(panelObj.transform, "  Switches: 0", ...);
switchText.gameObject.name = "SwitchCountText";

var pcText = CreateInfoText(panelObj.transform, "  PCs: 0", ...);
pcText.gameObject.name = "PCCountText";
```

## Métodos de FindUITexts en BuildTopologyActivity

```csharp
private void FindUITexts()
{
    var panel = GameObject.Find("TopologyInfoPanel");
    if (panel != null)
    {
        var texts = panel.GetComponentsInChildren<Text>();
        foreach (var text in texts)
        {
            if (text.gameObject.name == "TopologyTypeText")
                topologyTypeText = text;
            else if (text.gameObject.name == "LinkCountText")
                linkCountText = text;
            // ...
        }
    }
}
```

## Orden de Creación Importante

```csharp
// CORRECTO: Primero crear panel, luego agregar componentes
SetupManagers();
CreateVisualizer(canvasTransform);
CreateBuildTopologyPanel(canvasTransform);  // Panel PRIMERO

// DESPUÉS agregar BuildTopologyActivity
if (gameManagerObj.GetComponent<BuildTopologyActivity>() == null)
    gameManagerObj.AddComponent<BuildTopologyActivity>();
```

## Errores Comunes

- ❌ Agregar componentes antes de crear el panel → Referencias nulas
- ❌ Textos sin nombre único → FindUITexts no los encuentra
- ❌ Llaves faltantes en métodos
