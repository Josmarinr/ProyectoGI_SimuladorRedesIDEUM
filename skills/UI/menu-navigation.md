# Skill: Navegación de Menús

## Descripción
Sistema de navegación por teclado y mouse para todos los paneles.

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
        // Teclas numericas 1-5
        else if (Input.GetKeyDown(KeyCode.Alpha1))
            SelectAndInvoke(0);
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            SelectAndInvoke(1);
        // etc...
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

    private void UpdateSelection()
    {
        for (int i = 0; i < currentButtons.Length; i++)
        {
            var image = currentButtons[i].GetComponent<Image>();
            var rt = currentButtons[i].GetComponent<RectTransform>();
            var text = currentButtons[i].GetComponentInChildren<Text>();

            if (i == selectedIndex)
            {
                image.color = selectedColor;
                rt.sizeDelta = new Vector2(normalWidth * selectScale, normalHeight * selectScale);
                text.color = UIColors.textPrimary;
                text.fontStyle = FontStyle.Bold;
            }
            else
            {
                image.color = normalColor;
                rt.sizeDelta = new Vector2(normalWidth, normalHeight);
                text.color = UIColors.textSecondary;
                text.fontStyle = FontStyle.Normal;
            }
        }
    }

    public void ClearPanel()
    {
        currentButtons = null;
        currentPanel = null;
        isInitialized = false;
    }
}
```

## Setup en SceneSetup

```csharp
private void CreateMainMenu(Transform canvasTransform)
{
    // ... crear panel ...

    var navigator = FindObjectOfType<MenuNavigator>();
    if (navigator != null)
    {
        navigator.SetupPanel(mainMenuPanel, () => {
            // callback de ESC
        });
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

## Singleton Pattern para MenuNavigator

```csharp
private void Awake()
{
    if (Instance != null && Instance != this)
        Destroy(Instance.gameObject);
    Instance = this;
}

private void OnDestroy()
{
    if (Instance == this)
        Instance = null;
}
```

## Errores Comunes
- No destruir MenuNavigator antes de crear nuevo menu
- No llamar ClearPanel() al volver al menu
- No usar el singleton Instance para acceder
- Olvidar isInitialized check en Update
