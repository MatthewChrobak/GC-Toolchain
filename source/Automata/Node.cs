using System.Collections.Generic;
using System.Linq;

namespace Automata
{
    public class Node
    {
        public int ID { get; set; }
        public bool IsFinal { get; set; }

        private HashSet<string> _tags;
        public const string LABEL = "label";

        public Node(int id, bool isFinal = false) {
            this.ID = id;
            this.IsFinal = IsFinal;
            this._tags = new HashSet<string>();
        }

        public Node(Node node) {
            this.ID = node.ID;
            this.IsFinal = node.IsFinal;
            this._tags = new HashSet<string>();
            this.UnionTags(node.GetTags());
        }

        public Node Clone() {
            return new Node(this);
        }

        public void SetTag(string key, string value) {
            this._tags.Add($"{key}:{value}");
        }

        public void UnionTags(IEnumerable<string> tags) {
            this._tags.UnionWith(tags);
        }

        public IEnumerable<string> GetTags() {
            return this._tags;
        }

        public IEnumerable<string> GetTags(string key) {
            return this._tags.Where(val => val.StartsWith(key)).Select(val => val.Remove(0, key.Length + 1));
        }
    }
}
