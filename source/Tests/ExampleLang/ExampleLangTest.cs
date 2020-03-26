using ASTVisitor;
using CodeGeneration;
using Core;
using LexicalAnalysis;
using NUnit.Framework;
using SemanticAnalysis;
using SyntacticAnalysis;
using SyntacticAnalysis.CLR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Tests.Lang
{
    public class Column
    {
        public const string Name = "name";
        public const string Type = "type";
        public const string IsParameter = "is_parameter";
        public const string ParameterIndex = "parameter_index";
        public const string EntityType = "entity_type";
    }

    public class EntityType
    {
        public const string Function = "function";
        public const string Variable = "variable";
        public const string CLASS = "class";
    }

    public class ExampleLangTest
    {
        private static readonly string ProgramPath = new DirectoryInfo(Directory.GetCurrentDirectory() + "\\..\\..\\..\\..\\..\\examples\\lang").FullName;
        private static readonly string TokenPath = $"{ProgramPath}\\tokens";
        private static readonly string SyntaxPath = $"{ProgramPath}\\syntax";
        private static readonly string SemanticVisitorsPath = $"{ProgramPath}\\semantics\\";
        private static readonly string CodeGeneratorVisitorsPath = $"{ProgramPath}\\codegeneration\\";
        private static readonly string PostBuildScript = $"{ProgramPath}\\build_d.ps1";
        private static readonly string STDIO_D = $"{ProgramPath}\\stdio_d.c";
        private static readonly string InstructionStreamFileName = "output.ir";
        private readonly string OutputPath;
        public string ProgramOutput => File.Exists(this.OutputPath) ? File.ReadAllText(this.OutputPath) : string.Empty;

        public ExampleLangTest(string program, [CallerMemberNameAttribute] string testName = "unknown") {
            string cwd = Directory.GetCurrentDirectory();
            string localDirectory;
            using (var md5 = System.Security.Cryptography.MD5.Create()) {
                var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(program));
                localDirectory = Path.Combine(cwd, $"{testName}_{BitConverter.ToString(hash).Replace("-", "")}");
            }
            if (Directory.Exists(localDirectory)) {
                Directory.Delete(localDirectory, true);
            }
            Directory.CreateDirectory(localDirectory);
            try {
                SymbolTable.Reset();
                // Lexical analysis
                var tokenConfigurationFile = new LexicalConfigurationFile(TokenPath);
                var tokenParserTableGenerator = new TokenParserTableGenerator(tokenConfigurationFile);
                var tokenParser = new TokenParser(tokenParserTableGenerator.NFATable);
                var tokenStream = tokenParser.ParseString(program);

                // Syntactic analysis
                var syntaxConfigurationFile = new SyntacticConfigurationFile(SyntaxPath);
                var productionTable = new ProductionTable(syntaxConfigurationFile);
                var clrStates = new CLRStateGenerator(productionTable, syntaxConfigurationFile);
                var lrTable = LRParsingTable.From(clrStates, productionTable);
                var syntaxParser = new LRParser(syntaxConfigurationFile, tokenStream);
                var ast = syntaxParser.Parse(lrTable, tokenStream);

                foreach (var file in Directory.GetFiles(SemanticVisitorsPath, "*.py")) {
                    new SemanticVisitor(file).Traverse(ast);
                }

                var instructionStream = new InstructionStream();
                foreach (var file in Directory.GetFiles(CodeGeneratorVisitorsPath, "*.py")) {
                    new CodeGeneratorVisiter(file, instructionStream).Traverse(ast);
                }
                File.WriteAllText(Path.Combine(localDirectory, InstructionStreamFileName), instructionStream.ToString());

                File.Copy(PostBuildScript, Path.Combine(localDirectory, new FileInfo(PostBuildScript).Name));
                File.Copy(STDIO_D, Path.Combine(localDirectory, new FileInfo(STDIO_D).Name));
                var process = new System.Diagnostics.Process();
                process.StartInfo = new System.Diagnostics.ProcessStartInfo() {
                    FileName = "powershell",
                    Arguments = PostBuildScript,
                    WorkingDirectory = localDirectory,
                    RedirectStandardOutput = true
                };
                process.Start();
                process.WaitForExit();
                Log.WriteLineVerbose(process.StandardOutput.ReadToEnd());

                this.OutputPath = Path.Combine(localDirectory, "debug_output.out");
                process = new System.Diagnostics.Process();
                process.StartInfo = new System.Diagnostics.ProcessStartInfo() {
                    FileName = Path.Combine(localDirectory, "program.exe"),
                    WorkingDirectory = localDirectory
                };
                process.Start();
                process.WaitForExit();
            } catch (Exception e) {
                throw e;
            } finally {
                Log.Dump(Path.Combine(localDirectory, "dmp.log"));
            }
        }

        public ExistingSymbolTable SymbolTableExists(string id) {
            Assert.IsTrue(SymbolTable.Exists(id));
            return new ExistingSymbolTable(SymbolTable.GetOrCreate(id));
        }
    }

    public class ExistingSymbolTable
    {
        private SymbolTable _symbolTable;

        public ExistingSymbolTable(SymbolTable symboltable) {
            this._symbolTable = symboltable;
        }

        public IEnumerable<ExistingRow> WithAtLeastNRows(int n, params (string key, dynamic value)[] filters) {
            var rows = this._symbolTable.Rows;
            foreach (var entry in filters) {
                rows = rows.Where(row => row.Contains(entry.key) && row[entry.key] == entry.value);
            }
            Assert.GreaterOrEqual(rows.Count(), n);
            return rows.Select(row => new ExistingRow(row));
        }

        public IEnumerable<ExistingRow> WithOneRow(params (string key, dynamic value)[] filters) {
            return WithExactlyNRows(1, filters);
        }

        public IEnumerable<ExistingRow> WithExactlyNRows(int n, params (string key, dynamic value)[] filters) {
            var rows = WithAtLeastNRows(n, filters);
            Assert.AreEqual(n, rows.Count());
            return rows;
        }

        public IEnumerable<ExistingRow> WithAnyRows(params (string key, dynamic value)[] filters) {
            return WithAtLeastNRows(1, filters);
        }

        public void WithNoRows(params (string key, dynamic value)[] filters) {
            WithExactlyNRows(0, filters);
        }
    }

    public class ExistingRow
    {
        private AssociativeArray _row;

        public dynamic this[string key] {
            get {
                return this._row[key];
            }
        }

        public ExistingRow(AssociativeArray row) {
            this._row = row;
        }

        public bool Contains(string column) {
            return this._row.Contains(column);
        }
    }

    public static class Extensions
    {
        public static IEnumerable<ExistingRow> WithColumn(this IEnumerable<ExistingRow> rows, string column, dynamic value) {
            foreach (var row in rows) {
                Assert.IsTrue(row.Contains(column));
                Assert.AreEqual(value, row[column]);
            }
            return rows;
        }

        public static ExistingRow WithColumn(this ExistingRow row, string column, dynamic value) {
            Assert.IsTrue(row.Contains(column));
            Assert.AreEqual(value, row[column]);
            return row;
        }
    }
}
