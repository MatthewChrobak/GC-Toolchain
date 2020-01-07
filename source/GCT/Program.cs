﻿using Core;
using LexicalAnalysis;
using System;
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
                }

                match = match.NextMatch();
            }

            LexicalConfigurationFile tokenConfigurationFile;
            if (tokenConfigurationFilePath != null) {
                try {
                    tokenConfigurationFile = new LexicalConfigurationFile(tokenConfigurationFilePath);
                } catch (Exception e) {
                    Log.WriteLineError(e.Message);
                    return;
                }
            }
        }
    }
}