using System;
using System.Collections.Generic;
using System.IO;

namespace Core.Config
{
    public partial class ConfigurationFile
    {
        public const string SectionSymbol = "#";
        // Tag -> Sections
        private readonly Dictionary<string, List<ConfigSection>> _sections = new Dictionary<string, List<ConfigSection>>();
        public readonly string ConfigFileName;

        public ConfigurationFile(string[] configurationFileContents, string configFileName) {
            // A configuration file is a textfile that stores data in sections.
            // Section definition starts with the section symbol # and is followed by a Tag, and an optional header.
            // Subsequent non-empty lines that do not start with the section symbol # make up the body of the section.
            // Sections are aggregated under a common tag.

            // Configuration file is structured as 
            /* 
             * #Tag Header
             * Body
             */
            this.CacheID = Guid.NewGuid().ToString();
            this.ConfigFileName = configFileName;
            this.Parse(configurationFileContents);
        }

        public ConfigurationFile(string configurationFilePath) : this(File.ReadAllLines(configurationFilePath), new FileInfo(configurationFilePath).Name) {
            this.CacheID = MD5.HashFile(configurationFilePath);
        }

        private void Parse(string[] lines) {
            int ptr = 0;

            string? GetNextLine() {
                if (ptr >= lines.Length) {
                    return null;
                }
                return lines[ptr++];
            }

            string? headerLine;
            // Keep looping until we see a non-empty line.
            while ((headerLine = GetNextLine()) != null) {

                // TODO: How to handle a non-empty line that does not belong in a section?
                //       Option 1: Throw exception for badly formatted config file.
                //       Option 2: Allow possible error and keep parsing. <- currently implemented.
                if (!headerLine.StartsWith(SectionSymbol)) {
                    continue;
                }

                // We're defining a new section. Find the start of the NEXT section.
                // StartPtr is subtracted by - 1 because GetNextLine increments ptr.
                // EndPtr is not because upperbound is exclusive.
                int sectionStartPtr = ptr - 1;
                int bodyStartPtr = ptr;
                string? bodyLine;
                while (!String.IsNullOrEmpty(bodyLine = GetNextLine())) {
                    if (bodyLine.StartsWith(SectionSymbol)) {
                        ptr--;
                        break;
                    }
                }
                int bodyEndPtr = ptr;

                this.AddSection(new ConfigSection(headerLine, lines[bodyStartPtr..bodyEndPtr], this.ConfigFileName, sectionStartPtr));
            }
        }

        private void AddSection(ConfigSection section) {
            string tag = section.Tag.ToLowerInvariant();
            
            if (!this._sections.ContainsKey(tag)) {
                this._sections[tag] = new List<ConfigSection>();
            }
            this._sections[tag].Add(section);
        }

        public IEnumerable<ConfigSection> GetSections(string tag) {
            tag = tag.ToLowerInvariant();
            if (!this._sections.ContainsKey(tag)) {
                return Array.Empty<ConfigSection>();
            }
            return this._sections[tag];
        }
    }

    public partial class ConfigurationFile : ICachable
    {
        private readonly string CacheID;

        public string GetCacheID() {
            return this.CacheID;
        }
    }
}
