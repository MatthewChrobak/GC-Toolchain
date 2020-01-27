using Core;
using Core.ReportGeneration;
using LexicalAnalysis;
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

        public static void Main(string[] args) {
            // If no args are given, take some.
            if (args.Length == 0) {
                Main(Console.ReadLine().Split(' '));
                return;
            }

            try {
                RealMain(args);
            } catch (AssertionFailedException e) {
                Log.WriteLineError($"Unable to continue due to exception of type {e.GetType()} being thrown during lexical analysis. Exiting.");
            } finally {
                report.Save();
            }
        }

        private static void RealMain(string[] args) {
            string? tokenConfigurationFilePath = null;
            string? syntaxConfigurationFilePath = null;
            string? reportName = null;
            string? sourcefile = null;

            // Extract arguments.
            var match = Regex.Match(string.Join(' ', args), @"\-(\w+)(\s+([^\-\r\n]+|\"".+?\""|\'.+?\'))?");
            while (match.Success) {

                string flagKey = match.Groups[1].Value.Trim();
                string flagValue = match.Groups[2].Value.Trim();

                switch (flagKey) {
                    case "f":
                        MakeSureFolderExists(flagValue);

                        tokenConfigurationFilePath = flagValue + "/tokens.config";
                        syntaxConfigurationFilePath = flagValue + "/syntax.config";
                        sourcefile = flagValue + "/program.source";
                        reportName = flagValue + "/report";

                        MakeSureFileExists(tokenConfigurationFilePath);
                        MakeSureFileExists(syntaxConfigurationFilePath);
                        MakeSureFileExists(sourcefile);
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
                }

                match = match.NextMatch();
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
                report.AddSection(tokenParserTableGenerator.GetReportSections());

                tokenParser = new TokenParser(tokenParserTableGenerator.NFATable);
            }

            // Get the token stream
            if (sourcefile != null) {
                Debug.Assert(tokenParser != null, "TokenParser was not initialized.");

                tokenStream = tokenParser.ParseFile(sourcefile);
                report.AddSection(tokenParser.GetReportSections());
            }

            Log.SetState("Syntactic-Analysis");
            if (syntaxConfigurationFilePath != null) {

                var syntaxConfigFile = new SyntacticConfigurationFile(syntaxConfigurationFilePath);
                var productionTable = new ProductionTable(syntaxConfigFile);
                report?.AddSection(productionTable.GetReportSection());
                var clrStates = new CLRStateGenerator(productionTable, syntaxConfigFile);
                var lrTable = LRParsingTable.From(clrStates, productionTable);
                report?.AddSection(lrTable.GetReportSection());

                Debug.Assert(tokenStream != null, "Unable to perform synactic analysis with an empty or null token stream");
                var parser = new LRParser(syntaxConfigFile, tokenStream);
                var ast = parser.Parse(lrTable, tokenStream);
                report?.AddSection(parser.GetReportSection());
                if (ast == null) {
                    Log.WriteLineError("Failed to parse.");
                } else {
                    report?.AddSection(new ASTViewer(ast.ToJSON()));
                }
            }

            report.AddSection(Log.GetReportSections());
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
