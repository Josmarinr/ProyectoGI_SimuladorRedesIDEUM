using NUnit.Framework;
using SimRedes.Network;

namespace Tests.EditMode.Network
{
    public class TestACLManager
    {
        // ====================================================================
        // ACLRule tests
        // ====================================================================

        [Test]
        public void ACLRule_DefaultValues_ArePermitAndAny()
        {
            var rule = new ACLRule();
            Assert.AreEqual(ACLAction.Permit, rule.Action);
            Assert.AreEqual(ACLProtocol.Any, rule.Protocol);
            Assert.AreEqual("0.0.0.0", rule.SourceMask);
            Assert.AreEqual("0.0.0.0", rule.DestMask);
        }

        [Test]
        public void ACLRule_Matches_WildcardSourceAny()
        {
            var rule = new ACLRule();
            Assert.IsTrue(rule.Matches("192.168.1.1", "10.0.0.1"));
        }

        [Test]
        public void ACLRule_Matches_WildcardDestAny()
        {
            var rule = new ACLRule();
            Assert.IsTrue(rule.Matches("192.168.1.1", "10.0.0.1"));
        }

        [Test]
        public void ACLRule_Matches_SpecificSource_Matches()
        {
            var rule = new ACLRule
            {
                SourceIP = "192.168.1.0",
                SourceMask = "255.255.255.0"
            };
            Assert.IsTrue(rule.Matches("192.168.1.10", "10.0.0.1"));
        }

        [Test]
        public void ACLRule_Matches_SpecificSource_DoesNotMatch()
        {
            var rule = new ACLRule
            {
                SourceIP = "192.168.1.0",
                SourceMask = "255.255.255.0"
            };
            Assert.IsFalse(rule.Matches("10.0.0.10", "10.0.0.1"));
        }

        [Test]
        public void ACLRule_Matches_ProtocolMatch()
        {
            var rule = new ACLRule
            {
                Protocol = ACLProtocol.TCP
            };
            Assert.IsTrue(rule.Matches("10.0.0.1", "20.0.0.1", null, null, "tcp"));
        }

        [Test]
        public void ACLRule_Matches_ProtocolMismatch()
        {
            var rule = new ACLRule
            {
                Protocol = ACLProtocol.TCP
            };
            Assert.IsFalse(rule.Matches("10.0.0.1", "20.0.0.1", null, null, "udp"));
        }

        [Test]
        public void ACLRule_Matches_ProtocolAny_MatchesAll()
        {
            var rule = new ACLRule
            {
                Protocol = ACLProtocol.Any
            };
            Assert.IsTrue(rule.Matches("10.0.0.1", "20.0.0.1", null, null, "tcp"));
            Assert.IsTrue(rule.Matches("10.0.0.1", "20.0.0.1", null, null, "udp"));
            Assert.IsTrue(rule.Matches("10.0.0.1", "20.0.0.1", null, null, "icmp"));
        }

        [Test]
        public void ACLRule_Matches_SourcePort_Matches()
        {
            var rule = new ACLRule
            {
                SourcePort = 80
            };
            Assert.IsTrue(rule.Matches("10.0.0.1", "20.0.0.1", 80, null));
        }

        [Test]
        public void ACLRule_Matches_SourcePort_Mismatch()
        {
            var rule = new ACLRule
            {
                SourcePort = 80
            };
            Assert.IsFalse(rule.Matches("10.0.0.1", "20.0.0.1", 8080, null));
        }

        [Test]
        public void ACLRule_Matches_DestPort_Matches()
        {
            var rule = new ACLRule
            {
                DestPort = 443
            };
            Assert.IsTrue(rule.Matches("10.0.0.1", "20.0.0.1", null, 443));
        }

        [Test]
        public void ACLRule_Matches_DestPort_Mismatch()
        {
            var rule = new ACLRule
            {
                DestPort = 443
            };
            Assert.IsFalse(rule.Matches("10.0.0.1", "20.0.0.1", null, 80));
        }

        [Test]
        public void ACLRule_Matches_NullPorts_Skipped()
        {
            var rule = new ACLRule();
            // When SourcePort and DestPort are null, they should not be checked,
            // even if the caller passes port values.
            Assert.IsTrue(rule.Matches("10.0.0.1", "20.0.0.1", 12345, 54321));
        }

        // ====================================================================
        // ACLManager tests
        // ====================================================================

        private ACLManager manager;

        [SetUp]
        public void SetUp()
        {
            manager = new ACLManager();
        }

        [Test]
        public void CreateACL_NewName_CreatesACL()
        {
            manager.CreateACL("TEST_ACL");
            var rules = manager.GetRules("TEST_ACL");
            Assert.AreEqual(0, rules.Count);
        }

        [Test]
        public void CreateACL_DuplicateName_NoOp()
        {
            manager.CreateACL("TEST_ACL");
            manager.CreateACL("TEST_ACL");
            // Should not duplicate — only one ACL with this name
            var rules = manager.GetRules("TEST_ACL");
            Assert.AreEqual(0, rules.Count);
        }

        [Test]
        public void AddRule_AutoCreatesACL()
        {
            var rule = new ACLRule { SourceIP = "10.0.0.0", SourceMask = "255.0.0.0" };
            manager.AddRule("AUTO_ACL", rule);
            var rules = manager.GetRules("AUTO_ACL");
            Assert.AreEqual(1, rules.Count);
        }

        [Test]
        public void AddRule_SequenceAutoAssign()
        {
            var rule1 = new ACLRule();
            var rule2 = new ACLRule();
            manager.AddRule("ACL", rule1);
            manager.AddRule("ACL", rule2);
            Assert.AreEqual(10, rule1.Sequence);
            Assert.AreEqual(20, rule2.Sequence);
        }

