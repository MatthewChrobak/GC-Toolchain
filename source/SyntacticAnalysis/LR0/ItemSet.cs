namespace SyntacticAnalysis.LR0
{
    public class ItemSet
    {
        public readonly Rule Rule;
        public readonly int Ptr;

        public ItemSet(Rule rule, int ptr) {
            this.Rule = rule;
            this.Ptr = ptr;
        }

        public override bool Equals(object? obj) {
            if (obj is ItemSet rs) {
                if (rs == this) {
                    return true;
                }

                if (this.Rule.Equals(rs.Rule)) {
                    if (this.Ptr == rs.Ptr) {
                        return true;
                    }
                }
            }
            return false;
        }

        public override int GetHashCode() {
            return this.Rule.GetHashCode() + this.Ptr.GetHashCode();
        }
    }
}