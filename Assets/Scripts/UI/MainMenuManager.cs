using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using SimRedes.Network;
using SimRedes.Simulation;

namespace SimRedes.UI
{
    public class MainMenuManager : MonoBehaviour
    {
        public static MainMenuManager Instance { get; private set; }

        [Header("Paneles")]
        [SerializeField] public GameObject mainMenuPanel;
        [SerializeField] public GameObject activitiesPanel;
        [SerializeField] public GameObject connectivityPanel;
        [SerializeField] public GameObject instructionsPanel;

        [Header("Botones Menu")]
        [SerializeField] private Button startButton;
        [SerializeField] private Button activitiesButton;
        [SerializeField] private Button connectivityButton;
        [SerializeField] private Button instructionsButton;
        [SerializeField] private Button exitButton;

        [Header("Botones de Return")]
        [SerializeField] private Button backFromActivities;
        [SerializeField] private Button backFromConnectivity;
        [SerializeField] private Button backFromInstructions;

        private int selectedIndex = 0;
        private Button[] currentMenuButtons;
        private bool menuActive = false;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        private void Start()
        {
            menuActive = true;
            SetupButtons();
            ShowMainMenu();
            UpdateButtonSelection();
        }

        private void Update()
        {
            if (menuActive)
            {
                HandleKeyboardNavigation();
            }
        }

        private void HandleKeyboardNavigation()
        {
            if (currentMenuButtons == null || currentMenuButtons.Length == 0) return;

            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            if (keyboard.upArrowKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame)
            {
                selectedIndex--;
                if (selectedIndex < 0) selectedIndex = currentMenuButtons.Length - 1;
                UpdateButtonSelection();
                UnityEngine.Debug.Log("[Menu] Navegando arriba, index: " + selectedIndex);
            }
            else if (keyboard.downArrowKey.wasPressedThisFrame || keyboard.sKey.wasPressedThisFrame)
            {
                selectedIndex++;
                if (selectedIndex >= currentMenuButtons.Length) selectedIndex = 0;
                UpdateButtonSelection();
                UnityEngine.Debug.Log("[Menu] Navegando abajo, index: " + selectedIndex);
            }
            else if (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame)
            {
                if (selectedIndex >= 0 && selectedIndex < currentMenuButtons.Length)
                {
                    currentMenuButtons[selectedIndex].onClick.Invoke();
                    UnityEngine.Debug.Log("[Menu] Enter presionado, invocando boton: " + selectedIndex);
                }
            }
            else if (keyboard.digit1Key.wasPressedThisFrame || keyboard.numpad1Key.wasPressedThisFrame)
            {
                SelectButtonByIndex(0);
            }
            else if (keyboard.digit2Key.wasPressedThisFrame || keyboard.numpad2Key.wasPressedThisFrame)
            {
                SelectButtonByIndex(1);
            }
            else if (keyboard.digit3Key.wasPressedThisFrame || keyboard.numpad3Key.wasPressedThisFrame)
            {
                SelectButtonByIndex(2);
            }
            else if (keyboard.digit4Key.wasPressedThisFrame || keyboard.numpad4Key.wasPressedThisFrame)
            {
                SelectButtonByIndex(3);
            }
            else if (keyboard.digit5Key.wasPressedThisFrame || keyboard.numpad5Key.wasPressedThisFrame)
            {
                SelectButtonByIndex(4);
            }
            else if (keyboard.escapeKey.wasPressedThisFrame || keyboard.backspaceKey.wasPressedThisFrame)
            {
                GoBackToPreviousMenu();
            }
        }

        private void SelectButtonByIndex(int index)
        {
            if (currentMenuButtons != null && index >= 0 && index < currentMenuButtons.Length)
            {
                selectedIndex = index;
                UpdateButtonSelection();
                currentMenuButtons[selectedIndex].onClick.Invoke();
            }
        }

        private void UpdateButtonSelection()
        {
            if (currentMenuButtons == null) return;

            for (int i = 0; i < currentMenuButtons.Length; i++)
            {
                if (currentMenuButtons[i] != null)
                {
                    var image = currentMenuButtons[i].GetComponent<Image>();
                    if (image != null)
                    {
                        image.color = (i == selectedIndex)
                            ? new Color(0.4f, 0.6f, 0.8f, 1f)  // seleccionado: mas claro
                            : new Color(0.2f, 0.4f, 0.6f, 1f); // no seleccionado: color base
                    }
                }
            }
        }

