using Core;
using Core.ReportGeneration;
using LexicalAnalysis;
using SyntacticAnalysis;
using SyntacticAnalysis.CLR_V3;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace GCT
{
    public static class Program
    {
        public static void Main(string[] args) {
            // If no args are given, take some.
            if (args.Length == 0) {
                Main(Console.ReadLine().Split(' '));
            }

            try {
                RealMain(args);
            } catch (AssertionFailedException e) {
                Log.WriteLineError($"Unable to continue due to exception of type {e.GetType()} being thrown during lexical analysis. Exiting.");
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
            var report = new Report();

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

            report.AddSection(Log.GetReportSections());

            if (reportName != null) {
                File.WriteAllText($"{reportName}.html", report.ToHTML());
            }

            Log.SetState("Syntactic-Analysis");
            if (syntaxConfigurationFilePath != null) {

                var syntaxConfigFile = new SyntacticConfigurationFile(syntaxConfigurationFilePath);
                var productionTable = new ProductionTable(syntaxConfigFile);
                var lrTable = new CLRTable(productionTable, syntaxConfigFile);
            }

            //Debug.Assert(tokenStream != null, "Unable to perform synactic analysis with an empty or null token stream.");
        }
    }
}
