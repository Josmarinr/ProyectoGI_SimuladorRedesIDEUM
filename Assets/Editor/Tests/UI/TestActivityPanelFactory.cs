using NUnit.Framework;
using SimRedes.UI;
using SimRedes.Simulation;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Tests.EditMode.UI
{
    public class TestActivityPanelFactory
    {
        private GameObject canvasGo;
        private Canvas canvas;
        private Font font;
        private List<PredefinedScenarios.NetworkScenario> dummyScenarios;

        [SetUp]
        public void SetUp()
        {
            canvasGo = new GameObject("TestCanvas");
            canvas = canvasGo.AddComponent<Canvas>();
            font = UIPanelFactory.GetFont();

            dummyScenarios = new List<PredefinedScenarios.NetworkScenario>
            {
                new PredefinedScenarios.NetworkScenario
                {
                    name = "Test Scenario",
                    description = "A test scenario",
                    difficulty = PredefinedScenarios.ScenarioDifficulty.Basico,
                    objectives = new string[] { "Objective 1" },
                    hints = new string[] { "Hint 1" },
                    devices = new List<PredefinedScenarios.DeviceConfig>(),
                    links = new List<PredefinedScenarios.LinkConfig>(),
                    faults = new List<PredefinedScenarios.FaultConfig>()
                }
            };
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(canvasGo);
        }

        [Test]
        public void CreateBuildTopologyInfoPanel_ReturnsPanel()
        {
            GameObject panel = ActivityPanelFactory.CreateBuildTopologyInfoPanel(canvas.transform);

            Assert.IsNotNull(panel);
            Assert.AreEqual("BuildTopologyInfoPanel", panel.name);
            Assert.IsNotNull(panel.transform.Find("CloseBtn"));
        }

        [Test]
        public void CreateFindFaultPanel_WithCallbacks_ReturnsPanel()
        {
            GameObject panel = ActivityPanelFactory.CreateFindFaultPanel(
                canvas.transform,
                onDiscClick: null,
                onNext: null,
                onPrev: null,
                onBack: null
            );

            Assert.IsNotNull(panel);
            Assert.AreEqual("FindFaultPanel", panel.name);
            Assert.IsNotNull(panel.transform.Find("TactileDiscBtn"));
            Assert.IsNotNull(panel.transform.Find("BackBtn"));
        }

        [Test]
        public void CreateBestRoutePanel_WithCallbacks_ReturnsPanel()
        {
            GameObject panel = ActivityPanelFactory.CreateBestRoutePanel(
                canvas.transform,
                onBack: null
            );

            Assert.IsNotNull(panel);
            Assert.AreEqual("BestRoutePanel", panel.name);
            Assert.IsNotNull(panel.transform.Find("NextBtn"));
            Assert.IsNotNull(panel.transform.Find("BackBtn"));
        }

        [Test]
        public void CreateRoutingTablesPanel_WithCallbacks_ReturnsPanel()
        {
            GameObject panel = ActivityPanelFactory.CreateRoutingTablesPanel(
                canvas.transform,
                onBack: null
            );

            Assert.IsNotNull(panel);
            Assert.AreEqual("RoutingTablesPanel", panel.name);
            Assert.IsNotNull(panel.transform.Find("RefreshBtn"));
            Assert.IsNotNull(panel.transform.Find("BackBtn"));
        }

        [Test]
        public void CreateStaticRoutingPanel_WithCallbacks_ReturnsPanel()
        {
            GameObject panel = ActivityPanelFactory.CreateStaticRoutingPanel(
                canvas.transform,
                onBack: null
            );

            Assert.IsNotNull(panel);
            Assert.AreEqual("StaticRoutingPanel", panel.name);
            Assert.IsNotNull(panel.transform.Find("ManualRouteBtn"));
            Assert.IsNotNull(panel.transform.Find("AddRouteBtn"));
            Assert.IsNotNull(panel.transform.Find("TestRouteBtn"));
            Assert.IsNotNull(panel.transform.Find("BackBtn"));
        }

        [Test]
        public void CreateDynamicRoutingPanel_WithCallbacks_ReturnsPanel()
        {
            GameObject panel = ActivityPanelFactory.CreateDynamicRoutingPanel(
                canvas.transform,
                onSelectRIP: null,
                onSelectOSPF: null,
                onStart: null,
                onStop: null,
                onClearRoutes: null,
                onViewRoutes: null,
                onBack: null,
                onSetNeighbor: null,
                onSetNetwork: null,
                onSetCost: null,
                onSetBW: null
            );

            Assert.IsNotNull(panel);
            Assert.AreEqual("DynamicRoutingPanel", panel.name);
            Assert.IsNotNull(panel.transform.Find("RIPBtn"));
            Assert.IsNotNull(panel.transform.Find("OSBFBtn"));
            Assert.IsNotNull(panel.transform.Find("StartBtn"));
            Assert.IsNotNull(panel.transform.Find("StopBtn"));
            Assert.IsNotNull(panel.transform.Find("ClearBtn"));
            Assert.IsNotNull(panel.transform.Find("ViewBtn"));
            Assert.IsNotNull(panel.transform.Find("BackBtn"));
        }

        [Test]
        public void CreateScenariosPanel_WithDummyData_ReturnsPanel()
        {
            GameObject panel = ActivityPanelFactory.CreateScenariosPanel(
                canvas.transform,
                dummyScenarios,
                onScenarioClick: null,
                onBack: null
            );

            Assert.IsNotNull(panel);
            Assert.AreEqual("ScenariosPanel", panel.name);
            Assert.IsNotNull(panel.transform.Find("BackBtn"));
        }

        [Test]
        public void CreateScenarioInfoPanel_WithDummyData_ReturnsPanel()
        {
            GameObject panel = ActivityPanelFactory.CreateScenarioInfoPanel(
                canvas.transform,
                dummyScenarios[0],
                onStart: null,
                onCancel: null
            );

            Assert.IsNotNull(panel);
            Assert.AreEqual("ScenarioInfoPanel", panel.name);
            Assert.IsNotNull(panel.transform.Find("StartScenarioBtn"));
            Assert.IsNotNull(panel.transform.Find("CancelBtn"));
        }
    }
}
