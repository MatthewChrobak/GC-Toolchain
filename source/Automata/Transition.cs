namespace Automata
{
    public class Transition
    {
        public readonly Node Source;
        public readonly Node Destination;
        public readonly string? Symbol;

        private Transition(Node source, Node destination, string? symbol) {
            this.Source = source;
            this.Destination = destination;
            this.Symbol = symbol;
        }

        public static Transition New(Node source    , Node destination, string symbol) {
            return new Transition(source, destination, symbol);
        }

        public static Transition New(Node source, Node destination) {
            return new Transition(source, destination, null);
        }
    }
}
