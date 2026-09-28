using System.Reflection;
using NUnit.Framework;
using SimRedes.Network;
using SimRedes.Simulation;
using UnityEngine;

namespace Tests.EditMode.Simulation
{
    /// <summary>
    /// Tests de C6b: BuildTopologyActivity debe desuscribir TODOS sus handlers
    /// de TopologyManager en OnDestroy. Antes de C6, OnNodeAdded/OnNodeRemoved/
    /// OnLinkAdded se suscribian con lambdas que nunca se removian, dejando
    /// delegates huerfanos en el publicador que retenian el componente destruido.
    /// </summary>
    public class TestBuildTopologyActivity
    {
        private static readonly BindingFlags PrivateInstance =
            BindingFlags.Instance | BindingFlags.NonPublic;

        private GameObject topologyGo;
        private TopologyManager topology;
        private GameObject activityGo;
        private BuildTopologyActivity activity;

        [SetUp]
        public void SetUp()
        {
            // Estado limpio del singleton: AddComponent no invoca Awake en EditMode,
            // y un Instance colgado de otro test haria que Awake realice Destroy.
            // Tambien se elimina cualquier TopologyManager residual para que
            // Start() suscriba sobre EL manager de este test y no sobre otro.
            var existingTopology = Object.FindAnyObjectByType<TopologyManager>();
            if (existingTopology != null)
                Object.DestroyImmediate(existingTopology.gameObject);
            ResetTopologySingleton();

            topologyGo = new GameObject("TopologyManager");
            topology = topologyGo.AddComponent<TopologyManager>();
            InvokePrivate(topology, "Awake");

            activityGo = new GameObject("BuildTopologyActivity");
            activity = activityGo.AddComponent<BuildTopologyActivity>();
        }

        [TearDown]
        public void TearDown()
        {
            if (activityGo != null) Object.DestroyImmediate(activityGo);
            if (topologyGo != null) Object.DestroyImmediate(topologyGo);
            ResetTopologySingleton();
        }

        [Test]
        public void Start_SuscribeLosCuatroHandlers_DeTopologia()
        {
            // Act
            InvokePrivate(activity, "Start");

            // Assert
            Assert.AreEqual(1, HandlerCount("OnTopologyChanged"),
                "Start debe suscribir OnTopologyChanged");
            Assert.AreEqual(1, HandlerCount("OnNodeAdded"),
                "Start debe suscribir OnNodeAdded (antes via lambda huerfana)");
            Assert.AreEqual(1, HandlerCount("OnNodeRemoved"),
                "Start debe suscribir OnNodeRemoved (antes via lambda huerfana)");
            Assert.AreEqual(1, HandlerCount("OnLinkAdded"),
                "Start debe suscribir OnLinkAdded (antes via lambda huerfana)");
        }

        [Test]
        public void OnDestroy_DesuscribeTodosLosHandlers()
        {
            // Arrange
            InvokePrivate(activity, "Start");

            // Act
            InvokePrivate(activity, "OnDestroy");

            // Assert — ninguno puede quedar colgado en el publicador
            Assert.AreEqual(0, HandlerCount("OnTopologyChanged"),
                "OnDestroy debe desuscribir OnTopologyChanged");
            Assert.AreEqual(0, HandlerCount("OnNodeAdded"),
                "OnDestroy debe desuscribir OnNodeAdded");
            Assert.AreEqual(0, HandlerCount("OnNodeRemoved"),
                "OnDestroy debe desuscribir OnNodeRemoved");
            Assert.AreEqual(0, HandlerCount("OnLinkAdded"),
                "OnDestroy debe desuscribir OnLinkAdded");
        }

        [Test]
        public void OnDestroy_SinStart_NoLanzaNiDejaSuscripciones()
        {
            // Act & Assert — el componente puede destruirse antes de Start
            Assert.DoesNotThrow(() => InvokePrivate(activity, "OnDestroy"));

            Assert.AreEqual(0, HandlerCount("OnTopologyChanged"));
            Assert.AreEqual(0, HandlerCount("OnNodeAdded"));
            Assert.AreEqual(0, HandlerCount("OnNodeRemoved"));
            Assert.AreEqual(0, HandlerCount("OnLinkAdded"));
        }

        /// <summary>
        /// Cuenta los handlers suscritos a un evento field-like de TopologyManager
        /// leyendo su backing field privado (la cuenta es observable directo: los
        /// delegates huerfanos viven ahi hasta que el publicador los limpia).
        /// </summary>
        private int HandlerCount(string eventName)
        {
            var field = typeof(TopologyManager).GetField(eventName, PrivateInstance);
            Assert.IsNotNull(field, $"Backing field del evento {eventName} no encontrado");
            var handler = field.GetValue(topology) as System.Delegate;
            return handler == null ? 0 : handler.GetInvocationList().Length;
        }

        private static void InvokePrivate(object target, string methodName)
        {
            var method = target.GetType().GetMethod(methodName, PrivateInstance);
            Assert.IsNotNull(method, $"Metodo {methodName} no encontrado");
            method.Invoke(target, null);
        }

        private static void ResetTopologySingleton()
        {
            var backingField = typeof(TopologyManager).GetField(
                "<Instance>k__BackingField",
                BindingFlags.Static | BindingFlags.NonPublic);
            backingField?.SetValue(null, null);
        }
    }
}
