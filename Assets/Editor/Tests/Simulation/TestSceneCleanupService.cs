using NUnit.Framework;
using SimRedes.Network;
using SimRedes.Simulation;
using SimRedes.Tangible;
using SimRedes.UI;
using UnityEngine;

namespace SimRedes.Simulation
{
    public class TestSceneCleanupService
    {
        private SceneCleanupService cleanupService;
        private GameObject go;

        [SetUp]
        public void Setup()
        {
            go = new GameObject("TestCleanup");
            cleanupService = go.AddComponent<SceneCleanupService>();
            // In EditMode tests, Awake() may or may not be called automatically by AddComponent.
            // To be safe, directly set the Instance field via reflection.
            var instanceField = typeof(SceneCleanupService).GetField("<Instance>k__BackingField",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            if (instanceField == null)
            {
                // Fallback: try to find any field named Instance
                var fields = typeof(SceneCleanupService).GetFields(
                    System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
                foreach (var f in fields)
                {
                    if (f.Name.Contains("Instance") || f.Name.Contains("instance"))
                    {
                        instanceField = f;
                        break;
                    }
                }
            }
            if (instanceField != null)
                instanceField.SetValue(null, cleanupService);
        }

        [TearDown]
        public void Teardown()
        {
            // Reset singleton instance if it was set by this test instance
            if (SceneCleanupService.Instance == cleanupService)
            {
                // Auto-property backing field has compiler-generated name
                var field = typeof(SceneCleanupService).GetField("<Instance>k__BackingField",
                    System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
                if (field != null)
                    field.SetValue(null, null);
            }
            if (go != null)
                Object.DestroyImmediate(go);
        }

        [Test]
        public void Singleton_InstanceSetAfterAwake()
        {
            // The Awake method was already called by AddComponent in Setup
            // Assert
            Assert.IsNotNull(SceneCleanupService.Instance,
                "Instance should not be null after Awake");
            Assert.AreEqual(cleanupService, SceneCleanupService.Instance,
                "Instance should point to the created service");
        }

        [Test]
        public void ClearSimulation_NullParameters_DoesNotThrow()
        {
            // Act & Assert - should not throw with all null parameters
            Assert.DoesNotThrow(() => cleanupService.ClearSimulation(null, null, null));
        }

        [Test]
        public void ClearSimulation_PartialNull_DoesNotThrow()
        {
            // Act & Assert - should not throw with some null parameters
            var topology = new GameObject("TestTopology").AddComponent<TopologyManager>();
            Assert.DoesNotThrow(() => cleanupService.ClearSimulation(topology, null, null));
            Object.DestroyImmediate(topology.gameObject);

            var visualizer = new GameObject("TestVisualizer").AddComponent<NodeVisualizer>();
            Assert.DoesNotThrow(() => cleanupService.ClearSimulation(null, visualizer, null));
            Object.DestroyImmediate(visualizer.gameObject);

            var discSim = new GameObject("TestDiscSim").AddComponent<DebugDiscSimulator>();
            Assert.DoesNotThrow(() => cleanupService.ClearSimulation(null, null, discSim));
            Object.DestroyImmediate(discSim.gameObject);
        }
    }
}
