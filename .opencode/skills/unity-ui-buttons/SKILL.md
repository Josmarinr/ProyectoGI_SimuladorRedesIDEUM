---
name: unity-ui-buttons
description: >-
  Use when creating buttons, panels, and UI components in SimuladorRedes
  IDEUM. Covers CreateMenuButton, CreateInfoText, rounded panels, color
  palette references, and common UI mistakes. Essential for any task
  involving panel creation or UI element generation.
---

# Skill: Creación de Botones y Paneles Unity

## Uso de UIComponents y Colors

```csharp
using UIComp = SimRedes.UI.UIComponents;
using UIColors = SimRedes.UI.UIComponents.Colors;
```

## Panel Redondeado

```csharp
GameObject panelObj = UIComp.CreateRoundedPanel(
    parent,
    new Vector2(ancho, alto),
    borderRadius,
    UIColors.surfacePanel,      // color fondo
    UIColors.borderAccent        // color borde
);
panelObj.name = "NombrePanel";
```

## Botón de Menú

```csharp
Button CreateMenuButton(Transform parent, string name, string label, 
    Vector2 position, Vector2 size, Font font, int fontSize = 18)
{
    var btnObj = new GameObject(name);
    btnObj.transform.SetParent(parent, false);

    var rect = btnObj.AddComponent<RectTransform>();
    rect.anchoredPosition = position;
    rect.sizeDelta = size;

    var image = btnObj.AddComponent<Image>();
    image.color = new Color(0.15f, 0.35f, 0.55f, 1f);

    Texture2D normalTex = UIComp.CreateRoundedRectTexture(
        (int)size.x, (int)size.y, 
        Mathf.Min((int)size.x, (int)size.y) / 3,
        UIColors.buttonNormal, UIColors.borderAccent, 2f);
    image.sprite = Sprite.Create(normalTex, new Rect(0, 0, (int)size.x, (int)size.y), 
        new Vector2(0.5f, 0.5f), 100);
    image.type = Image.Type.Sliced;

    var btn = btnObj.AddComponent<Button>();
    btn.colors = GetButtonColors(UIColors.buttonNormal);

    var textObj = new GameObject("Text");
    textObj.transform.SetParent(btnObj.transform, false);
    var textRect = textObj.AddComponent<RectTransform>();
    textRect.anchorMin = Vector2.zero;
    textRect.anchorMax = Vector2.one;
    textRect.offsetMin = Vector2.zero;
    textRect.offsetMax = Vector2.zero;

    var text = textObj.AddComponent<Text>();
    text.text = label;
    text.color = UIColors.textPrimary;
    text.fontSize = fontSize;
    text.fontStyle = FontStyle.Bold;
    text.alignment = TextAnchor.MiddleCenter;
    text.font = font;

    return btn;
}
```

## Texto Info

```csharp
private Text CreateInfoText(Transform parent, string text, Vector2 position, 
    Font font, int fontSize, Color color, bool bold)
{
    var textObj = new GameObject("InfoText");
    textObj.transform.SetParent(parent, false);
    var textRect = textObj.AddComponent<RectTransform>();
    textRect.anchorMin = new Vector2(0.5f, 0.5f);
    textRect.anchorMax = new Vector2(0.5f, 0.5f);
    textRect.anchoredPosition = position;
    textRect.sizeDelta = new Vector2(260, 20);

    var textComponent = textObj.AddComponent<Text>();
    textComponent.text = text;
    textComponent.color = color;
    textComponent.fontSize = fontSize;
    textComponent.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
    textComponent.alignment = TextAnchor.MiddleCenter;
    textComponent.font = font;

    return textComponent;
}
```

## Errores Comunes
- No usar el alias UIComp para acceder a metodos
- Olvidar asignar font al texto
- Textos sin nombre unico (dificultan FindUITexts)
- Posicion incorrecta del panel (usar anchor y pivot correctos)
