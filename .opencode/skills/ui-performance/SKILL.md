---
name: ui-performance
description: >-
  Use when optimizing UI performance in SimuladorRedes IDEUM. Covers
  Canvas Scaler configuration (Expand mode, reference resolution), avoiding
  re-renders, text clarity fixes, and Unity performance anti-patterns.
  Use when the user reports blurry text, low FPS, or UI lag.
---

# Skill: Unity UI Performance Optimization

## Canvas Scaler - Configuración Óptima

```csharp
var canvas = FindObjectOfType<Canvas>();
var scaler = canvas.GetComponent<CanvasScaler>();

// Modo recomendado para pantallas fixed (1920x1080)
scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
scaler.referenceResolution = new Vector2(1920, 1080);
scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
scaler.matchWidthOrHeight = 0.5f;

// Render mode
canvas.renderMode = RenderMode.ScreenSpaceOverlay;
canvas.sortingOrder = 100;
```

## Evitar Re-renders

1. **Separar Canvas**: Elementos estáticos en Canvas separado de dinámicos
2. **Evitar Layout Groups** si no son necesarios
3. **Desactivar Raycast Target** en elementos que no necesitan clicks
4. **No usar "Best Fit"** en Text components

```csharp
// Desactivar raycast en imágenes decorativas
Image decorImage = GetComponent<Image>();
decorImage.raycastTarget = false;
```

## Textos sin Borrosidad

```csharp
// Usar texto con tamaño fijo, no Best Fit
Text text = gameObject.AddComponent<Text>();
text.fontSize = 18;
text.resizeTextForBestFit = false;
text.dynamicPixelsPerUnit = 1; // Para CanvasScaler
```

## Errores a Evitar

- No usar `Camera.main` repetidamente (cachar referencia en Start)
- No usar `FindObjectOfType` en Update
- No crear nuevos materiales en cada frame
