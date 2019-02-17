namespace Ozh.Game {
    using Ozh.Services;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public struct CellPosition {
        public CellPosition( Grid grid, int row, int column) {
            Grid = grid;
            Row = row;
            Column = column;
        }

        public int Row { get; }
        public int Column { get; }
        public Grid Grid { get; }
        public GridCell Cell {
            get {
                if (IsValid) {
                    return Grid[Row, Column];
                }
                return null;
            }
        }

        public bool IsValid
            => Row >= 0 && Row < Grid.NumRows && Column >= 0 && Column < Grid.NumColumns;

        public CellPosition GetAdjacentPosition(Swipe direction ) {
            switch (direction) {
                case Swipe.Down:
                    return new CellPosition(Grid, Row + 1, Column);
                case Swipe.Up:
                    return new CellPosition(Grid, Row - 1, Column);
                case Swipe.Left:
                    return new CellPosition(Grid, Row, Column - 1);
                case Swipe.Right:
                    return new CellPosition(Grid, Row, Column + 1);
                default:
                    throw new ArgumentException($"invalid direction: {direction}");
            }
        }

        public static bool operator==(CellPosition left, CellPosition right ) {
            return left.Row == right.Row && left.Column == right.Column;
        }

        public static bool operator!=(CellPosition left, CellPosition right) {
            return !(left == right);
        }

        public override int GetHashCode() {
            return (Row << 2) ^ Column;
        }

        public override bool Equals(object obj) {
            if((obj == null) || !this.GetType().Equals(obj.GetType())) {
                return false;
            } else {
                CellPosition cellPosition = (CellPosition)obj;
                return (Row == cellPosition.Row) && (Column == cellPosition.Column);
            }
        }


    }

}