        [Test]
        public void AddRule_SequenceIncrementsBy10()
        {
            var rule1 = new ACLRule();
            var rule2 = new ACLRule();
            var rule3 = new ACLRule();
            manager.AddRule("ACL", rule1);
            manager.AddRule("ACL", rule2);
            manager.AddRule("ACL", rule3);
            Assert.AreEqual(10, rule1.Sequence);
            Assert.AreEqual(20, rule2.Sequence);
            Assert.AreEqual(30, rule3.Sequence);
        }

        [Test]
        public void RemoveRule_RemovesBySequence()
        {
            var rule1 = new ACLRule();
            var rule2 = new ACLRule();
            manager.AddRule("ACL", rule1);
            manager.AddRule("ACL", rule2);
            manager.RemoveRule("ACL", 10);
            var rules = manager.GetRules("ACL");
            Assert.AreEqual(1, rules.Count);
            Assert.AreEqual(20, rules[0].Sequence);
        }

        [Test]
        public void CheckPacket_NoACL_ReturnsTrue()
        {
            Assert.IsTrue(manager.CheckPacket("NONEXISTENT", "10.0.0.1", "20.0.0.1"));
        }

        [Test]
        public void CheckPacket_EmptyACL_ReturnsTrue()
        {
            manager.CreateACL("EMPTY");
            Assert.IsTrue(manager.CheckPacket("EMPTY", "10.0.0.1", "20.0.0.1"));
        }

        [Test]
        public void CheckPacket_DenyRule_Blocks()
        {
            var denyRule = new ACLRule
            {
                Action = ACLAction.Deny,
                SourceIP = "10.0.0.0",
                SourceMask = "255.0.0.0"
            };
            manager.AddRule("ACL", denyRule);
            Assert.IsFalse(manager.CheckPacket("ACL", "10.0.0.1", "20.0.0.1"));
        }

        [Test]
        public void CheckPacket_PermitRule_Allows()
        {
            var permitRule = new ACLRule
            {
                Action = ACLAction.Permit,
                SourceIP = "10.0.0.0",
                SourceMask = "255.0.0.0"
            };
            manager.AddRule("ACL", permitRule);
            Assert.IsTrue(manager.CheckPacket("ACL", "10.0.0.1", "20.0.0.1"));
        }

        [Test]
        public void CheckPacket_FirstMatchWins_OrderMatters()
        {
            var denyRule = new ACLRule
            {
                Action = ACLAction.Deny,
                SourceIP = "10.0.0.0",
                SourceMask = "255.0.0.0"
            };
            var permitRule = new ACLRule
            {
                Action = ACLAction.Permit,
                SourceIP = "10.0.0.0",
                SourceMask = "255.0.0.0"
            };

            // Deny first, then permit — should be denied (first match wins)
            manager.AddRule("ACL", denyRule);
            manager.AddRule("ACL", permitRule);
            Assert.IsFalse(manager.CheckPacket("ACL", "10.0.0.1", "20.0.0.1"));
        }

        [Test]
        public void CheckPacket_NoMatch_ImplicitDeny()
        {
            var permitRule = new ACLRule
            {
                Action = ACLAction.Permit,
                SourceIP = "192.168.1.0",
                SourceMask = "255.255.255.0"
            };
            manager.AddRule("ACL", permitRule);
            // Packet from a different network — no rule matches → implicit deny
            Assert.IsFalse(manager.CheckPacket("ACL", "10.0.0.1", "20.0.0.1"));
        }

        [Test]
        public void DeleteACL_Existing_Removes()
        {
            manager.CreateACL("TEST_ACL");
            manager.DeleteACL("TEST_ACL");
            // After deletion, CheckPacket for non-existent ACL returns true (permit all)
            Assert.IsTrue(manager.CheckPacket("TEST_ACL", "10.0.0.1", "20.0.0.1"));
        }

        [Test]
        public void CreateStandardRule_ReturnsCorrectAction()
        {
            var permitRule = ACLManager.CreateStandardRule("permit", "192.168.1.0");
            Assert.AreEqual(ACLAction.Permit, permitRule.Action);
            Assert.AreEqual("192.168.1.0", permitRule.SourceIP);

            var denyRule = ACLManager.CreateStandardRule("deny", "10.0.0.0");
            Assert.AreEqual(ACLAction.Deny, denyRule.Action);
            Assert.AreEqual("10.0.0.0", denyRule.SourceIP);
        }

        [Test]
        public void CreateExtendedRule_ReturnsCorrectFields()
        {
            var rule = ACLManager.CreateExtendedRule("deny", "tcp", "10.0.0.0", "20.0.0.0", "Block TCP");
            Assert.AreEqual(ACLAction.Deny, rule.Action);
            Assert.AreEqual(ACLProtocol.TCP, rule.Protocol);
            Assert.AreEqual("10.0.0.0", rule.SourceIP);
            Assert.AreEqual("20.0.0.0", rule.DestIP);
            Assert.AreEqual("Block TCP", rule.Description);
        }

        [Test]
        public void Rules_Property_AggregatesAllACLs()
        {
            manager.AddRule("ACL_A", new ACLRule { SourceIP = "10.0.0.0" });
            manager.AddRule("ACL_A", new ACLRule { SourceIP = "20.0.0.0" });
            manager.AddRule("ACL_B", new ACLRule { SourceIP = "30.0.0.0" });
            Assert.AreEqual(3, manager.Rules.Count);
        }
    }
}
