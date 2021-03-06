﻿using Core.ReportGeneration;
using System.Linq;

namespace LexicalAnalysis
{
    internal class TokenStreamReportSection : ReportSection
    {
        private TokenStream _stream;

        public TokenStreamReportSection(string header, TokenStream stream) : base(header) {
            this._stream = stream;
        }

        public override string GetContent() {
            return string.Join("", this._stream.Select(token => token.ToString()));
        }
    }
}