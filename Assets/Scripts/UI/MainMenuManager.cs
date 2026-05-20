using UnityEngine;
using UnityEngine.UI;
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
        private float buttonScaleNormal = 1f;
        private float buttonScaleSelected = 1.1f;

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

            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            {
                selectedIndex--;
                if (selectedIndex < 0) selectedIndex = currentMenuButtons.Length - 1;
                UpdateButtonSelection();
                UnityEngine.Debug.Log("[Menu] Navegando arriba, index: " + selectedIndex);
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            {
                selectedIndex++;
                if (selectedIndex >= currentMenuButtons.Length) selectedIndex = 0;
                UpdateButtonSelection();
                UnityEngine.Debug.Log("[Menu] Navegando abajo, index: " + selectedIndex);
            }
            else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space))
            {
                if (selectedIndex >= 0 && selectedIndex < currentMenuButtons.Length)
                {
                    currentMenuButtons[selectedIndex].onClick.Invoke();
                    UnityEngine.Debug.Log("[Menu] Enter presionado, invocando boton: " + selectedIndex);
                }
            }
            else if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
            {
                SelectButtonByIndex(0);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
            {
                SelectButtonByIndex(1);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
            {
                SelectButtonByIndex(2);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4))
            {
                SelectButtonByIndex(3);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Keypad5))
            {
                SelectButtonByIndex(4);
            }
            else if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Backspace))
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
                        if (i == selectedIndex)
                        {
                            image.color = new Color(0.4f, 0.6f, 0.8f, 1f);
                            var rt = currentMenuButtons[i].GetComponent<RectTransform>();
                            if (rt != null)
                            {
                                rt.sizeDelta = new Vector2(currentMenuButtons[i].GetComponent<RectTransform>().sizeDelta.x * buttonScaleSelected,
                                                          currentMenuButtons[i].GetComponent<RectTransform>().sizeDelta.y * buttonScaleSelected);
                            }
                        }
                        else
                        {
                            image.color = new Color(0.2f, 0.4f, 0.6f, 1f);
                            var rt = currentMenuButtons[i].GetComponent<RectTransform>();
                            if (rt != null)
                            {
                                rt.sizeDelta = new Vector2(rt.sizeDelta.x / buttonScaleSelected,
                                                          rt.sizeDelta.y / buttonScaleSelected);
                            }
                        }
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
            var gameManager = FindObjectOfType<GameManager>();
            if (gameManager == null)
            {
                var gmObj = new GameObject("GameManager");
                gameManager = gmObj.AddComponent<GameManager>();
            }

            var topology = FindObjectOfType<TopologyManager>();
            if (topology == null)
            {
                var tObj = new GameObject("TopologyManager");
                topology = tObj.AddComponent<TopologyManager>();
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