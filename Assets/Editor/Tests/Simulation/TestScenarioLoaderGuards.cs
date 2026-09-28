using System.Reflection;
using NUnit.Framework;
using SimRedes.Simulation;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests.EditMode.Simulation
{
    /// <summary>
    /// Tests de C6a: guards de singletons nulos en ScenarioLoader.LoadScenario.
    /// AddComponent no invoca Awake en EditMode, asi que PredefinedScenarios.Instance
    /// puede seguir siendo null aunque el componente ya exista; antes de C6 ese camino
    /// terminaba en NullReferenceException en GetScenario.
    /// </summary>
    public class TestScenarioLoaderGuards
    {
        private GameObject loaderGo;
        private ActivityLoader loader;

        [SetUp]
        public void SetUp()
        {
            // Asegurar que NO haya singleton vivo ni escenario residual de otros tests
            var existingScenarios = Object.FindAnyObjectByType<PredefinedScenarios>();
            if (existingScenarios != null)
                Object.DestroyImmediate(existingScenarios.gameObject);
            ResetSingleton<PredefinedScenarios>();

            var existingSetup = Object.FindAnyObjectByType<SceneSetup>();
            if (existingSetup != null)
                Object.DestroyImmediate(existingSetup.gameObject);

            DestroyIfExists("Canvas");
            DestroyIfExists("GameManager");
            DestroyIfExists("TopologyInfoPanel");
            DestroyIfExists("ScenariosPanel");
            DestroyIfExists("ScenarioInfoPanel");

            loaderGo = new GameObject("ActivityLoader");
            loader = loaderGo.AddComponent<ActivityLoader>();
        }

        [TearDown]
        public void TearDown()
        {
            if (loaderGo != null) Object.DestroyImmediate(loaderGo);
            DestroyIfExists("GameManager");
            DestroyIfExists("TopologyInfoPanel");
            DestroyIfExists("ScenarioInfoPanel");
            ResetSingleton<PredefinedScenarios>();
        }

        [Test]
        public void LoadScenario_SinPredefinedScenarios_LogErrorYNoLanza()
        {
            // Arrange — singleton nulo (caso EditMode / antes del Awake)
            var scenarioLoader = new ScenarioLoader(loader);

            // Act & Assert — el guard reporta el error en vez de lanzar NRE
            LogAssert.Expect(LogType.Error, "[Scenarios] PredefinedScenarios.Instance es null");
            Assert.DoesNotThrow(() => scenarioLoader.LoadScenario(0));

            Assert.IsNull(GameObject.Find("ScenarioInfoPanel"),
                "Sin PredefinedScenarios no debe construir el panel de informacion");
        }

        private static void DestroyIfExists(string objectName)
        {
            var obj = GameObject.Find(objectName);
            if (obj != null) Object.DestroyImmediate(obj);
        }

        private static void ResetSingleton<T>() where T : MonoBehaviour
        {
            var backingField = typeof(T).GetField(
                "<Instance>k__BackingField",
                BindingFlags.Static | BindingFlags.NonPublic);
            backingField?.SetValue(null, null);
        }
    }
}
