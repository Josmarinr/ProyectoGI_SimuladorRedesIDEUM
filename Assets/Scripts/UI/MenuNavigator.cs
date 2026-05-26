using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace SimRedes.UI
{
    public class MenuNavigator : MonoBehaviour
    {
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
        [SerializeField] private float normalWidth = 260f;
        [SerializeField] private float normalHeight = 50f;

        private Vector3[] targetScales;
        private bool[] isHovered;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(Instance.gameObject);
            }
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        private void Start()
        {
            UnityEngine.Debug.Log("[MenuNavigator] Iniciado");
        }

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
                PlayClickSound();
                UnityEngine.Debug.Log("[Navigator] Arriba, index: " + selectedIndex);
            }
            else if (keyboard.downArrowKey.wasPressedThisFrame || keyboard.sKey.wasPressedThisFrame)
            {
                selectedIndex++;
                if (selectedIndex >= currentButtons.Length) selectedIndex = 0;
                UpdateSelection();
                PlayClickSound();
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
            
            if (Input.GetMouseButtonDown(0))
            {
                CheckMouseClick();
            }
        }

        private void CheckMouseClick()
        {
            if (currentButtons == null || currentPanel == null) return;
            
            Vector2 mousePos = Input.mousePosition;
            
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

        private void UpdateHoverState()
        {
            if (currentButtons == null || currentPanel == null) return;
            
            Vector2 mousePos = Input.mousePosition;
            
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

        private void PlayClickSound()
        {
            #if UNITY_EDITOR
            // Debug feedback
            #endif
        }

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

        private void SelectAndInvoke(int index)
        {
            if (currentButtons != null && index >= 0 && index < currentButtons.Length)
            {
                selectedIndex = index;
                UpdateSelection();
                InvokeSelectedButton();
            }
        }

        private void InvokeSelectedButton()
        {
            if (selectedIndex >= 0 && selectedIndex < currentButtons.Length && currentButtons[selectedIndex] != null)
            {
                UnityEngine.Debug.Log("[Navigator] Invocando boton: " + selectedIndex);
                currentButtons[selectedIndex].onClick.Invoke();
            }
        }

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
                        rt.sizeDelta = new Vector2(normalWidth * selectScale, normalHeight * selectScale);
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
                        rt.sizeDelta = new Vector2(normalWidth, normalHeight);
                    }

                    if (text != null)
                    {
                        text.color = UIComponents.Colors.textSecondary;
                        text.fontStyle = FontStyle.Normal;
                    }
                }
            }
        }

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