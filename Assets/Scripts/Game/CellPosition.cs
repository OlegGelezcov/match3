namespace Ozh.Game {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public struct CellPosition {
        public CellPosition(int row, int column) {
            Row = row;
            Column = column;
        }

        public int Row { get; }
        public int Column { get; }
    }

}