using ASTVisitor;
using CodeGeneration;
using Core;
using Core.ReportGeneration;
using LexicalAnalysis;
using SemanticAnalysis;
using SyntacticAnalysis;
using SyntacticAnalysis.CLR;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace GCT
{
    public static class Program
    {
        private static Report report;
        private static string logDumpPath;

        public static void Main(string[] args) {
            // If no args are given, take some.
            if (args.Length == 0) {
                Main(Console.ReadLine().Split(' '));
                return;
            }

            try {
                RealMain(args);
            } catch (AssertionFailedException e) {
                Console.WriteLine($"\r\nUnable to continue due to exception of type {e.GetType()} being thrown during {Log.State}. Exiting.");
                Console.ReadKey();
            } finally {
                report.AddSection(Log.GetReportSections());
                report.Save();

                Log.WriteLineVerbose($"Writing log to {logDumpPath}...");
                Log.Dump(logDumpPath);
            }
        }

        private static void RealMain(string[] args) {
            string? tokenConfigurationFilePath = null;
            string? syntaxConfigurationFilePath = null;
            string? reportName = null;
            string? sourcefile = null;
            string? semanticsFolder = null;
            string? codeGeneratorFolder = null;
            string? postBuildScript = null;
            string? instructionStreamFilepath = null;
            string? cwd = AppDomain.CurrentDomain.BaseDirectory;
            logDumpPath = Path.Join(cwd, "dmp.log");

            bool includeAST = false;
            bool includeStateMachine = false;
            bool includeTokenStream = false;
            bool includeGrammar = false;
            bool includeLRTable = false;
            bool includeLRTrace = false;
            bool includeLRStates = false;
            bool includeSymbolTables = false;

            // Extract arguments
            var matches = Regex.Matches(string.Join(' ', args), @"\-(\w+)((\s+\'[^\r\n\']+\')|(\s+\""[^\r\n\""]+\"")|(\s+[^\r\n\s\-]+))?");
            for (int i = 0; i < matches.Count; i++) {
                var match = matches[i];
                string flagKey = match.Groups[1].Value.Trim();
                string flagValue = match.Groups[2].Value.Trim();

                if (flagValue.StartsWith('"') && flagValue.EndsWith('"') || flagValue.StartsWith('\'') && flagValue.EndsWith('\'')) {
                    flagValue = flagValue[1..^1];
                }

                switch (flagKey) {
                    case "f":
                        MakeSureFolderExists(flagValue);

                        tokenConfigurationFilePath = flagValue + "/tokens";
                        syntaxConfigurationFilePath = flagValue + "/syntax";
                        sourcefile = flagValue + "/program";
                        reportName = flagValue + "/report";
                        semanticsFolder = flagValue + "/semantics/";
                        codeGeneratorFolder = flagValue + "/codegeneration/";
                        postBuildScript = flagValue + "/build.ps1";
                        instructionStreamFilepath = flagValue + "/output.ir";

                        MakeSureFileExists(instructionStreamFilepath);
                        MakeSureFileExists(tokenConfigurationFilePath);
                        MakeSureFileExists(syntaxConfigurationFilePath);
                        MakeSureFileExists(sourcefile);
                        MakeSureFolderExists(semanticsFolder);
                        MakeSureFolderExists(codeGeneratorFolder);
                        MakeSureFileExists(postBuildScript);

                        cwd = flagValue;
                        logDumpPath = Path.Combine(cwd, "dmp.log");
                        break;
                    case "v":
                    case "verbose":
                        Debug.Assert(flagValue == string.Empty, "Verbose flag should not trailed by any value");
                        Log.EnableLevel(OutputLevel.Verbose);
                        break;
                    case "t":
                    case "token":
                    case "tokens":
                        tokenConfigurationFilePath = flagValue;
                        break;
                    case "r":
                    case "report":
                        reportName = flagValue;
                        break;
                    case "s":
                        syntaxConfigurationFilePath = flagValue;
                        break;
                    case "sourcefile":
                    case "program":
                    case "p":
                        sourcefile = flagValue;
                        break;
                    case "i":
                        instructionStreamFilepath = flagValue;
                        break;
                    case "ast":
                        includeAST = true;
                        break;
                    case "nfa":
                        includeStateMachine = true;
                        break;
                    case "tokenstream":
                        includeTokenStream = true;
                        break;
                    case "grammar":
                        includeGrammar = true;
                        break;
                    case "lrtable":
                        includeLRTable = true;
                        break;
                    case "lrtrace":
                        includeLRTrace = true;
                        break;
                    case "lrstates":
                        includeLRTrace = true;
                        break;
                    case "symboltables":
                        includeSymbolTables = true;
                        break;
                    case "all":
                        includeAST = true;
                        includeGrammar = true;
                        includeStateMachine = true;
                        includeLRTable = true;
                        includeTokenStream = true;
                        includeLRTrace = true;
                        includeLRStates = true;
                        includeSymbolTables = true;
                        logDumpPath = "log.dmp";
                        break;
                }
            }

            LexicalConfigurationFile tokenConfigurationFile;
            Log.SetState("Lexical-Analysis");
            TokenParser? tokenParser = null;
            TokenStream? tokenStream = null;
            report = new Report($"{reportName}.html");

            // Build the parser table
            if (tokenConfigurationFilePath != null) {
                Log.WriteLineVerbose($"Parsing configuration file: {tokenConfigurationFilePath}");
                tokenConfigurationFile = new LexicalConfigurationFile(tokenConfigurationFilePath);
                Log.WriteLineVerbose($"Done.");

                var tokenParserTableGenerator = new TokenParserTableGenerator(tokenConfigurationFile);
                if (includeStateMachine) {
                    report.AddSection(tokenParserTableGenerator.GetReportSections());
                }

                tokenParser = new TokenParser(tokenParserTableGenerator.NFATable);
            }

            // Get the token stream
            if (sourcefile != null) {
                Debug.Assert(tokenParser != null, "TokenParser was not initialized.");
                tokenStream = tokenParser.ParseFile(sourcefile);
                if (includeTokenStream) {
                    report.AddSection(tokenParser.GetReportSections());
                }
            }

            ASTNode? ast = null;
            Log.SetState("Syntactic-Analysis");
            if (syntaxConfigurationFilePath != null) {

                var syntaxConfigFile = new SyntacticConfigurationFile(syntaxConfigurationFilePath);
                var productionTable = new ProductionTable(syntaxConfigFile);
                if (includeGrammar) {
                    report?.AddSection(productionTable.GetReportSection());
                }

                var clrStates = new CLRStateGenerator(productionTable, syntaxConfigFile);
                if (includeLRStates) {
                    report?.AddSection(clrStates.GetReportSection());
                }

                var lrTable = LRParsingTable.From(clrStates, productionTable);
                if (includeLRTable) {
                    report?.AddSection(lrTable.GetReportSection());
                }

                Debug.Assert(tokenStream != null, "Unable to perform synactic analysis with an empty or null token stream");
                var parser = new LRParser(syntaxConfigFile, tokenStream);
                ast = parser.Parse(lrTable, tokenStream);

                if (includeLRTrace) {
                    report?.AddSection(parser.GetReportSection());
                }

                Debug.Assert(ast != null, $"Failed to parse");

                if (includeAST) {
                    report?.AddSection(new ASTViewer(ast.ToJSON()));
                }
            }

            Log.SetState("Semantic-Analysis");
            if (semanticsFolder != null) {
                foreach (var file in Directory.GetFiles(semanticsFolder, "*.py")) {
                    new SemanticVisitor(file).Traverse(ast);
                }
                if (includeSymbolTables) {
                    report?.AddSection(SymbolTable.GetReportSection());
                }
            }

            Log.SetState("Code-Generation");
            if (codeGeneratorFolder != null) {
                var instructionStream = new InstructionStream();
                foreach (var file in Directory.GetFiles(codeGeneratorFolder, "*.py")) {
                    new CodeGeneratorVisiter(file, instructionStream).Traverse(ast);
                }
                File.WriteAllText(instructionStreamFilepath, instructionStream.ToString());
            }

            Log.SetState("Post-Build");
            if (postBuildScript != null) {
                var process = new System.Diagnostics.Process();
                process.StartInfo = new System.Diagnostics.ProcessStartInfo() {
                    FileName = "powershell",
                    Arguments = postBuildScript,
                    WorkingDirectory = cwd,
                    RedirectStandardOutput = true
                };
                process.Start();
                process.WaitForExit();
                Log.WriteLineVerbose(process.StandardOutput.ReadToEnd());
            }
        }

        private static void MakeSureFolderExists(string path) {
            if (!Directory.Exists(path)) {
                Directory.CreateDirectory(path);
            }
        }

        private static void MakeSureFileExists(string path) {
            if (!File.Exists(path)) {
                using (var file = File.Create(path)) {

                }
            }
        }
    }
}
