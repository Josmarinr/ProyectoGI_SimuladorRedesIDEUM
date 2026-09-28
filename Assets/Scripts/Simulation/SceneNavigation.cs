using UnityEngine;
using SimRedes.UI;
using UIComp = SimRedes.UI.UIComponents;

namespace SimRedes.Simulation
{
    /// <summary>
    /// Navegacion de menu y paneles de la escena: crea/destruye el menu principal,
    /// muestra los paneles de actividades, conectividad, instrucciones y leyenda de
    /// discos, y orquesta el arranque de la simulacion desde el menu.
    /// Extraido de <see cref="SceneSetup"/> (tarea C5) sin cambios de comportamiento;
    /// delega la composicion de managers en <see cref="SceneBootstrap"/>.
    /// </summary>
    public class SceneNavigation
    {
        private readonly SceneSetup owner;

        /// <summary>
        /// Crea el navegador de menu asociado a la fachada <see cref="SceneSetup"/> dueña,
        /// cuyos campos aportan el canvas y el ActivityLoader cacheados.
        /// </summary>
        /// <param name="owner">Fachada <see cref="SceneSetup"/> que usa como contexto.</param>
        public SceneNavigation(SceneSetup owner)
        {
            this.owner = owner;
        }

        /// <summary>
        /// Crea el menu principal limpiando paneles previos y delegando en
        /// <see cref="UIPanelFactory.CreateMainMenu"/> con sus callbacks.
        /// Punto de entrada publico para recrear el menu desde otros componentes.
        /// </summary>
        /// <param name="ct">Transform del Canvas donde se instancia el menu.</param>
        public void CreateMainMenu(Transform ct)
        {
            // Limpiar cualquier panel que haya quedado abierto antes de crear el menu
            DestroyPreviousPanels();
            UIPanelFactory.CreateMainMenu(ct,
                () => StartSimulation(),
                () => ShowActivities(ct),
                () => ShowConnectivity(ct),
                () => ShowInstructions(ct),
                () => ShowDiscLegend(ct),
                () => owner.ExitApplication());
        }

        /// <summary>
        /// Inicia la simulacion: destruye el menu, configura managers,
        /// crea el visualizador, se suscribe a eventos y notifica al ActivityLoader.
        /// </summary>
        public void StartSimulation()
        {
            DestroyMainMenu();
            owner.SetupManagers();
            owner.CreateVisualizer(owner.canvasTransform);
            owner.SubscribeToTopologyEvents();
            owner.Bootstrap.CacheReferences();
            if (owner.activityLoader != null) owner.activityLoader.StartSimulation(owner.canvasTransform);
        }

        /// <summary>
        /// Muestra el panel de seleccion de actividades. Al elegir una,
        /// configura managers y delega en <see cref="ActivityLoader.SelectActivity"/>.
        /// </summary>
        /// <param name="ct">Transform del Canvas donde instanciar el panel.</param>
        public void ShowActivities(Transform ct)
        {
            DestroyMainMenu();
            UIPanelFactory.CreateActivitiesPanel(ct,
                (index) => {
                    owner.SetupManagers();
                    owner.CreateVisualizer(owner.canvasTransform);
                    owner.SubscribeToTopologyEvents();
                    owner.Bootstrap.CacheReferences();
                    if (owner.activityLoader != null)
                        owner.activityLoader.SelectActivity(index);
                },
                () => { CreateMainMenu(ct); });
        }

        /// <summary>
        /// Muestra el panel de prueba de conectividad. Inicializa managers
        /// y visualizador si aun no estan listos.
        /// </summary>
        /// <param name="ct">Transform del Canvas donde instanciar el panel.</param>
        public void ShowConnectivity(Transform ct)
        {
            DestroyMainMenu();
            owner.SetupManagers();
            owner.CreateVisualizer(owner.canvasTransform);
            owner.SubscribeToTopologyEvents();
            owner.Bootstrap.CacheReferences();
            if (owner.activityLoader != null) owner.activityLoader.ShowConnectivityPanel(ct);
        }

        /// <summary>
        /// Muestra el panel de instrucciones de uso del simulador.
        /// </summary>
        /// <param name="ct">Transform del Canvas donde instanciar el panel.</param>
        public void ShowInstructions(Transform ct)
        {
            DestroyMainMenu();
            UIPanelFactory.CreateInstructionsPanel(ct,
                () => { CreateMainMenu(ct); });
        }

        /// <summary>
        /// Muestra la leyenda de discos fisicos y virtuales con sus tipos y colores.
        /// </summary>
        /// <param name="ct">Transform del Canvas donde instanciar el panel.</param>
        public void ShowDiscLegend(Transform ct)
        {
            DestroyMainMenu();
            UIPanelFactory.CreateDiscLegendPanel(ct,
                () => { DestroyPreviousPanels(); CreateMainMenu(ct); });
        }

        /// <summary>
        /// Elimina solo el menu principal al hacer clic en una opcion (INICIAR, ACTIVIDADES, etc.),
        /// conservando el MenuNavigator para que los sub-paneles tengan animaciones.
        /// </summary>
        private void DestroyMainMenu()
        {
            GameObject panel = GameObject.Find("MainMenuPanel");
            if (panel != null)
            {
                // B4: liberar los Sprite del menu antes de destruirlo
                UIComp.SafeDestroyPanelSprites(panel);
                GameObject.Destroy(panel);
            }

            GameObject hudPanel = GameObject.Find("TopologyInfoPanel");
            if (hudPanel != null)
            {
                UIComp.SafeDestroyPanelSprites(hudPanel);
                GameObject.Destroy(hudPanel);
            }

            // Usar DestroyImmediate para evitar conflictos de singleton con destruccion diferida
            // al recrear el menu en el mismo frame (ej: desde el panel de Instrucciones)
            var mm = UnityEngine.Object.FindAnyObjectByType<MainMenuManager>();
            if (mm != null) GameObject.DestroyImmediate(mm.gameObject);

            // NO destruir MenuNavigator — se reutiliza para dar animaciones a los sub-paneles
            Debug.Log("[SceneSetup] Menu principal destruido (navigator conservado)");
        }

        /// <summary>
        /// Elimina cualquier panel de actividades, instrucciones o conectividad
        /// que haya quedado abierto al volver al menu principal.
        /// Delega en la ruta unica de limpieza: <see cref="SceneCleanupService.DestroyPreviousPanels"/>.
        /// Conserva MenuNavigator para que el nuevo menu tenga animaciones.
        /// </summary>
        private void DestroyPreviousPanels()
        {
            SceneCleanupService.GetOrCreate().DestroyPreviousPanels();
        }
    }
}
