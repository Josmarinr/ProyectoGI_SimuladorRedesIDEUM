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

        [Test]
        public void CreateScenariosPanel_ScenarioItem_HasExpectedButtonColors()
        {
            GameObject panel = ActivityPanelFactory.CreateScenariosPanel(
                canvas.transform,
                dummyScenarios,
                onScenarioClick: null,
                onBack: null
            );

            Transform item = panel.transform.Find("ScenarioItem_0");
            Assert.IsNotNull(item);

            Button btn = item.GetComponent<Button>();
            Assert.IsNotNull(btn);

            // Colores explicitos del bloque manual de escenarios (sin derivar)
            ColorBlock colors = btn.colors;
            Assert.AreEqual(UIComponents.Colors.surfaceElevated, colors.normalColor);
            Assert.AreEqual(UIComponents.Colors.buttonHover, colors.highlightedColor);
            Assert.AreEqual(UIComponents.Colors.buttonNormal, colors.pressedColor);
            Assert.AreEqual(1f, colors.colorMultiplier);

            // Estados sin usar quedan en cero y sin transicion (ColorBlock a cero)
            Assert.AreEqual(new Color(0f, 0f, 0f, 0f), colors.disabledColor);
            Assert.AreEqual(new Color(0f, 0f, 0f, 0f), colors.selectedColor);
            Assert.AreEqual(0f, colors.fadeDuration, 0.0001f);
        }

        [Test]
        public void CreateScenariosPanel_ScenarioItem_HasExpectedChildren()
        {
            GameObject panel = ActivityPanelFactory.CreateScenariosPanel(
                canvas.transform,
                dummyScenarios,
                onScenarioClick: null,
                onBack: null
            );

            Transform item = panel.transform.Find("ScenarioItem_0");
            Assert.IsNotNull(item);
            Assert.AreEqual(3, item.childCount);
            Assert.IsNotNull(item.Find("Name").GetComponent<Text>());
            Assert.IsNotNull(item.Find("Difficulty").GetComponent<Text>());
            Assert.IsNotNull(item.Find("Description").GetComponent<Text>());
            Assert.IsNotNull(item.GetComponent<Image>());
            Assert.IsNotNull(item.GetComponent<Button>());
        }

        [Test]
        public void CreateDynamicRoutingPanel_InputFields_HaveExpectedStructure()
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

            // Campo con placeholder y sin valor: label inicial vacio, placeholder visible
            Transform neighbor = panel.transform.Find("NeighborInput");
            Assert.IsNotNull(neighbor);
            Assert.AreEqual(2, neighbor.childCount);

            Image bg = neighbor.GetComponent<Image>();
            Assert.IsNotNull(bg);
            Assert.AreEqual(new Color(0.15f, 0.17f, 0.20f, 1f), bg.color);
            Assert.AreEqual(Image.Type.Sliced, bg.type);

            InputField neighborInput = neighbor.GetComponent<InputField>();
            Assert.AreEqual(string.Empty, neighborInput.text);
            Assert.AreEqual("Router1", neighborInput.placeholder.GetComponent<Text>().text);
            Assert.IsTrue(neighborInput.placeholder.GetComponent<Text>().enabled);

            Text neighborText = neighbor.Find("Text").GetComponent<Text>();
            Assert.AreEqual(string.Empty, neighborText.text);
            Assert.AreEqual(16, neighborText.fontSize);
            Assert.AreEqual(UIComponents.Colors.textPrimary, neighborText.color);
            Assert.IsFalse(neighborText.raycastTarget);
            Assert.AreEqual(new Vector2(4, 2), neighborText.rectTransform.offsetMin);
            Assert.AreEqual(new Vector2(-4, -2), neighborText.rectTransform.offsetMax);

            Text neighborPh = neighbor.Find("Placeholder").GetComponent<Text>();
            Assert.AreEqual(new Color(0.4f, 0.4f, 0.4f, 1f), neighborPh.color);
            Assert.IsFalse(neighborPh.raycastTarget);
            Assert.AreEqual(new Vector2(4, 2), neighborPh.rectTransform.offsetMin);
            Assert.AreEqual(neighborText, neighborInput.textComponent);

            // Campo con valor: UpdateLabel hidro el placeholder y mostro el texto
            Transform cost = panel.transform.Find("CostInput");
            Assert.IsNotNull(cost);
            InputField costInput = cost.GetComponent<InputField>();
            Assert.AreEqual("10", costInput.text);
            Assert.AreEqual("10", cost.Find("Text").GetComponent<Text>().text);
            Assert.IsFalse(costInput.placeholder.GetComponent<Text>().enabled);
        }

        [Test]
        public void CreateBestRoutePanel_BackButton_HavePaletteColors()
        {
            GameObject panel = ActivityPanelFactory.CreateBestRoutePanel(
                canvas.transform,
                onBack: null
            );

            Button backBtn = panel.transform.Find("BackBtn").GetComponent<Button>();
            Assert.IsNotNull(backBtn);

            // CreateMenuButton usa la tripleta de paleta explicita, no el esquema derivado
            ColorBlock colors = backBtn.colors;
            Assert.AreEqual(UIComponents.Colors.buttonNormal, colors.normalColor);
            Assert.AreEqual(UIComponents.Colors.buttonHover, colors.highlightedColor);
            Assert.AreEqual(UIComponents.Colors.buttonPressed, colors.pressedColor);
            Assert.AreEqual(new Color(0.3f, 0.3f, 0.3f, 0.5f), colors.disabledColor);
            Assert.AreEqual(1f, colors.colorMultiplier);

            // Heredados del ColorBlock por defecto del Button (base defaultColorBlock)
            Color expectedSelected = new Color32(245, 245, 245, 255);
            Assert.AreEqual(expectedSelected, colors.selectedColor);
            Assert.AreEqual(0.1f, colors.fadeDuration, 0.0001f);
        }
    }
}
