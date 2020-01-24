namespace SyntacticAnalysis
{
    public class Symbol
    {
        public readonly SymbolType Type;
        public readonly string ID;
        public readonly string? Tag;

        public static Symbol EndStream { get; internal set; }

        static Symbol() {
            EndStream = new Symbol();
        }

        private Symbol() {
            this.Type = SymbolType.Token;
            this.ID = "$";
            this.Tag = null;
        }

        public Symbol(string symbolData, SyntacticConfigurationFile config) {
            string[] data = symbolData.Split(SyntacticConfigurationFile.TOKEN_TAG_SEPARATOR);
            string id = data[0];
            string? tag = data.Length == 2 ? data[1] : null;

            if (id.StartsWith(config.GetRule(SyntacticConfigurationFile.RULE_PRODUCTION_PREFIX_KEY))) {
                this.Type = SymbolType.Production;
                id = id[1..];
            } else {
                this.Type = SymbolType.Token;
            }

            if (id == config.GetRule(SyntacticConfigurationFile.RULE_EPSILON_KEY).ToString()) {
                id = System.String.Empty;
            }

            this.ID = id;
            this.Tag = tag;
        }

        public override bool Equals(object? obj) {
            if (obj is Symbol s) {
                if (s == this) {
                    return true;
                }

                return this.ID == s.ID && this.Type == s.Type && s?.Tag == s?.Tag;
            }

            return false;
        }

        public override int GetHashCode() {
            return this.Type.GetHashCode() + this.ID.GetHashCode() + (this.Tag ?? string.Empty).GetHashCode();
        }
    }

    public enum SymbolType
    {
        Token,
        Production
    }
}