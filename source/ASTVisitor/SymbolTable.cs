using Core.ReportGeneration;
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
        private Dictionary<string, string> _meta;

        public SymbolTable() {
            this._rows = new List<AssociativeArray>();
            this._meta = new Dictionary<string, string>();
        }

        public (AssociativeArray row, int index) CreateRow() {
            var row = new AssociativeArray();
            int index = this._rows.Count;
            this._rows.Add(row);
            return (row, index);
        }

        public AssociativeArray RowAt(int index) {
            return this._rows[index];
        }

        public bool RowExistsWhere(string column, dynamic value) {
            return this._rows.Any(row => row.Contains(column) && row[column].Equals(value));
        }

        public AssociativeArray GetRowWhere(string column1, dynamic value1, string column2, dynamic value2) {
            return this._rows.Where(row => row.Contains(column1) && row.Contains(column2) && row[column1].Equals(value1) && row[column2].Equals(value2)).First();
        }

        public bool RowExistsWhere(string column1, dynamic value1, string column2, dynamic value2) {
            return this._rows.Any(row => row.Contains(column1) && row.Contains(column2) && row[column1].Equals(value1) && row[column2].Equals(value2));
        }

        public IEnumerable<AssociativeArray> GetRowsWhere(string column, dynamic value) {
            return this._rows.Where(row => row.Contains(column) && row[column].Equals(value));
        }

        public IEnumerable<AssociativeArray> GetRowsWhere(string column1, dynamic value1, string column2, dynamic value2) {
            return this._rows.Where(row => row.Contains(column1) && row.Contains(column2) && row[column1].Equals(value1) && row[column2].Equals(value2));
        }

        public static ReportSection GetReportSection() {
            return new SymbolTableReportSection(GlobalScope);
        }

        public string GetMetaData(string key) {
            return this._meta.ContainsKey(key) ? this._meta[key] : string.Empty;
        }

        public void SetMetaData(string key, string value) {
            this._meta[key] = value;
        }
    }
}