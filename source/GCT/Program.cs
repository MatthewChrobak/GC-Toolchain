using Core;
using Core.ReportGeneration;
using LexicalAnalysis;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace GCT
{
    public static class Program
    {
        private static void Main(string[] args) {
            // If no args are given, take some.
            if (args.Length == 0) {
                Main(Console.ReadLine().Split(' '));
            }

            string? tokenConfigurationFilePath = null;
            string? reportName = null;

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
                }

                match = match.NextMatch();
            }

            LexicalConfigurationFile tokenConfigurationFile;
            Log.SetState("Lexical-Analysis");
            var report = new Report();
            if (tokenConfigurationFilePath != null) {
                try {
                    Log.WriteLineVerbose($"Parsing configuration file: {tokenConfigurationFilePath}");
                    tokenConfigurationFile = new LexicalConfigurationFile(tokenConfigurationFilePath);
                    Log.WriteLineVerbose($"Done.");
                } catch (Exception e) {
                    Log.WriteLineError($"Unable to continue due to exception of type {e.GetType()} being thrown during lexical analysis. Exiting.");
                    return;
                }

                var tokenizer = new Tokenizer(tokenConfigurationFile);
            }

            report.AddSectionToTop(Log.GetSections());

            if (reportName != null) {
                File.WriteAllText($"{reportName}.html", report.ToHTML());
            }
        }
    }
}
