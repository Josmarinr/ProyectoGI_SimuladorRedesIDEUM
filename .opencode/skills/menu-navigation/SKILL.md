---
name: menu-navigation
description: >-
  Use when creating or modifying menu navigation in SimuladorRedes IDEUM.
  Covers MenuNavigator singleton, keyboard navigation (arrows + numeric),
  panel setup, ESC callback, GoBackToMainMenu cleanup, and common
  navigation bugs. Use for any task involving the main menu or panel
  navigation system.
---

# Skill: Navegación de Menús

## MenuNavigator - Estructura

```csharp
public class MenuNavigator : MonoBehaviour
{
    public static MenuNavigator Instance { get; private set; }
    private Button[] currentButtons;
    private int selectedIndex = 0;
    private GameObject currentPanel;
    private System.Action onEscape;
    private bool isInitialized = false;

    private void Update()
    {
        if (!isInitialized || currentButtons == null) return;

        // Navegacion con flechas
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            selectedIndex--;
            if (selectedIndex < 0) selectedIndex = currentButtons.Length - 1;
            UpdateSelection();
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            selectedIndex++;
            if (selectedIndex >= currentButtons.Length) selectedIndex = 0;
            UpdateSelection();
        }
        // Seleccionar con Enter
        else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            InvokeSelectedButton();
        }
        // Click de mouse
        else if (Input.GetMouseButtonDown(0))
        {
            CheckMouseClick();
        }
    }

    public void SetupPanel(GameObject panel, System.Action onEscapeCallback)
    {
        currentPanel = panel;
        onEscape = onEscapeCallback;
        Button[] allButtons = panel.GetComponentsInChildren<Button>();
        currentButtons = allButtons;
        selectedIndex = 0;
        isInitialized = true;
        UpdateSelection();
    }

    public void ClearPanel()
    {
        currentButtons = null;
        currentPanel = null;
        isInitialized = false;
    }
}
```

## GoBackToMainMenu - Limpieza Correcta

```csharp
private void GoBackToMainMenu(Transform canvasTransform)
{
    // Limpiar MenuNavigator
    if (MenuNavigator.Instance != null)
        MenuNavigator.Instance.ClearPanel();

    // Destruir objetos existentes
    var existingNavigator = GameObject.Find("MenuNavigator");
    var existingMenuManager = GameObject.Find("MainMenuManager");
    var existingMainMenu = GameObject.Find("MainMenuPanel");

    if (existingNavigator != null) Destroy(existingNavigator);
    if (existingMenuManager != null) Destroy(existingMenuManager);
    if (existingMainMenu != null) Destroy(existingMainMenu);

    // Recrear menu
    CreateMainMenu(canvasTransform);
}
```

## Errores Comunes
- No destruir MenuNavigator antes de crear nuevo menu
- No llamar ClearPanel() al volver al menu
- No usar el singleton Instance para acceder
- Olvidar isInitialized check en Update
