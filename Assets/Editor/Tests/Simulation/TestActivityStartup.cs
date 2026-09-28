using System.Reflection;
using NUnit.Framework;
using SimRedes.Network;
using SimRedes.Simulation;
using UnityEngine;

namespace SimRedes.Simulation
{
    /// <summary>
    /// Tests for <see cref="ActivityStartup"/> (arranque compartido de las actividades).
    /// Verifica que la resolucion del TopologyManager no cree singletons fantasma
    /// y que Start() de las actividades funcione con y sin manager en la escena.
    /// </summary>
    public class TestActivityStartup
    {
        private static readonly System.Type[] ActivityTypes =
        {
            typeof(RoutingTablesActivity),
            typeof(StaticRoutingActivity),
            typeof(DynamicRoutingActivity)
        };

        [SetUp]
        public void SetUp()
        {
            ClearTopologyManagers();
        }

        [TearDown]
        public void TearDown()
        {
            ClearTopologyManagers();
        }

        /// <summary>
        /// Destruye todos los TopologyManager de la escena y anula el singleton
        /// estatico para que ningun test herede estado de otro.
        /// </summary>
        private static void ClearTopologyManagers()
        {
            foreach (var tm in Object.FindObjectsByType<TopologyManager>(
                FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                Object.DestroyImmediate(tm.gameObject);
            }

            var field = typeof(TopologyManager).GetField("<Instance>k__BackingField",
                BindingFlags.Static | BindingFlags.NonPublic);
            if (field != null) field.SetValue(null, null);
        }

        [Test]
        public void ResolveTopologyManager_NoManagerInScene_ReturnsNull()
        {
            // Act
            var resolved = ActivityStartup.ResolveTopologyManager();

            // Assert
            Assert.IsNull(resolved, "Sin TopologyManager en escena debe retornar null");
        }

        [Test]
        public void ResolveTopologyManager_ManagerInScene_ReturnsIt()
        {
            // Arrange
            var managerGo = new GameObject("TopologyManager");
            var manager = managerGo.AddComponent<TopologyManager>();

            // Act
            var resolved = ActivityStartup.ResolveTopologyManager();

            // Assert
            Assert.AreSame(manager, resolved);
        }

        [Test]
        public void ResolveTopologyManager_NeverCreatesPhantomGameObject()
        {
            // Act
            ActivityStartup.ResolveTopologyManager();

            // Assert
            Assert.IsNull(GameObject.Find("TopologyManager"),
                "La resolucion no debe crear singletons fantasma");
        }

        [Test]
        public void Start_ActivitiesWithoutManager_DoNotCreatePhantomOrFail()
        {
            foreach (var type in ActivityTypes)
            {
                var go = new GameObject(type.Name);
                var component = (MonoBehaviour)go.AddComponent(type);
                var start = type.GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.IsNotNull(start, $"{type.Name} debe declarar Start()");

                Assert.DoesNotThrow(() => start.Invoke(component, null),
                    $"{type.Name}.Start() no debe fallar sin TopologyManager");

                Object.DestroyImmediate(go);
            }

            Assert.IsNull(GameObject.Find("TopologyManager"),
                "Start() no debe crear un TopologyManager fantasma");
        }

        [Test]
        public void Start_WithExistingManager_AssignsItToTheActivity()
        {
            // Arrange
            var managerGo = new GameObject("TopologyManager");
            var manager = managerGo.AddComponent<TopologyManager>();

            var go = new GameObject("RoutingTablesActivity");
            var activity = go.AddComponent<RoutingTablesActivity>();
            var start = typeof(RoutingTablesActivity).GetMethod("Start",
                BindingFlags.Instance | BindingFlags.NonPublic);

            // Act
            start.Invoke(activity, null);

            // Assert
            var field = typeof(RoutingTablesActivity).GetField("topologyManager",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field);
            Assert.AreSame(manager, field.GetValue(activity),
                "Start() debe asignar el TopologyManager de la escena");

            Object.DestroyImmediate(go);
        }
    }
}
