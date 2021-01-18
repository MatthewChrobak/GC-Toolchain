using Core.LexicalAnalysis;

namespace GCTPlugin
{
    public interface ILexicalAnalyzer
    {
        TokenStream Parse(string programText);
    }
}