        private void SetupButtons()
        {
            if (startButton != null)
                startButton.onClick.AddListener(OnStartClicked);

            if (activitiesButton != null)
                activitiesButton.onClick.AddListener(OnActivitiesClicked);

            if (connectivityButton != null)
                connectivityButton.onClick.AddListener(OnConnectivityClicked);

            if (instructionsButton != null)
                instructionsButton.onClick.AddListener(OnInstructionsClicked);

            if (exitButton != null)
                exitButton.onClick.AddListener(OnExitClicked);

            if (backFromActivities != null)
                backFromActivities.onClick.AddListener(ShowMainMenu);

            if (backFromConnectivity != null)
                backFromConnectivity.onClick.AddListener(ShowMainMenu);

            if (backFromInstructions != null)
                backFromInstructions.onClick.AddListener(ShowMainMenu);
        }

        public void ShowMainMenu()
        {
            menuActive = true;
            HideAllPanels();
            if (mainMenuPanel != null)
                mainMenuPanel.SetActive(true);

            Button[] buttons = mainMenuPanel.GetComponentsInChildren<Button>();
            currentMenuButtons = buttons;
            selectedIndex = 0;
            UpdateButtonSelection();

            UnityEngine.Debug.Log("[Menu] Mostrando menu principal");
        }

        public void OnStartClicked()
        {
            UnityEngine.Debug.Log("[Menu] Iniciando simulacion...");
            menuActive = false;
            HideAllPanels();
            StartSimulation();
        }

        public void OnActivitiesClicked()
        {
            menuActive = true;
            HideAllPanels();
            if (activitiesPanel != null)
                activitiesPanel.SetActive(true);

            Button[] buttons = activitiesPanel.GetComponentsInChildren<Button>();
            currentMenuButtons = buttons;
            selectedIndex = 0;
            UpdateButtonSelection();

            UnityEngine.Debug.Log("[Menu] Abriendo actividades");
        }

        public void OnConnectivityClicked()
        {
            menuActive = true;
            HideAllPanels();
            if (connectivityPanel != null)
                connectivityPanel.SetActive(true);

            Button[] buttons = connectivityPanel.GetComponentsInChildren<Button>();
            currentMenuButtons = buttons;
            selectedIndex = 0;
            UpdateButtonSelection();

            UnityEngine.Debug.Log("[Menu] Abriendo pruebas de conectividad");
        }

        public void OnInstructionsClicked()
        {
            menuActive = true;
            HideAllPanels();
            if (instructionsPanel != null)
                instructionsPanel.SetActive(true);

            Button[] buttons = instructionsPanel.GetComponentsInChildren<Button>();
            currentMenuButtons = buttons;
            selectedIndex = 0;
            UpdateButtonSelection();

            UnityEngine.Debug.Log("[Menu] Abriendo instrucciones");
        }

        public void OnExitClicked()
        {
            UnityEngine.Debug.Log("[Menu] Saliendo de la aplicacion");
            Application.Quit();

            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #endif
        }

        private void HideAllPanels()
        {
            if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
            if (activitiesPanel != null) activitiesPanel.SetActive(false);
            if (connectivityPanel != null) connectivityPanel.SetActive(false);
            if (instructionsPanel != null) instructionsPanel.SetActive(false);
        }

        private void StartSimulation()
        {
            var topology = Object.FindAnyObjectByType<TopologyManager>();
            if (topology == null)
            {
                var tObj = new GameObject("TopologyManager");
                topology = tObj.AddComponent<TopologyManager>();
            }

            // Asegurar que existe el contenedor GameManager (usado por ActivityLoader)
            if (GameObject.Find("GameManager") == null)
            {
                new GameObject("GameManager");
            }

            UnityEngine.Debug.Log("[Menu] Simulacion iniciada");
        }

        public void SelectActivity(string activityName)
        {
            UnityEngine.Debug.Log("[Menu] Actividad seleccionada: " + activityName);
            menuActive = false;
            HideAllPanels();
            StartSimulation();
        }

        private void GoBackToPreviousMenu()
        {
            if (activitiesPanel != null && activitiesPanel.activeSelf)
            {
                ShowMainMenu();
            }
            else if (connectivityPanel != null && connectivityPanel.activeSelf)
            {
                ShowMainMenu();
            }
            else if (instructionsPanel != null && instructionsPanel.activeSelf)
            {
                ShowMainMenu();
            }
        }
    }
}