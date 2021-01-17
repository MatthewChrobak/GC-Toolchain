using ASTVisitor;
using Core.Logging;
using System;
using System.Dynamic;

namespace SemanticAnalysis
{
    public class SemanticVisitor : PyVisitor
    {
        public SemanticVisitor(string pythonPluginPath, Log? log) : base(pythonPluginPath, log) {
            this._scope.log = CreateLogProxy();
            this._scope.symboltable = CreateSymbolTableProxy();
        }

        private dynamic CreateLogProxy() {
            dynamic proxy = new ExpandoObject();
            if (this._log is not null) {
                proxy.WriteLineVerbose = new Action<string>(this._log.WriteLineVerbose);
                proxy.WriteLineError = new Action<string>(this._log.WriteLineError);
            } else {
                proxy.WriteLineVerbose = new Action<string>(arg => { });
                proxy.WriteLineError = new Action<string>(arg => { });
            }
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
