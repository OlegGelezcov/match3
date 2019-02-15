namespace Ozh.Game {
    using Ozh.Services;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class Grid {

        private IGridLevelRepository gridLevelRepository;
        private IPrefabRepository<string> prefabRepository;
        private float CellWidth { get;  set; }
        private float CellHeight { get;  set; }
        private float CellPadding { get;  set; }
        private int NumRows { get; set; }
        private int NumColumns { get;  set; }

        private Transform GridParent { get; set; }
        private GridCell[,] Cells { get; set; }

        public Grid(IGridLevelRepository gridLevelRepository, IPrefabRepository<string> prefabRepository) {
            this.gridLevelRepository = gridLevelRepository;
            this.prefabRepository = prefabRepository;
        }

        public void MakeGrid(int gridLevel) {
            ClearGrid();
            GridLevelData data = gridLevelRepository.GetGridLevelData(gridLevel);
            CellWidth = data.cellWidth;
            CellHeight = data.cellHeight;
            CellPadding = data.cellPadding;
            NumRows = data.numRows;
            NumColumns = data.numColumns;
            Cells = new GridCell[NumRows, NumColumns];

            GameObject gridParent = new GameObject("GridParent");
            gridParent.transform.localPosition = Vector3.zero;
            gridParent.transform.localRotation = Quaternion.identity;
            GridParent = gridParent.transform;

            GameObject cellPrefab = prefabRepository.GetPrefab("gridcell");

            for(int i = 0; i < NumRows; i++ ) {
                for(int j = 0; j < NumColumns; j++ ) {
                    Cells[i, j] = cellPrefab.MakeInstance(GridParent).GetComponent<GridCell>();
                    Cells[i, j].name = $"Cell_{i}_{j}";
                    Cells[i, j].Construct(this);
                    Cells[i, j].SetPosition(new CellPosition(i, j));

                }
            }
        }

        public void ClearGrid() {
            if(Cells != null ) {
                for(int i = 0; i < NumRows; i++ ) {
                    for(int j = 0; j < NumColumns; j++ ) {
                        GridCell cell = Cells[i, j];
                        if(cell != null && cell.gameObject) {
                            GameObject.Destroy(cell.gameObject);                           
                        }
                        Cells[i, j] = null;
                    }
                }
                Cells = null;
            }
            if(GridParent != null && GridParent.gameObject) {
                GameObject.Destroy(GridParent.gameObject);
                GridParent = null;
            }
        }

        public Vector3 GetWorldGridCellPosition(CellPosition cellPosition ) {
            float totalWidth = CellWidth * NumRows + CellPadding * (NumRows - 1);
            float totalHeight = CellHeight * NumColumns + CellPadding * (NumColumns - 1);
            float startX = -totalWidth * .5f + CellWidth * .5f;
            float startY = totalHeight * .5f - CellHeight * .5f;
            return new Vector3(
                startX + cellPosition.Column * (CellWidth + CellPadding),
                startY - cellPosition.Row * (CellHeight + CellPadding), 0f);
        }

    }
}
