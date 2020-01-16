using Automata.Deterministic;
using NUnit.Framework;
using System.Linq;

namespace Tests.Automata
{
    public class DFATests
    {
        private DFATable ConstructDFA() {
            var dfa = new DFATable();
            var a = dfa.CreateNode();
            var b = dfa.CreateNode();
            var c = dfa.CreateNode();
            var d = dfa.CreateNode();
            var e = dfa.CreateNode();
            var f = dfa.CreateNode();

            dfa.StartState = a;

            c.IsFinal = true;
            d.IsFinal = true;
            e.IsFinal = true;

            dfa.AddTransition(a, b, "0");
            dfa.AddTransition(a, c, "1");

            dfa.AddTransition(b, a, "0");
            dfa.AddTransition(b, d, "1");

            dfa.AddTransition(c, f, "1");
            dfa.AddTransition(c, e, "0");

            dfa.AddTransition(d, e, "0");
            dfa.AddTransition(d, f, "1");

            dfa.AddTransition(e, e, "0");
            dfa.AddTransition(e, f, "1");

            dfa.AddTransition(f, f, "1");
            dfa.AddTransition(f, f, "0");

            return dfa;
        }

        [Test]
        public void DFA_Minimization()
        {
            var dfa = ConstructDFA();
            var minDFA = dfa.Minimize();

            Assert.AreEqual(minDFA.Nodes.Count(), 3);
            Assert.AreEqual(minDFA.Transitions.Count(), 6);
        }

        [Test]
        public void DFA_Traversal() {
            var dfa = ConstructDFA();

            var states = dfa.ApplyTransition(dfa.StartState, "0");
            Assert.AreEqual(states.Count, 1);
            Assert.AreEqual(states.First().ID, b.ID);

            states = dfa.ApplyTransition(states, "1");
            Assert.AreEqual(states.Count, 1);
            Assert.AreEqual(states.First().ID, d.ID);

            states = dfa.ApplyTransition(states, "1");
            Assert.AreEqual(states.Count, 1);
            Assert.AreEqual(states.First().ID, f.ID);

            states = dfa.ApplyTransition(states, "1");
            Assert.AreEqual(states.Count, 1);
            Assert.AreEqual(states.First().ID, f.ID);

            states = dfa.ApplyTransition(states, "0");
            Assert.AreEqual(states.Count, 1);
            Assert.AreEqual(states.First().ID, f.ID);
        }
    }
}
