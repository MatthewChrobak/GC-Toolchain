using System.Collections.Generic;

namespace ASTVisitor
{
    public class AssociativeArray
    {
        private Dictionary<string, dynamic> _elements;
        public IEnumerable<KeyValuePair<string, dynamic>> Elements => this._elements;
        public int Length => this._elements.Count;

        public dynamic this[string key] {
            get {
                return this._elements[key];
            }
            set {
                this._elements[key] = value;
            }
        }

        public bool Contains(string key) {
            return this._elements.ContainsKey(key);
        }

        public AssociativeArray() {
            this._elements = new Dictionary<string, dynamic>();
        }
    }
}
