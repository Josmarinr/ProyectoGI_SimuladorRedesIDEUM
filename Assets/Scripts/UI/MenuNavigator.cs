using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace SimRedes.UI
{
    /// <summary>
    /// Navegador de menus con soporte para teclado (flechas, Enter, ESC, numeros) y mouse.
    /// Mantiene un indice de seleccion, animaciones de escala y colores de boton.
    /// </summary>
    public class MenuNavigator : MonoBehaviour
    {
        /// <summary>
        /// Instancia singleton del navegador de menus.
        /// </summary>
        public static MenuNavigator Instance { get; private set; }

        private Button[] currentButtons;
        private int selectedIndex = 0;
        private GameObject currentPanel;
        private System.Action onEscape;
        private bool isInitialized = false;
        
        [Header("Visual Settings")]
        [SerializeField] private Color normalColor = UIComponents.Colors.buttonNormal;
        [SerializeField] private Color selectedColor = UIComponents.Colors.buttonSelected;
        [SerializeField] private Color hoverColor = UIComponents.Colors.buttonHover;
        [SerializeField] private float selectScale = 1.15f;
        [SerializeField] private float animSpeed = 8f;

        private Vector3[] targetScales;
        private bool[] isHovered;

        /// <summary>
        /// Configura el singleton. Si ya existe otra instancia, la destruye y se queda con la nueva.
        /// </summary>
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(Instance.gameObject);
            }
            Instance = this;
        }

        /// <summary>
        /// Limpia la referencia al singleton al destruirse.
        /// </summary>
        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        private void Start()
        {
            UnityEngine.Debug.Log("[MenuNavigator] Iniciado");
        }

        /// <summary>
        /// Procesa entrada del teclado (flechas, Enter, ESC, digitos 1-6) y del mouse
        /// para navegar entre botones del panel activo. Actualiza hover y animaciones en cada frame.
        /// </summary>
        private void Update()
        {
            if (!isInitialized || currentButtons == null || currentButtons.Length == 0) return;

            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            UpdateHoverState();
            UpdateAnimations();

            if (keyboard.upArrowKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame)
            {
                selectedIndex--;
                if (selectedIndex < 0) selectedIndex = currentButtons.Length - 1;
                UpdateSelection();
                UnityEngine.Debug.Log("[Navigator] Arriba, index: " + selectedIndex);
            }
            else if (keyboard.downArrowKey.wasPressedThisFrame || keyboard.sKey.wasPressedThisFrame)
            {
                selectedIndex++;
                if (selectedIndex >= currentButtons.Length) selectedIndex = 0;
                UpdateSelection();
                UnityEngine.Debug.Log("[Navigator] Abajo, index: " + selectedIndex);
            }
            else if (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame)
            {
                InvokeSelectedButton();
            }
            else if (keyboard.escapeKey.wasPressedThisFrame || keyboard.backspaceKey.wasPressedThisFrame)
            {
                if (onEscape != null)
                {
                    UnityEngine.Debug.Log("[Navigator] ESC presionado");
                    onEscape.Invoke();
                }
            }
            else if (keyboard.digit1Key.wasPressedThisFrame || keyboard.numpad1Key.wasPressedThisFrame)
            {
                SelectAndInvoke(0);
            }
            else if (keyboard.digit2Key.wasPressedThisFrame || keyboard.numpad2Key.wasPressedThisFrame)
            {
                SelectAndInvoke(1);
            }
            else if (keyboard.digit3Key.wasPressedThisFrame || keyboard.numpad3Key.wasPressedThisFrame)
            {
                SelectAndInvoke(2);
            }
            else if (keyboard.digit4Key.wasPressedThisFrame || keyboard.numpad4Key.wasPressedThisFrame)
            {
                SelectAndInvoke(3);
            }
            else if (keyboard.digit5Key.wasPressedThisFrame || keyboard.numpad5Key.wasPressedThisFrame)
            {
                SelectAndInvoke(4);
            }
            else if (keyboard.digit6Key.wasPressedThisFrame || keyboard.numpad6Key.wasPressedThisFrame)
            {
                SelectAndInvoke(5);
            }
            
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                CheckMouseClick();
            }
        }

        /// <summary>
        /// Detecta si el mouse hizo clic sobre algun boton y lo selecciona e invoca.
        /// </summary>
        private void CheckMouseClick()
        {
            if (currentButtons == null || currentPanel == null) return;
            
            if (Mouse.current != null)
            {
                Vector2 mousePos = Mouse.current.position.ReadValue();
                
                for (int i = 0; i < currentButtons.Length; i++)
                {
                    if (currentButtons[i] == null) continue;
                    
                    RectTransform rt = currentButtons[i].GetComponent<RectTransform>();
                    if (rt == null) continue;
                    
                    bool isOver = RectTransformUtility.RectangleContainsScreenPoint(rt, mousePos, null);
                    
                    if (isOver)
                    {
                        selectedIndex = i;
                        UpdateSelection();
                        InvokeSelectedButton();
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Actualiza el estado de hover sobre los botones segun la posicion del mouse.
        /// Cambia el indice seleccionado si el mouse pasa sobre otro boton.
        /// </summary>
        private void UpdateHoverState()
        {
            if (currentButtons == null || currentPanel == null) return;
            
            if (Mouse.current != null)
            {
                Vector2 mousePos = Mouse.current.position.ReadValue();
                
                for (int i = 0; i < currentButtons.Length; i++)
                {
                    if (currentButtons[i] == null) continue;
                    
                    RectTransform rt = currentButtons[i].GetComponent<RectTransform>();
                    if (rt == null) continue;
                    
                    bool isOver = RectTransformUtility.RectangleContainsScreenPoint(rt, mousePos, null);
                    
                    if (isOver && i != selectedIndex)
                    {
                        selectedIndex = i;
                        UpdateSelection();
                    }
                    
                    if (isOver != isHovered[i])
                    {
                        isHovered[i] = isOver;
                        if (!isOver && i != selectedIndex)
                        {
                            targetScales[i] = Vector3.one;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Interpola la escala de cada boton hacia su escala objetivo para una animacion suave.
        /// </summary>
        private void UpdateAnimations()
        {
            if (currentButtons == null || targetScales == null) return;

            for (int i = 0; i < currentButtons.Length; i++)
            {
                if (currentButtons[i] == null) continue;

                RectTransform rt = currentButtons[i].GetComponent<RectTransform>();
                if (rt != null)
                {
                    Vector3 currentScale = rt.localScale;
                    Vector3 target = targetScales[i];
                    Vector3 newScale = Vector3.Lerp(currentScale, target, Time.deltaTime * animSpeed);
                    rt.localScale = newScale;
                }
            }
        }

        /// <summary>
        /// Configura el panel actual para navegacion. Busca todos los botones hijos,
        /// reinicia el indice de seleccion y las escalas, y aplica la seleccion inicial.
        /// </summary>
        /// <param name="panel">GameObject del panel que contiene los botones.</param>
        /// <param name="onEscapeCallback">Accion a ejecutar al presionar ESC.</param>
        public void SetupPanel(GameObject panel, System.Action onEscapeCallback)
        {
            if (panel == null) return;

            currentPanel = panel;
            onEscape = onEscapeCallback;

            Button[] allButtons = panel.GetComponentsInChildren<Button>();
            currentButtons = allButtons;
            selectedIndex = 0;
            targetScales = new Vector3[allButtons.Length];
            isHovered = new bool[allButtons.Length];
            
            for (int i = 0; i < allButtons.Length; i++)
            {
                targetScales[i] = Vector3.one;
                isHovered[i] = false;
            }

            isInitialized = true;
            UpdateSelection();
            UnityEngine.Debug.Log("[Navigator] Panel configurado: " + panel.name + ", botones: " + currentButtons.Length);
        }

        /// <summary>
        /// Selecciona e invoca el boton en el indice dado (usado por atajos numericos 1-6).
        /// </summary>
        /// <param name="index">Indice del boton a seleccionar e invocar.</param>
        private void SelectAndInvoke(int index)
        {
            if (currentButtons != null && index >= 0 && index < currentButtons.Length)
            {
                selectedIndex = index;
                UpdateSelection();
                InvokeSelectedButton();
            }
        }

        /// <summary>
        /// Invoca el evento onClick del boton actualmente seleccionado.
        /// </summary>
        private void InvokeSelectedButton()
        {
            if (selectedIndex >= 0 && selectedIndex < currentButtons.Length && currentButtons[selectedIndex] != null)
            {
                UnityEngine.Debug.Log("[Navigator] Invocando boton: " + selectedIndex);
                currentButtons[selectedIndex].onClick.Invoke();
            }
        }

        /// <summary>
        /// Actualiza los colores, escala y estilo de texto de todos los botones
        /// segun el indice seleccionado actual.
        /// </summary>
        private void UpdateSelection()
        {
            if (currentButtons == null) return;

            for (int i = 0; i < currentButtons.Length; i++)
            {
                if (currentButtons[i] == null) continue;

                var image = currentButtons[i].GetComponent<Image>();
                var rt = currentButtons[i].GetComponent<RectTransform>();
                var text = currentButtons[i].GetComponentInChildren<Text>();

                if (i == selectedIndex)
                {
                    if (image != null)
                        image.color = selectedColor;

                    if (rt != null)
                    {
                        targetScales[i] = Vector3.one * selectScale;
                    }

                    if (text != null)
                    {
                        text.color = UIComponents.Colors.textPrimary;
                        text.fontStyle = FontStyle.Bold;
                    }
                }
                else
                {
                    if (image != null)
                        image.color = normalColor;

                    if (rt != null)
                    {
                        targetScales[i] = Vector3.one;
                    }

                    if (text != null)
                    {
                        text.color = UIComponents.Colors.textSecondary;
                        text.fontStyle = FontStyle.Normal;
                    }
                }
            }
        }

        /// <summary>
        /// Limpia el estado del navegador: botones, panel, callback y arreglos de animacion.
        /// </summary>
        public void ClearPanel()
        {
            currentButtons = null;
            currentPanel = null;
            onEscape = null;
            selectedIndex = 0;
            isInitialized = false;
            
            if (targetScales != null)
                System.Array.Clear(targetScales, 0, targetScales.Length);
            if (isHovered != null)
                System.Array.Clear(isHovered, 0, isHovered.Length);
        }
    }
}