using Core.ReportGeneration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ASTVisitor
{
    public class SymbolTable  
    {
        private static Dictionary<string, SymbolTable> GlobalScope;
        
        static SymbolTable() {
            GlobalScope = new Dictionary<string, SymbolTable>();
        }

        public static bool Exists(string id) {
            return GlobalScope.ContainsKey(id);
        }

        public static SymbolTable GetOrCreate(string id) {
            if (!GlobalScope.ContainsKey(id)) {
                GlobalScope[id] = new SymbolTable();
            }
            return GlobalScope[id];
        }

        private List<AssociativeArray> _rows;
        public IEnumerable<AssociativeArray> Rows => this._rows;


        public SymbolTable() {
            this._rows = new List<AssociativeArray>();
        }

        public AssociativeArray CreateRow() {
            var row = new AssociativeArray();
            this._rows.Add(row);
            return row;
        }

        public bool RowExistsWhere(string column, dynamic value) {
            return this._rows.Any(row => row[column].Equals(value));
        }

        public IEnumerable<AssociativeArray> GetRowsWhere(string column, dynamic value) {
            return this._rows.Where(row => row[column].Equals(value));
        }

        public static ReportSection GetReportSection() {
            return new SymbolTableReportSection(GlobalScope);
        }
    }
}