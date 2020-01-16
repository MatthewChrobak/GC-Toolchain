using Automata.NonDeterministic;
using NUnit.Framework;
using System.Linq;

namespace Tests.Automata
{
    public class NFATests
    {
        [Test]
        public void ConstructNFA() {
            var nfa = new NFATable();
            var a = nfa.CreateNode();
            var b = nfa.CreateNode();
            nfa.AddTransition(a, b);

            Assert.IsTrue(nfa.Nodes.Contains(a));
            Assert.IsTrue(nfa.Nodes.Contains(b));
            Assert.AreEqual(nfa.Nodes.Count(), 2);

            Assert.IsTrue(nfa.Transitions.Any(transition => transition.Source == a && transition.Destination == b));
            Assert.AreEqual(nfa.Transitions.Count(), 1);
        }

        [Test]
        public void StartState_NotNull_AfterAssignment() {
            var nfa = new NFATable();
            var a = nfa.CreateNode();
            nfa.StartState = a;

            Assert.AreEqual(nfa.StartState, a);
        }

        [Test]
        public void NFAtoDFA() {
            var nfa = new NFATable();
            var a = nfa.CreateNode();
            var b = nfa.CreateNode();
            var c = nfa.CreateNode();
            c.IsFinal = true;
            nfa.StartState = a;

            nfa.AddTransition(a, a, "1");
            nfa.AddTransition(a, b);
            nfa.AddTransition(a, b, "0");
            nfa.AddTransition(a, c, "0");

            nfa.AddTransition(b, b, "1");
            nfa.AddTransition(b, c);

            nfa.AddTransition(c, c, "1");
            nfa.AddTransition(c, c, "0");

            var dfa = nfa.ToDFATable();

            Assert.AreEqual(dfa.Nodes.Count(), 3);
            Assert.AreEqual(dfa.Transitions.Count(), 6);

            var nodes = dfa.Nodes.ToArray();
            Assert.IsTrue(nodes[0].IsFinal);
            Assert.IsTrue(nodes[1].IsFinal);
            Assert.IsTrue(nodes[2].IsFinal);
        }
    }
}
