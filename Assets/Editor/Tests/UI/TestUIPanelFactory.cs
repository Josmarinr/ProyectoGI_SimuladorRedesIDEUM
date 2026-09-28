using NUnit.Framework;
using SimRedes.UI;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;

namespace Tests.EditMode.UI
{
    public class TestUIPanelFactory
    {
        private GameObject canvasGo;
        private Canvas canvas;

        [SetUp]
        public void SetUp()
        {
            canvasGo = new GameObject("TestCanvas");
            canvas = canvasGo.AddComponent<Canvas>();
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up any objects created by factories that are not children of canvas
            CleanupRootObjects();
            UnityEngine.Object.DestroyImmediate(canvasGo);
        }

        private void CleanupRootObjects()
        {
            var nav = GameObject.Find("MenuNavigator");
            if (nav != null) UnityEngine.Object.DestroyImmediate(nav);
            var mgr = GameObject.Find("MainMenuManager");
            if (mgr != null) UnityEngine.Object.DestroyImmediate(mgr);
            var panel = GameObject.Find("ClickOutsideBG_ConnectivityPanel");
            if (panel != null) UnityEngine.Object.DestroyImmediate(panel);
        }

        [Test]
        public void GetFont_ReturnsValidFont()
        {
            Font font = UIPanelFactory.GetFont();
            Assert.IsNotNull(font);
        }

        [Test]
        public void CreateMainMenu_WithCallbacks_ReturnsPanel()
        {
            GameObject panel = UIPanelFactory.CreateMainMenu(
                canvas.transform,
                onStartSimulation: null,
                onActivities: null,
                onConnectivity: null,
                onInstructions: null,
                onDiscLegend: null,
                onExit: null
            );

            Assert.IsNotNull(panel);
            Assert.AreEqual("MainMenuPanel", panel.name);

            Assert.IsNotNull(panel.transform.Find("Btn_Iniciar"));
            Assert.IsNotNull(panel.transform.Find("Btn_Actividades"));
            Assert.IsNotNull(panel.transform.Find("Btn_Conectividad"));
            Assert.IsNotNull(panel.transform.Find("Btn_Instrucciones"));
            Assert.IsNotNull(panel.transform.Find("Btn_DiscLegend"));
            Assert.IsNotNull(panel.transform.Find("Btn_Salir"));
        }

        [Test]
        public void CreateActivitiesPanel_WithCallbacks_ReturnsPanel()
        {
            GameObject panel = UIPanelFactory.CreateActivitiesPanel(
                canvas.transform,
                onSelectActivity: null,
                onBack: null
            );

            Assert.IsNotNull(panel);
            Assert.AreEqual("ActivitiesPanel", panel.name);

            var titleObj = panel.transform.Find("MenuTitle");
            Assert.IsNotNull(titleObj);
            var titleText = titleObj.GetComponent<Text>();
            Assert.IsTrue(titleText.text.Contains("Selecciona una Actividad"));

            for (int i = 0; i <= 6; i++)
            {
                Assert.IsNotNull(panel.transform.Find("Activity_" + i),
                    $"Activity_{i} button should exist");
            }
            Assert.IsNotNull(panel.transform.Find("BackBtn"));
        }

        [Test]
        public void CreateConnectivityPanel_ReturnsPanelWithRefs()
        {
            var refs = UIPanelFactory.CreateConnectivityPanel(
                canvas.transform,
                onBack: null
            );

            Assert.IsNotNull(refs.panelObj);
            Assert.IsNotNull(refs.sourceText);
            Assert.IsNotNull(refs.destText);
            Assert.IsNotNull(refs.resultText);
            Assert.IsNotNull(refs.resultIcon);
            Assert.IsNotNull(refs.pingButton);
            Assert.IsNotNull(refs.statusText);
        }

        [Test]
        public void CreateInstructionsPanel_ReturnsPanelWithBackBtn()
        {
            GameObject panel = UIPanelFactory.CreateInstructionsPanel(
                canvas.transform,
                onBack: null
            );

            Assert.IsNotNull(panel);
            Assert.AreEqual("InstructionsPanel", panel.name);
            Assert.IsNotNull(panel.transform.Find("BackBtn"));
        }

        [Test]
        public void CreateDiscLegendPanel_ReturnsPanelWithBackBtn()
        {
            GameObject panel = UIPanelFactory.CreateDiscLegendPanel(
                canvas.transform,
                onBack: null
            );

            Assert.IsNotNull(panel);
            Assert.AreEqual("DiscLegendPanel", panel.name);
            Assert.IsNotNull(panel.transform.Find("BackBtn"));
        }
    }
}
