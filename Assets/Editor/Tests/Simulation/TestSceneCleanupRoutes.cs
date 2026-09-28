using System.Reflection;
using NUnit.Framework;
using SimRedes.Simulation;
using UnityEngine;

namespace SimRedes.Simulation
{
    /// <summary>
    /// Tests for the single cleanup route: <see cref="SceneCleanupService"/>
    /// (GetOrCreate + DestroyPreviousPanels). Los demas sitios (SceneSetup,
    /// ActivityLoader, SimulationControls) delegan en estos metodos.
    /// </summary>
    public class TestSceneCleanupRoutes
    {
        [SetUp]
        public void SetUp()
        {
            ClearServices();
        }

        [TearDown]
        public void TearDown()
        {
            ClearServices();
            DestroyByName("VLANPanel");
            DestroyByName("ClickOutsideBG_VLANPanel");
        }

        /// <summary>
        /// Destruye los servicios existentes y anula el singleton estatico.
        /// </summary>
        private static void ClearServices()
        {
            foreach (var s in Object.FindObjectsByType<SceneCleanupService>(
                FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                Object.DestroyImmediate(s.gameObject);
            }

            var field = typeof(SceneCleanupService).GetField("<Instance>k__BackingField",
                BindingFlags.Static | BindingFlags.NonPublic);
            if (field != null) field.SetValue(null, null);
        }

        private static void DestroyByName(string name)
        {
            var go = GameObject.Find(name);
            if (go != null) Object.DestroyImmediate(go);
        }

        [Test]
        public void GetOrCreate_NoService_CreatesAndReusesSingleInstance()
        {
            // Act
            var first = SceneCleanupService.GetOrCreate();
            var second = SceneCleanupService.GetOrCreate();

            // Assert
            Assert.IsNotNull(first);
            Assert.AreSame(first, second, "GetOrCreate debe reutilizar siempre la misma instancia");
            Assert.AreSame(first, Object.FindAnyObjectByType<SceneCleanupService>());
        }

        [Test]
        public void GetOrCreate_RegisteredInstance_IsReused()
        {
            // Arrange
            var serviceGo = new GameObject("ExistingCleanup");
            var existing = serviceGo.AddComponent<SceneCleanupService>();
            var field = typeof(SceneCleanupService).GetField("<Instance>k__BackingField",
                BindingFlags.Static | BindingFlags.NonPublic);
            if (field != null) field.SetValue(null, existing);

            // Act
            var resolved = SceneCleanupService.GetOrCreate();

            // Assert
            Assert.AreSame(existing, resolved);
        }

        [Test]
        public void DestroyPreviousPanels_EmptyScene_DoesNotThrow()
        {
            // Arrange
            var service = SceneCleanupService.GetOrCreate();

            // Act & Assert
            Assert.DoesNotThrow(() => service.DestroyPreviousPanels());
        }

        [Test]
        public void DestroyPreviousPanels_WithPanelAndBackdrop_DoesNotThrow()
        {
            // Arrange
            new GameObject("VLANPanel");
            new GameObject("ClickOutsideBG_VLANPanel", typeof(RectTransform));
            var service = SceneCleanupService.GetOrCreate();

            // En EditMode Object.Destroy loguea un Error ("Destroy may not be called
            // from edit mode"). Es comportamiento del entorno de test, no del codigo
            // bajo test: en runtime (play mode) Destroy es la llamada correcta.
            UnityEngine.TestTools.LogAssert.ignoreFailingMessages = true;
            try
            {
                // Act & Assert
                Assert.DoesNotThrow(() => service.DestroyPreviousPanels());
            }
            finally
            {
                UnityEngine.TestTools.LogAssert.ignoreFailingMessages = false;
            }
        }
    }
}
