using NUnit.Framework;
using SimRedes.UI;
using SimRedes.Network;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;
using DeviceType = SimRedes.Network.DeviceType;

namespace Tests.EditMode.UI
{
    public class TestConfigPanelFactory
    {
        private GameObject canvasGo;
        private Canvas canvas;
        private GameObject topologyGo;
        private TopologyManager topology;

        [SetUp]
        public void SetUp()
        {
            canvasGo = new GameObject("TestCanvas");
            canvas = canvasGo.AddComponent<Canvas>();

            topologyGo = new GameObject("TestTopology");
            topology = topologyGo.AddComponent<TopologyManager>();
            var awakeMethod = typeof(TopologyManager).GetMethod("Awake",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (awakeMethod != null)
                awakeMethod.Invoke(topology, null);
        }

        [TearDown]
        public void TearDown()
        {
            // Clear TopologyManager singleton
            var instanceField = typeof(TopologyManager).GetField("Instance",
                BindingFlags.Static | BindingFlags.NonPublic);
            if (instanceField != null)
                instanceField.SetValue(null, null);

            // Clean up any panels left in scene by factories
            CleanupPanels();

            UnityEngine.Object.DestroyImmediate(topologyGo);
            UnityEngine.Object.DestroyImmediate(canvasGo);
        }

        private void CleanupPanels()
        {
            string[] panelNames = new string[]
            {
                "IPConfigPanel", "ARPPanel", "RoutingPanel", "AddRoutePanel",
                "VLANPanel", "ACLPanel", "NATPanel",
                "ClickOutsideBG_VLANPanel", "ClickOutsideBG_ACLPanel", "ClickOutsideBG_NATPanel"
            };
            foreach (string name in panelNames)
            {
                GameObject obj = GameObject.Find(name);
                if (obj != null)
                    UnityEngine.Object.DestroyImmediate(obj);
            }
        }

        [Test]
        public void CreateIPConfigPanel_WithData_ReturnsValidationText()
        {
            Text validationText = ConfigPanelFactory.CreateIPConfigPanel(
                canvas.transform,
                "TestRouter",
                "192.168.1.1",
                "255.255.255.0",
                onIPChanged: null,
                onMaskChanged: null,
                onApply: null,
                onARP: null,
                onRouting: null,
                onCancel: null
            );

            Assert.IsNotNull(validationText);
            Assert.IsNotNull(GameObject.Find("IPConfigPanel"));
        }

        [Test]
        public void CreateARPPanel_WithNode_ReturnsPanel()
        {
            var node = new NetworkNode(1, DeviceType.Router, Vector2.zero);

            GameObject panel = ConfigPanelFactory.CreateARPPanel(canvas.transform, node);

            Assert.IsNotNull(panel);
            Assert.AreEqual("ARPPanel", panel.name);
        }

        [Test]
        public void CreateRoutingPanel_WithNode_ReturnsPanel()
        {
            var node = new NetworkNode(1, DeviceType.Router, Vector2.zero);

            GameObject panel = ConfigPanelFactory.CreateRoutingPanel(
                canvas.transform,
                node,
                onDeleteRoute: null,
                onAddRoute: null,
                onClose: null
            );

            Assert.IsNotNull(panel);
            Assert.AreEqual("RoutingPanel", panel.name);
        }

        [Test]
        public void CreateAddRoutePanel_WithNode_ReturnsPanel()
        {
            var node = new NetworkNode(1, DeviceType.Router, Vector2.zero);

            GameObject panel = ConfigPanelFactory.CreateAddRoutePanel(
                canvas.transform,
                node,
                onAddRoute: null,
                onCancel: null
            );

            Assert.IsNotNull(panel);
            Assert.AreEqual("AddRoutePanel", panel.name);
            Assert.IsNotNull(panel.transform.Find("ConfirmAddRouteBtn"));
            Assert.IsNotNull(panel.transform.Find("CancelAddRouteBtn"));
        }

        [Test]
        public void CreateVLANPanel_WithTopology_DoesNotThrow()
        {
            Assert.DoesNotThrow(() =>
            {
                ConfigPanelFactory.CreateVLANPanel(canvas.transform, topology);
            });

            GameObject panel = GameObject.Find("VLANPanel");
            Assert.IsNotNull(panel);
        }

        [Test]
        public void CreateACLPanel_WithTopology_DoesNotThrow()
        {
            Assert.DoesNotThrow(() =>
            {
                ConfigPanelFactory.CreateACLPanel(canvas.transform, topology);
            });

            GameObject panel = GameObject.Find("ACLPanel");
            Assert.IsNotNull(panel);
        }

        [Test]
        public void CreateNATPanel_WithTopology_DoesNotThrow()
        {
            Assert.DoesNotThrow(() =>
            {
                ConfigPanelFactory.CreateNATPanel(canvas.transform, topology);
            });

            GameObject panel = GameObject.Find("NATPanel");
            Assert.IsNotNull(panel);
        }
    }
}
