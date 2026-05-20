# Skill: Unity Scene Setup para Simulador de Redes

## Descripción
Guía para crear y modificar escenas en SceneSetup.cs para el Simulador de Redes IDEUM.

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
// Crear textos con nombres únicos para FindUITexts()
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
            else if (text.gameObject.name == "RouterCountText")
                routerCountText = text;
            else if (text.gameObject.name == "SwitchCountText")
                switchCountText = text;
            else if (text.gameObject.name == "PCCountText")
                pcCountText = text;
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

## Botón de Información (i)

```csharp
private Button CreateSmallInfoButton(Transform parent, Vector2 position, Font font)
{
    var btnObj = new GameObject("InfoButton");
    var rect = btnObj.AddComponent<RectTransform>();
    rect.anchoredPosition = position;
    rect.sizeDelta = new Vector2(34, 34);

    var img = btnObj.AddComponent<Image>();
    Texture2D tex = UIComp.CreateRoundedRectTexture(34, 34, 10, 
        UIColors.textPrimary, UIColors.textPrimary, 0f);
    img.sprite = Sprite.Create(tex, new Rect(0, 0, 34, 34), new Vector2(0.5f, 0.5f), 100);

    var btn = btnObj.AddComponent<Button>();
    btn.colors = GetButtonColors(UIColors.textPrimary);

    var textObj = new GameObject("Text");
    var text = textObj.AddComponent<Text>();
    text.text = "i";
    text.color = UIColors.backgroundBase;
    text.fontSize = 26;
    text.fontStyle = FontStyle.Bold;
    text.alignment = TextAnchor.MiddleCenter;
    text.font = font;

    return btn;
}
```

## Panel de Información de Topologías

```csharp
private void ToggleTopologyExamplePanel(Transform canvasTransform)
{
    var existingPanel = GameObject.Find("TopologyExamplePanel");
    if (existingPanel != null)
    {
        Destroy(existingPanel);
        return;
    }

    GameObject panelObj = UIComp.CreateRoundedPanel(canvasTransform, new Vector2(700, 600), 20,
        UIColors.surfacePanel,
        UIColors.textAccent);
    panelObj.name = "TopologyExamplePanel";

    // Contenido...
}
```

## Uso

1. Crear panel con `CreateRoundedPanel`
2. Asignar nombres únicos a cada texto
3. Crear botones con `CreateMenuButton`
4. Agregar listeners a los botones
5. Crear BuildTopologyActivity DESPUÉS del panel

## Errores Comunes

- ❌ Agregar componentes antes de crear el panel → Referencias nulas
- ❌ Textos sin nombre único → FindUITexts no los encuentra
- ❌ Llaves faltantes en métodos