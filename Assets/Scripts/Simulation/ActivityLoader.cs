using UnityEngine;
using SimRedes.Network;

namespace SimRedes.Simulation
{
    /// <summary>
    /// Fachada del cargador de actividades del simulador. Conserva la superficie publica
    /// historica (seleccion de actividad, simulacion libre, conectividad y protocolo) y
    /// delega la implementacion en los componentes extraidos: <see cref="ActivityDispatcher"/>
    /// (despacho/orquestacion), <see cref="ActivityHudFactory"/> (HUD) y
    /// <see cref="ScenarioLoader"/> (escenarios). Ver tarea C5 del plan de trabajo.
    /// </summary>
    public class ActivityLoader : MonoBehaviour
    {
        internal string selectedProtocol = null;
        internal TopologyManager topology;
        internal Canvas canvas;

        private ActivityDispatcher dispatch;
        private ActivityHudFactory hud;
        private ScenarioLoader scenarios;

        /// <summary>Despachador de actividades (creado perezosamente).</summary>
        internal ActivityDispatcher Dispatch => dispatch ?? (dispatch = new ActivityDispatcher(this));

        /// <summary>Fabrica del HUD de simulacion (creada perezosamente).</summary>
        internal ActivityHudFactory Hud => hud ?? (hud = new ActivityHudFactory(this));

        /// <summary>Cargador de escenarios (creado perezosamente).</summary>
        internal ScenarioLoader Scenarios => scenarios ?? (scenarios = new ScenarioLoader(this));

        private void Start()
        {
            topology = UnityEngine.Object.FindAnyObjectByType<TopologyManager>();
            canvas = UnityEngine.Object.FindAnyObjectByType<Canvas>();
        }

        /// <summary>
        /// Almacena el protocolo de enrutamiento dinamico seleccionado (RIP, OSPF o EIGRP).
        /// </summary>
        /// <param name="protocol">Nombre del protocolo ("RIP", "OSPF" o "EIGRP").</param>
        public void SetSelectedProtocol(string protocol)
        {
            selectedProtocol = protocol;
        }

        private void EnsureTopology()
        {
            if (topology == null) topology = UnityEngine.Object.FindAnyObjectByType<TopologyManager>();
        }

        /// <summary>
        /// Inicia una actividad por su indice. Delega en <see cref="ActivityDispatcher.SelectActivity"/>:
        /// destruye el panel de actividades, crea el HUD de simulacion y agrega los componentes
        /// especificos segun la actividad elegida.
        /// </summary>
        /// <param name="activityIndex">Indice de la actividad (0: BuildTopology, 1: FindFault,
        /// 2: RoutingTables, 3: BestRoute, 4: StaticRouting, 5: DynamicRouting, 6: Escenarios).</param>
        public void SelectActivity(int activityIndex)
        {
            Dispatch.SelectActivity(activityIndex);
        }

        /// <summary>
        /// Inicia una simulacion libre. Delega en <see cref="ActivityDispatcher.StartSimulation"/>:
        /// destruye el menu principal, crea el HUD de simulacion y los componentes base
        /// (BuildTopologyActivity, SimulationControls, ScoringSystem).
        /// </summary>
        /// <param name="canvasTransform">Transform del canvas donde se crea el HUD.</param>
        public void StartSimulation(Transform canvasTransform)
        {
            Dispatch.StartSimulation(canvasTransform);
        }

        /// <summary>
        /// Muestra el panel de conectividad independiente. Delega en
        /// <see cref="ActivityDispatcher.ShowConnectivityPanel"/>: crea los gestores si no
        /// existen y configura el ConnectivityTestPanel con referencias a los textos UI.
        /// </summary>
        /// <param name="canvasTransform">Transform del canvas donde se crea el panel.</param>
        public void ShowConnectivityPanel(Transform canvasTransform)
        {
            Dispatch.ShowConnectivityPanel(canvasTransform);
        }

        /// <summary>
        /// Carga un escenario preconfigurado. Delega en <see cref="ScenarioLoader.LoadScenario"/>:
        /// configura los gestores, crea el HUD de simulacion, inicia la sesion de puntuacion
        /// y muestra la informacion del escenario.
        /// </summary>
        /// <param name="scenarioIndex">Indice del escenario en la lista de PredefinedScenarios.</param>
        private void LoadScenario(int scenarioIndex)
        {
            Scenarios.LoadScenario(scenarioIndex);
        }

        /// <summary>
        /// Vuelve al menu principal delegando en la ruta unica de limpieza
        /// <see cref="SceneCleanupService.GoBackToMainMenu"/>; el callback
        /// recrea el menu via SceneSetup.CreateMainMenu.
        /// Interna (antes privada): los componentes extraidos (HUD, despacho, escenarios)
        /// la usan para sus callbacks de vuelta al menu.
        /// </summary>
        internal void GoBackToMainMenu()
        {
            var cleanup = SceneCleanupService.GetOrCreate();
            var ct = canvas != null ? canvas.transform : UnityEngine.Object.FindAnyObjectByType<Canvas>()?.transform;
            cleanup.GoBackToMainMenu(ct, () => {
                var ss = UnityEngine.Object.FindAnyObjectByType<SceneSetup>();
                if (ss != null && ct != null) ss.CreateMainMenu(ct);
            });
        }
    }
}
