using Automata;
using Automata.Deterministic;
using NUnit.Framework;
using System.Linq;

namespace Tests.Automata
{
    public class DFATests
    {
        private readonly DFATable dfa;
        private readonly Node a;
        private readonly Node b;
        private readonly Node c;
        private readonly Node d;
        private readonly Node e;
        private readonly Node f;

        public DFATests() {
            dfa = new DFATable();
            a = dfa.CreateNode();
            b = dfa.CreateNode();
            c = dfa.CreateNode();
            d = dfa.CreateNode();
            e = dfa.CreateNode();
            f = dfa.CreateNode(); 

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
        }

        [Test]
        public void DFA_Minimization() {
            var minDFA = dfa.Minimize();

            Assert.AreEqual(minDFA.Nodes.Count(), 3);
            Assert.AreEqual(minDFA.Transitions.Count(), 6);
        }

        [Test]
        public void DFA_Traversal() {
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
