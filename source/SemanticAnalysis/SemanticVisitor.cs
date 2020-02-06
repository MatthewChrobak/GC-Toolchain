using ASTVisitor;
using Core;
using System;
using System.Dynamic;

namespace SemanticAnalysis
{
    public class SemanticVisitor : PyVisitor
    {
        public SemanticVisitor(string pythonPluginPath) : base(pythonPluginPath) {
            this._scope.log = CreateLogProxy();
            this._scope.symboltable = CreateSymbolTableProxy();
        }

        private dynamic CreateLogProxy() {
            dynamic proxy = new ExpandoObject();
            proxy.WriteLineVerbose = new Action<string>(Log.WriteLineVerbose);
            proxy.WriteLineError = new Action<string>(Log.WriteLineError);
            return proxy;
        }

        private dynamic CreateSymbolTableProxy() {
            dynamic proxy = new ExpandoObject();
            proxy.GetOrCreate = new Func<string, SymbolTable>(SymbolTable.GetOrCreate);
            proxy.Exists = new Func<string, bool>(SymbolTable.Exists);
            return proxy;
        }
    }
}
