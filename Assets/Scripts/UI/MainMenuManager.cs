using UnityEngine;
using UnityEngine.UI;
using SimRedes.Simulation;

namespace SimRedes.UI
{
    /// <summary>
    /// Estado y paneles del menu principal (menu, actividades, conectividad,
    /// instrucciones) con sus botones de retorno y colores de seleccion.
    /// La navegacion por teclado NO vive aqui: el loop unico de teclado
    /// (flechas/W-S, Enter/Espacio, ESC/Backspace, digitos 1-6) es el de
    /// <see cref="MenuNavigator"/>, que navega los mismos botones del panel
    /// activo (C4: un solo dueño del input para evitar dobles invocaciones).
    /// </summary>
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

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        private void Start()
        {
            SetupButtons();
            ShowMainMenu();
            UpdateButtonSelection();
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
            HideAllPanels();
            StartSimulation();
        }

        public void OnActivitiesClicked()
        {
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
            // NOTA: no se crea TopologyManager aqui. SceneSetup.SetupManagers() es
            // quien lo instancia al iniciar la simulacion; el fallback anterior
            // new GameObject("TopologyManager") solo generaba singletons fantasma.
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
            HideAllPanels();
            StartSimulation();
        }
    }
}