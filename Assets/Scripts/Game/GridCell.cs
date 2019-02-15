namespace Ozh.Game {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class GridCell : MonoBehaviour {

        public Grid Grid { get; private set; }
        public CellPosition Position { get; private set; }

        public void Construct(Grid grid ) {
            Grid = grid;
        }

        public void SetPosition(CellPosition position) {
            Position = position;
            transform.localPosition = Grid.GetWorldGridCellPosition(Position);
        }
    }

}