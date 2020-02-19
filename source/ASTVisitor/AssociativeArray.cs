using System.Collections.Generic;

namespace ASTVisitor
{
    public class AssociativeArray
    {
        private List<string> _insertOrder;
        private Dictionary<string, dynamic> _elements;
        public IEnumerable<KeyValuePair<string, dynamic>> Elements => this.GetElements();
        public IEnumerable<KeyValuePair<string, dynamic>> ReverseElements => this.GetElementsReverse();

        private IEnumerable<KeyValuePair<string, dynamic>> GetElements() {
            for (int i = 0; i < this._insertOrder.Count; i++) {
                var element = this._insertOrder[i];
                if (this._elements.ContainsKey(element)) {
                    yield return new KeyValuePair<string, dynamic>(element, this._elements[element]);
                }
            }
        }

        private IEnumerable<KeyValuePair<string, dynamic>> GetElementsReverse() {
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
                this._insertOrder.Remove(key);
                this._insertOrder.Insert(0, key);
                this._elements[key] = value;
            }
        }

        public IEnumerable<ASTNode> AsArray(string key) {
            if (this._elements[key] is List<ASTNode> lst) {
                return new List<ASTNode>(lst);
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
