namespace Ozh.Game {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using System.Linq;
    using System.Text;

    public class CellRange : IEnumerable<GridCell> {

        public IEnumerable<GridCell> Cells { get; private set; }

        public CellRange(IEnumerable<GridCell> cells ) {
            Cells = cells;
        }

        public IEnumerator<GridCell> GetEnumerator() {
            return Cells.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public int MinRow
            => Cells.Min(c => c.Row);

        public int MaxRow
            => Cells.Max(c => c.Row);

        public int MinColumn
            => Cells.Min(c => c.Column);

        public int MaxColumn
            => Cells.Max(c => c.Column);

        public bool IsEmpty 
            => Cells.Count() == 0;

        public int NumRows
            => (MaxRow - MinRow + 1);

        public int NumColumns
            => (MaxColumn - MinColumn + 1);

        public static CellRange FromColumn(GridCell[,] cells, int column) {
            int rowCount = cells.GetUpperBound(0) + 1;
            GridCell[] columnCells = new GridCell[rowCount];
            for(int i = 0; i < rowCount; i++ ) {
                columnCells[i] = cells[i, column];
            }
            return new CellRange(columnCells);
        }

        public override string ToString() {
            StringBuilder sb = new StringBuilder();
            if(IsEmpty) {
                sb.AppendLine("Range: Empty");
                return sb.ToString();
            } else {
                return $"Range: column: {MinColumn}, rows: [{MinRow}, {MaxRow}] length: {Cells.Count()}";
            }
        }
    }

}