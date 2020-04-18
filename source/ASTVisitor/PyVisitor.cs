using Core;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using System;
using System.Collections.Generic;
using System.IO;

namespace ASTVisitor
{
    public abstract class PyVisitor
    {
        private readonly string _pythonPluginPath;
        private readonly ScriptEngine _engine;
        protected readonly dynamic _scope;
        private readonly ScriptSource _source;
        private readonly CompiledCode _compiled;
        private readonly Dictionary<string, dynamic> _cachedVariables;

        private const string PRE_ORDER_PREFIX = "preorder_";
        private const string POST_ORDER_PREFIX = "postorder_";

        public PyVisitor(string pythonPluginPath) {
            this._pythonPluginPath = pythonPluginPath;
            Log.WriteLineVerbose($"Creating visitor for {this._pythonPluginPath}");

            var fi = new FileInfo(pythonPluginPath);
            string folder = fi.Directory.FullName;
            string libs = Path.Combine(AppContext.BaseDirectory, "lib");
            this._engine = Python.CreateEngine();
            this._engine.SetSearchPaths(new string[] { folder, libs });
            this._scope = this._engine.CreateScope();
            this._source = this._engine.CreateScriptSourceFromFile(pythonPluginPath);
            this._compiled = this._source.Compile();
            this._compiled.Execute(this._scope);

            this._cachedVariables = new Dictionary<string, dynamic>();
            foreach (var variableName in this._scope.GetVariableNames()) {
                if (variableName.StartsWith(PRE_ORDER_PREFIX) || variableName.StartsWith(POST_ORDER_PREFIX)) {
                    _cachedVariables[variableName] = this._scope.GetVariable(variableName);
                }
            }
        }

        public void Traverse(ASTNode ast) {
            Log.WriteLineVerbose($"Running visitor for {this._pythonPluginPath}");

            var stk = new Stack<(string key, ASTNode node, bool visited)>();
            stk.Push(("global", ast, false));

            while (stk.Count != 0) {
                (string key, var node, bool elementsVisited) = stk.Pop();

                // Figure out what visit function to call.
                if (!elementsVisited) {
                    string pre_visit_id = $"{PRE_ORDER_PREFIX}{key}";
                    this.TryInvoke(pre_visit_id, node);

                    // If we didn't visit it yet, put it back on for postorder visit.
                    stk.Push((key, node, true));
                } else {
                    string post_visit_id = $"{POST_ORDER_PREFIX}{key}";
                    this.TryInvoke(post_visit_id, node);
                    continue;
                }
                
                foreach (var element in node.ReverseElements) {
                    var value = element.Value;
                    if (value is List<ASTNode> lst) {
                        for (int i = lst.Count - 1; i >= 0; i--) {
                            var member = lst[i];
                            stk.Push((element.Key, member, false));
                        }
                    } else if (value is ASTNode astnode) {
                        stk.Push((element.Key, astnode, false));
                    } else if (value is string) {
                        // Ignore
                    } else if (value is int) {
                        // Ignore
                    } else if (value is bool) {
                        // ignore
                    } else {
                        Log.WriteLineWarning($"Unknown element type in ASTNode: {element.Key}:{value?.GetType()}");
                    }
                }
            }
        }

        private void TryInvoke(string id, ASTNode node) {
            if (this._cachedVariables.TryGetValue(id, out var variable)) {
                try {
                    this._engine.Operations.Invoke(variable, node);
                } catch (Exception e) {
                    var eo = this._engine.GetService<ExceptionOperations>();
                    Debug.Assert(false, eo.FormatException(e));
                }
            }
        }
    }
}
