using System.Collections.Generic;
using System.Net;
using NUnit.Framework;
using SimRedes.Simulation;
using UnityEngine;

namespace Tests.EditMode.Simulation
{
    public class TestPredefinedScenarios
    {
        private GameObject go;
        private PredefinedScenarios scenarios;

        [SetUp]
        public void SetUp()
        {
            go = new GameObject();
            scenarios = go.AddComponent<PredefinedScenarios>();
            // In EditMode tests, Awake() may or may not be called automatically by AddComponent.
            // Set the singleton Instance directly and call InitializeScenarios via reflection.
            // This is more robust than invoking Awake(), which may conflict with a stale Instance
            // from a previous test that wasn't properly cleaned up.
            var instanceField = typeof(PredefinedScenarios).GetField("<Instance>k__BackingField",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            if (instanceField == null)
            {
                var fields = typeof(PredefinedScenarios).GetFields(
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
                instanceField.SetValue(null, scenarios);
            
            // Initialize scenarios by directly calling the private method
            var initMethod = typeof(PredefinedScenarios).GetMethod("InitializeScenarios",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (initMethod != null)
                initMethod.Invoke(scenarios, null);
        }

        [TearDown]
        public void TearDown()
        {
            // Reset singleton instance if it was set by this test
            var instanceField = typeof(PredefinedScenarios).GetField("<Instance>k__BackingField",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            if (instanceField != null && instanceField.GetValue(null) == scenarios)
                instanceField.SetValue(null, null);
            
            if (go != null)
                Object.DestroyImmediate(go);
        }

        [Test]
        public void GetScenarioCount_ReturnsFive()
        {
            int count = scenarios.GetScenarioCount();
            Assert.AreEqual(5, count);
        }

        [Test]
        public void GetScenario_ValidIndex_ReturnsCorrectScenario()
        {
            var scenario = scenarios.GetScenario(0);
            Assert.IsNotNull(scenario);
            Assert.AreEqual("Estrella Simple", scenario.name);
        }

        [Test]
        public void GetScenario_InvalidIndex_ReturnsNull()
        {
            Assert.IsNull(scenarios.GetScenario(-1));
            Assert.IsNull(scenarios.GetScenario(99));
        }

        [Test]
        public void GetScenariosByDifficulty_Basico_ReturnsCorrectCount()
        {
            var basicoScenarios = scenarios.GetScenariosByDifficulty(PredefinedScenarios.ScenarioDifficulty.Basico);
            Assert.AreEqual(1, basicoScenarios.Count);
        }

        [Test]
        public void GetScenariosByDifficulty_Intermedio_ReturnsCorrectCount()
        {
            var intermedioScenarios = scenarios.GetScenariosByDifficulty(PredefinedScenarios.ScenarioDifficulty.Intermedio);
            Assert.AreEqual(3, intermedioScenarios.Count);
        }

        [Test]
        public void GetScenariosByDifficulty_Avanzado_ReturnsCorrectCount()
        {
            var avanzadoScenarios = scenarios.GetScenariosByDifficulty(PredefinedScenarios.ScenarioDifficulty.Avanzado);
            Assert.AreEqual(1, avanzadoScenarios.Count);
        }

        [Test]
        public void AllScenarios_HaveNameAndDescription()
        {
            var all = scenarios.GetScenarios();
            foreach (var s in all)
            {
                Assert.IsFalse(string.IsNullOrEmpty(s.name), $"Scenario name is null or empty");
                Assert.IsFalse(string.IsNullOrEmpty(s.description), $"Scenario '{s.name}' has empty description");
            }
        }

        [Test]
        public void AllScenarios_HaveAtLeastTwoDevices()
        {
            var all = scenarios.GetScenarios();
            foreach (var s in all)
            {
                Assert.GreaterOrEqual(s.devices.Count, 2,
                    $"Scenario '{s.name}' has fewer than 2 devices ({s.devices.Count})");
            }
        }

        [Test]
        public void AllScenarios_LinksReferenceValidDevices()
        {
            var all = scenarios.GetScenarios();
            foreach (var s in all)
            {
                int deviceCount = s.devices.Count;
                foreach (var link in s.links)
                {
                    Assert.GreaterOrEqual(link.fromIndex, 0,
                        $"Scenario '{s.name}' has link with negative fromIndex ({link.fromIndex})");
                    Assert.Less(link.fromIndex, deviceCount,
                        $"Scenario '{s.name}' has link fromIndex {link.fromIndex} >= device count {deviceCount}");
                    Assert.GreaterOrEqual(link.toIndex, 0,
                        $"Scenario '{s.name}' has link with negative toIndex ({link.toIndex})");
                    Assert.Less(link.toIndex, deviceCount,
                        $"Scenario '{s.name}' has link toIndex {link.toIndex} >= device count {deviceCount}");
                }
            }
        }

        [Test]
        public void AllScenarios_IPConfigsReferenceValidDevices()
        {
            var all = scenarios.GetScenarios();
            foreach (var s in all)
            {
                int deviceCount = s.devices.Count;
                foreach (var ipc in s.ipConfigurations)
                {
                    Assert.GreaterOrEqual(ipc.deviceIndex, 0,
                        $"Scenario '{s.name}' has IPConfig with negative deviceIndex ({ipc.deviceIndex})");
                    Assert.Less(ipc.deviceIndex, deviceCount,
                        $"Scenario '{s.name}' has IPConfig deviceIndex {ipc.deviceIndex} >= device count {deviceCount}");
                }
            }
        }

        [Test]
        public void AllScenarios_IPAddressesAreValid()
        {
            var all = scenarios.GetScenarios();
            foreach (var s in all)
            {
                foreach (var ipc in s.ipConfigurations)
                {
                    Assert.IsTrue(IPAddress.TryParse(ipc.ip, out _),
                        $"Scenario '{s.name}' has invalid IP '{ipc.ip}' for device {ipc.deviceIndex}");
                    Assert.IsTrue(IPAddress.TryParse(ipc.mask, out _),
                        $"Scenario '{s.name}' has invalid mask '{ipc.mask}' for device {ipc.deviceIndex}");
                }
            }
        }

        [Test]
        public void AllScenarios_HaveObjectivesAndHints()
        {
            var all = scenarios.GetScenarios();
            foreach (var s in all)
            {
                Assert.IsNotNull(s.objectives, $"Scenario '{s.name}' has null objectives");
                Assert.Greater(s.objectives.Length, 0,
                    $"Scenario '{s.name}' has no objectives");
                Assert.IsNotNull(s.hints, $"Scenario '{s.name}' has null hints");
                Assert.Greater(s.hints.Length, 0,
                    $"Scenario '{s.name}' has no hints");
            }
        }

        [Test]
        public void GetScenarios_ReturnsCopy()
        {
            var list1 = scenarios.GetScenarios();
            var list2 = scenarios.GetScenarios();
            Assert.AreNotSame(list1, list2, "GetScenarios should return a new list each time");
        }
    }
}
