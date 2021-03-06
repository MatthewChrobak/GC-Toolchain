﻿using ASTVisitor;
using Core;
using System;
using System.Dynamic;

namespace CodeGeneration
{
    public class CodeGeneratorVisiter : PyVisitor
    {
        public CodeGeneratorVisiter(string pythonPluginPath, InstructionStream instructionStream) : base(pythonPluginPath) {
            this._scope.log = CreateLogProxy();
            this._scope.symboltable = CreateSymbolTableProxy();
            this._scope.instructionstream = CreateInstructionStreamProxy(instructionStream);
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

        private dynamic CreateInstructionStreamProxy(InstructionStream instructionStream) {
            dynamic proxy = new ExpandoObject();
            proxy.Append = new Action<string>(instructionStream.Append);
            proxy.AppendLine = new Action<string>(instructionStream.AppendLine);
            proxy.AppendLineNoIndent = new Action<string>(instructionStream.AppendLineNoIndent);
            proxy.IncrementTab = new Action<int>(instructionStream.IncrementTab);
            proxy.CreateEmptyInstruction = new Func<int>(instructionStream.CreateEmptyInstruction);
            proxy.AppendLineAt = new Action<int, string>(instructionStream.AppendInstructionAt);
            return proxy;
        }
    }
}
