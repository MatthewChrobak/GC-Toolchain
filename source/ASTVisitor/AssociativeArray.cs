using System.Collections.Generic;

namespace ASTVisitor
{
    public class AssociativeArray
    {
        private List<string> _insertOrder;
        private Dictionary<string, dynamic> _elements;
        public IEnumerable<KeyValuePair<string, dynamic>> Elements => this.GetElements();

        private IEnumerable<KeyValuePair<string, dynamic>> GetElements() {
            for (int i = this._insertOrder.Count - 1; i >= 0; i--) {
                var element = this._insertOrder[i];
                if (this._elements.ContainsKey(element)) {
                    yield return new KeyValuePair<string, dynamic>(element, this._elements[element]);
                }
            }
        }

        public int Length => this._elements.Count;

        public dynamic this[string key] {
            get {
                return this._elements[key];
            }
            set {
                if (!this._elements.ContainsKey(key)) {
                    this._insertOrder.Add(key);
                }
                this._elements[key] = value;
            }
        }

        public IEnumerable<ASTNode> AsArray(string key) {
            if (this._elements[key] is List<ASTNode> lst) {
                return new Stack<ASTNode>(lst);
            }
            return new ASTNode[] { this._elements[key] };
        }

        public bool Contains(string key) {
            return this._elements.ContainsKey(key);
        }

        public AssociativeArray() {
            this._elements = new Dictionary<string, dynamic>();
            this._insertOrder = new List<string>();
        }
    }
}
