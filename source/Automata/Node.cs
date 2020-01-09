namespace Automata
{
    public class Node
    {
        public int ID { get; set; }
        public bool IsFinal { get; set; }

        public const string LABEL = "label";

        public Node(int id, bool isFinal = false) {
            this.ID = id;
            this.IsFinal = IsFinal;
        }

        public Node(Node node) {
            this.ID = node.ID;
            this.IsFinal = node.IsFinal;
        }

        public Node Clone() {
            return new Node(this);
        }

        public void SetTag(string key, string value) {
            // TODO:
        }

        public string GetTag(string key) {
            // TODO: 
            return "";
        }
    }
}
