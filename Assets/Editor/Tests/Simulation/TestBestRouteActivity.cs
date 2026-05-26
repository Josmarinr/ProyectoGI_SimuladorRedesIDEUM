using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using SimRedes.Simulation;
using UnityEngine;

namespace SimRedes.Simulation
{
    public class TestBestRouteActivity
    {
        private BestRouteActivity activity;
        private GameObject go;

        [SetUp]
        public void Setup()
        {
            go = new GameObject("TestBestRoute");
            activity = go.AddComponent<BestRouteActivity>();

            // Call InitializeScenarios via reflection (it's private)
            var method = typeof(BestRouteActivity).GetMethod("InitializeScenarios",
                BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(method, "InitializeScenarios method not found");
            method.Invoke(activity, null);
        }

        [TearDown]
        public void Teardown()
        {
            if (go != null)
                Object.DestroyImmediate(go);
        }

        [TestCase("255.0.0.0", 8)]
        [TestCase("255.255.0.0", 16)]
        [TestCase("255.255.255.0", 24)]
        [TestCase("255.255.255.255", 32)]
        [TestCase("255.255.255.252", 30)]
        [TestCase("255.255.255.128", 25)]
        [TestCase("255.255.255.192", 26)]
        [TestCase("0.0.0.0", 0)]
        public void GetPrefixLength_AllMasks_ReturnsCorrectLength(string mask, int expected)
        {
            // Act
            int result = SimRedes.Network.IPValidation.GetPrefixLength(mask);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Scenarios_InitializedWithCorrectCount()
        {
            // Arrange
            var field = typeof(BestRouteActivity).GetField("scenarios",
                BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(field, "scenarios field not found");

            // Act
            var scenarios = field.GetValue(activity) as System.Collections.IList;

            // Assert
            Assert.IsNotNull(scenarios);
            Assert.AreEqual(4, scenarios.Count);
        }

        [Test]
        public void NextScenario_CyclesThroughScenarios()
        {
            // Arrange
            var indexField = typeof(BestRouteActivity).GetField("currentScenarioIndex",
                BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(indexField, "currentScenarioIndex field not found");

            // Initially -1 after InitializeScenarios (before any NextScenario call)
            int initialIndex = (int)indexField.GetValue(activity);
            Assert.AreEqual(-1, initialIndex, "Should start at -1 before any NextScenario call");

            // Act
            activity.NextScenario();
            int afterFirst = (int)indexField.GetValue(activity);

            activity.NextScenario();
            int afterSecond = (int)indexField.GetValue(activity);

            activity.NextScenario();
            int afterThird = (int)indexField.GetValue(activity);

            activity.NextScenario();
            int afterFourth = (int)indexField.GetValue(activity);

            // Fifth call should wrap around to 0
            activity.NextScenario();
            int afterWrap = (int)indexField.GetValue(activity);

            // Assert
            Assert.AreEqual(0, afterFirst, "First call should go to index 0");
            Assert.AreEqual(1, afterSecond, "Second call should go to index 1");
            Assert.AreEqual(2, afterThird, "Third call should go to index 2");
            Assert.AreEqual(3, afterFourth, "Fourth call should go to index 3");
            Assert.AreEqual(0, afterWrap, "Fifth call should wrap around to index 0");
        }

        [Test]
        public void EachScenario_CorrectIndexWithinRange()
        {
            // Arrange
            var field = typeof(BestRouteActivity).GetField("scenarios",
                BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(field, "scenarios field not found");
            var scenarios = field.GetValue(activity) as System.Collections.IList;
            Assert.IsNotNull(scenarios);

            // Act & Assert
            for (int i = 0; i < scenarios.Count; i++)
            {
                var scenario = scenarios[i];
                var correctIndexField = scenario.GetType().GetField("CorrectIndex",
                    BindingFlags.Public | BindingFlags.Instance);
                Assert.IsNotNull(correctIndexField, $"CorrectIndex field not found in scenario {i}");
                int correctIndex = (int)correctIndexField.GetValue(scenario);

                var optionsField = scenario.GetType().GetField("Options",
                    BindingFlags.Public | BindingFlags.Instance);
                Assert.IsNotNull(optionsField, $"Options field not found in scenario {i}");
                var options = optionsField.GetValue(scenario) as System.Collections.IList;
                Assert.IsNotNull(options, $"Options list is null in scenario {i}");

                Assert.IsTrue(correctIndex >= 0,
                    $"Scenario {i}: CorrectIndex ({correctIndex}) should be >= 0");
                Assert.IsTrue(correctIndex < options.Count,
                    $"Scenario {i}: CorrectIndex ({correctIndex}) should be < Options.Count ({options.Count})");
            }
        }

        [Test]
        public void EachScenario_HasAtLeastTwoOptions()
        {
            // Arrange
            var field = typeof(BestRouteActivity).GetField("scenarios",
                BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(field, "scenarios field not found");
            var scenarios = field.GetValue(activity) as System.Collections.IList;
            Assert.IsNotNull(scenarios);

            // Act & Assert
            for (int i = 0; i < scenarios.Count; i++)
            {
                var scenario = scenarios[i];
                var optionsField = scenario.GetType().GetField("Options",
                    BindingFlags.Public | BindingFlags.Instance);
                Assert.IsNotNull(optionsField, $"Options field not found in scenario {i}");
                var options = optionsField.GetValue(scenario) as System.Collections.IList;
                Assert.IsNotNull(options, $"Options list is null in scenario {i}");

                Assert.IsTrue(options.Count >= 2,
                    $"Scenario {i} has {options.Count} options, expected at least 2");
            }
        }
    }
}
