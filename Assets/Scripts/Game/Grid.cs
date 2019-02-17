namespace Ozh.Game {
    using Ozh.Services;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using UniRx;
    using UnityEngine;

    public interface IGridService : IGameService {
        void MakeGrid(GridLevelData data);
        void ClearGrid();
    }

    public class Grid : IGridService {

        private IElementSpawner ElementSpawner { get; set; }
        private ISwipeService SwipeService { get; set; }
        private IGameStateService GameStateService { get; set; }
        private IPrefabRepository<string> PrefabRepository { get; set; }
        private ICoroutineService CoroutineService { get; set; }

        private Transform GridParent { get; set; }
        private GridCell[,] Cells { get; set; }
        private IDisposable swipeDisposable;
        private GridLevelData Data { get; set; }




        public void Setup(IServiceCollection services) {
            ElementSpawner = services.Resolve<IElementSpawner>();
            SwipeService = services.Resolve<ISwipeService>();
            GameStateService = services.Resolve<IGameStateService>();
            PrefabRepository = services.Resolve<IResourceService>().GetRepository<IPrefabRepository<string>>();
            CoroutineService = services.Resolve<ICoroutineService>();

            swipeDisposable = SwipeService.SwipeObservable.Subscribe(info => {
                if (info.Phase == SwipePhase.Start) {
                    var hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(info.InputPosition), Vector2.zero);
                    if (hit.transform) {
                        Debug.Log(hit.transform.name);
                        var gridElement = hit.transform.GetComponent<GridElement>();
                        if (gridElement) {
                            SelectionChangedObservable.OnNext(new SelectionChangedArgs {
                                Element = gridElement
                            });
                        }
                    }
                } else if (info.Phase == SwipePhase.Continue && info.Direction != Swipe.None) {
                    if (HasSelection && !IsGridLocked) {
                        MoveObservable.OnNext(new MoveElementArgs { Direction = info.Direction });
                    }
                }
            });
        }

        private float CellWidth => Data?.cellWidth ?? 0;

        private float CellHeight => Data?.cellHeight ?? 0;

        private float CellPadding => Data?.cellPadding ?? 0;

        public int NumRows => Data?.numRows ?? 0;

        public int NumColumns => Data?.numColumns ?? 0;

        public float ElementVerticalSpeed
            => TotalHeight / 0.25f;

        public float ElementHorizontalSpeed
            => TotalWidth / 0.25f;

        private float TotalHeight
            => CellHeight * NumColumns + CellPadding * (NumColumns - 1);

        private float TotalWidth
            => CellWidth * NumRows + CellPadding * (NumRows - 1);

        public float SlideSpeed(Swipe direction) {
            if(direction == Swipe.Left || direction == Swipe.Right ) {
                return ElementHorizontalSpeed / 3;
            } else {
                return ElementVerticalSpeed / 3;
            }
        }

        public Subject<SelectionChangedArgs> SelectionChangedObservable { get; } =
            new Subject<SelectionChangedArgs>();
        public Subject<MoveElementArgs> MoveObservable { get; } =
            new Subject<MoveElementArgs>();




        public void MakeGrid(GridLevelData data) {
            ClearGrid();
            Data = data;
            Cells = new GridCell[NumRows, NumColumns];

            GameObject gridParent = new GameObject("GridParent");
            gridParent.transform.localPosition = Vector3.zero;
            gridParent.transform.localRotation = Quaternion.identity;
            GridParent = gridParent.transform;

            GameObject cellPrefab = PrefabRepository.GetPrefab("gridcell");

            for(int i = 0; i < NumRows; i++ ) {
                for(int j = 0; j < NumColumns; j++ ) {
                    Cells[i, j] = cellPrefab.MakeInstance(GridParent).GetComponent<GridCell>();
                    Cells[i, j].name = $"Cell_{i}_{j}";
                    Cells[i, j].Construct(this, new CellPosition(this, i, j));
                }
            }

            FillGrid(() => {
                ScanAndFill();
            });


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

        public GridCell this[int i, int j] {
            get {
                return Cells[i, j];
            }
        }



        private bool HasSelection {
            get {
                for(int i = 0; i < NumRows; i++ ) {
                    for(int j = 0; j < NumColumns; j++ ) {
                        if(Cells[i, j].IsSelected) {
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        private bool IsGridLocked {
            get {
                for(int i = 0; i < NumRows; i++ ) {
                    for(int j = 0; j <NumColumns; j++ ) {
                        if(Cells[i, j].State == GridCellState.Locked) {
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        private bool IsGridNormal {
            get {
                for(int i = 0; i < NumRows; i++ ) {
                    for(int j = 0; j < NumColumns; j++ ) {
                        if(Cells[i, j].State != GridCellState.Normal ) {
                            return false;
                        }
                    }
                }
                return true;
            }
        }

        

        public Vector3 GetWorldGridCellPosition(CellPosition cellPosition ) {
            float totalWidth = TotalWidth;
            float totalHeight = TotalHeight;
            float startX = -totalWidth * .5f + CellWidth * .5f;
            float startY = totalHeight * .5f - CellHeight * .5f;
            return new Vector3(
                startX + cellPosition.Column * (CellWidth + CellPadding),
                startY - cellPosition.Row * (CellHeight + CellPadding), 0f);
        }


        public void CheckMove(GridCell fromCell, GridCell toCell, Action onNotFoundMatch) {
            var rowRange = ScanRowAtCell(toCell, fromCell);
            var rowRange2 = ScanRowAtCell(fromCell, toCell);
            var columnRange = ScanColumnAtCell(toCell, fromCell);
            var columnRange2 = ScanColumnAtCell(fromCell, toCell);

            int maxLength = new List<CellRange> { rowRange, rowRange2, columnRange, columnRange2 }.Max(r => r.Count());

            if (rowRange.Count() >= 3 && (rowRange.Count() == maxLength)) {
                ExchangeCells(fromCell, toCell, () => {
                    CoroutineService.ExecuteCoroutine(DestroyScanAndFill(rowRange));
                    fromCell.SetState(GridCellState.Normal);
                    fromCell.ToggleSelection(false);
                });
                return;
            }
            if (rowRange2.Count() >= 3 && (rowRange2.Count() == maxLength)) {
                ExchangeCells(fromCell, toCell, () => {
                    CoroutineService.ExecuteCoroutine(DestroyScanAndFill(rowRange2));
                    toCell.SetState(GridCellState.Normal);
                    fromCell.ToggleSelection(false);
                });
                return;
            }

            if (columnRange.Count() >= 3 && (columnRange.Count() == maxLength)) {
                ExchangeCells(fromCell, toCell, () => {
                    CoroutineService.ExecuteCoroutine(DestroyScanAndFill(columnRange));
                    fromCell.SetState(GridCellState.Normal);
                    fromCell.ToggleSelection(false);
                });
                return;
            }

            if (columnRange2.Count() >= 3 && (columnRange2.Count() == maxLength)) {
                ExchangeCells(fromCell, toCell, () => {
                    CoroutineService.ExecuteCoroutine(DestroyScanAndFill(columnRange2));
                    toCell.SetState(GridCellState.Normal);
                    fromCell.ToggleSelection(false);
                });
                return;
            }

            onNotFoundMatch?.Invoke();
        }

        /// <summary>
        /// Scan all grid for matches and fill empty spaces
        /// </summary>
        /// <param name="fillBefore"></param>
        public void ScanAndFill(bool fillBefore = false) {
            if (fillBefore) {
                FillGrid(() => CoroutineService.ExecuteCoroutine(ScanAndFillImpl()));
            } else {
                CoroutineService.ExecuteCoroutine(ScanAndFillImpl());
            }
        }

        private IEnumerator ScanAndFillImpl() {
            int rowCursor = NumRows - 1;
            int colCursor = 0;
            while(rowCursor >= 0 ) {
                var range = ScanRowForMatch(rowCursor, 0);
                if(range.Count() >= 3 ) {
                    DestroyRange(range);
                    yield return new WaitUntil(() => IsGridNormal);
                    yield return FillGridImpl(() => { });
                    yield return ScanAndFillImpl();
                } else {
                    rowCursor--;
                }
            }
            while(colCursor < NumColumns) {
                var range = ScanColumnForMatch(NumRows - 1, colCursor);
                if(range.Count() >= 3 ) {
                    DestroyRange(range);
                    yield return new WaitUntil(() => IsGridNormal);
                    yield return FillGridImpl(() => { });
                    yield return ScanAndFillImpl();
                } else {
                    colCursor++;
                }
            }
        }


        public void FillGrid(Action afterFill = null ) {
            CoroutineService.ExecuteCoroutine(FillGridImpl(afterFill));
        }

        private IEnumerator FillGridImpl(Action afterFill) {
            for(int j = 0; j < NumColumns; j++ ) {
                ShiftColumn(j);
                CellRange jColumn = CellRange.FromColumn(Cells, j);
                yield return new WaitUntil(() => {
                    return jColumn.All(c => !c.IsEmpty && c.State == GridCellState.Normal);
                });

            }
            afterFill?.Invoke();
        }

        private void ShiftColumn(int column) {
            CoroutineService.ExecuteCoroutine(ShiftColumnImpl(column));
        }

        private IEnumerator ShiftColumnImpl(int column ) {
            CellRange wholeColumn = CellRange.FromColumn(Cells, column);
            yield return new WaitUntil(() => wholeColumn.All(c => c.State == GridCellState.Normal));
            CellRange emptyRange = GetFirstEmptyColumnRange(column);
            if(emptyRange.Count() == 0 ) {
                yield break;
            }
            CellRange nonEmptyRange = GetNonEmptyColumnRange(emptyRange.MinRow - 1, column);
            if(nonEmptyRange.Count() != 0 ) {
                //we drop items outside grid
                foreach(var cell in nonEmptyRange) {
                    cell.DropElementDownOn(emptyRange.Count());
                }
                ShiftColumn(column);
            } else {
                //we drop items on grid
                int outIndex = -1;
                foreach(var cell in emptyRange ) {
                    GameObject elementObject = ElementSpawner.Spawn();
                    elementObject.transform.SetParent(GridParent);
                    GridElement element = elementObject.GetComponent<GridElement>();
                    element.Construct(this, new CellPosition(this, outIndex, column));
                    outIndex--;
                    cell.DropOnMe(element);
                }
            }
        }

        private CellRange GetFirstEmptyColumnRange(int column) {
            List<GridCell> emptyCells = new List<GridCell>();
            int cursor = NumRows - 1;

            while(cursor >= 0 ) {
                if(Cells[cursor, column].IsEmpty) {
                    break;
                }
                cursor--;
            }

            while(cursor >= 0) {
                if(Cells[cursor, column].IsEmpty) {
                    emptyCells.Add(Cells[cursor, column]);
                    cursor--;
                } else {
                    break;
                }
            }
            return new CellRange(emptyCells);
        }

        private CellRange GetNonEmptyColumnRange(int startRow, int column) {
            int cursor = startRow;

            List<GridCell> nonEmptyCells = new List<GridCell>();

            while(cursor >= 0 ) {
                if(!Cells[cursor, column].IsEmpty) {
                    break;
                }
                cursor--;
            }

            while(cursor >= 0 ) {
                if(Cells[cursor, column].IsEmpty) {
                    break;
                }
                nonEmptyCells.Add(Cells[cursor, column]);
                cursor--;
            }

            return new CellRange(nonEmptyCells);
        }


        private IEnumerator DestroyScanAndFill(CellRange range) {
            DestroyRange(range);
            yield return new WaitUntil(() => IsGridNormal);
            ScanAndFill(true);
        }


        private void ExchangeCells(GridCell firstCell, GridCell secondCell, Action onCompleted) {
            firstCell.ExchangeWith(secondCell, onCompleted);
        }

        private CellRange ScanColumnAtCell(GridCell finalCell, GridCell sourceCell ) {
            List<GridCell> result = new List<GridCell> { finalCell };

            bool includeSourceCell = (finalCell.ElementType == sourceCell.ElementType);

            //move up
            int row = finalCell.Row - 1;
            while(row >= 0 ) {
                var testCell = Cells[row, finalCell.Column];
                if(testCell.IsEmpty || testCell.ElementType != sourceCell.ElementType) {
                    break;
                } else {
                    if (testCell != sourceCell) {
                        result.Add(testCell);
                        row--;
                    } else {
                        if(includeSourceCell) {
                            result.Add(testCell);
                            row--;
                        } else {
                            break;
                        }
                    } 
                }
            }

            //move down
            row = finalCell.Row + 1;
            while(row < NumRows ) {
                var testCell = Cells[row, finalCell.Column];
                if(testCell.IsEmpty || testCell.ElementType != sourceCell.ElementType ) {
                    break;
                } else {
                    if (testCell != sourceCell) {
                        result.Insert(0, testCell);
                        row++;
                    } else {
                        if(includeSourceCell) {
                            result.Insert(0, testCell);
                            row++;
                        } else {
                            break;
                        }
                    }
                }
            }
            result = result.Distinct().ToList();
            if (result.Count >= 3) {
                return new CellRange(result);
            } else {
                return new CellRange(new List<GridCell>());
            }
        }

        private CellRange ScanRowAtCell(GridCell finalCell, GridCell sourceCell ) {
            List<GridCell> result = new List<GridCell> { finalCell };

            bool includeSourceCell = (finalCell.ElementType == sourceCell.ElementType);

            //move right
            int col = finalCell.Column + 1;
            while(col < NumColumns) {
                var testCell = Cells[finalCell.Row, col];
                if(testCell.IsEmpty || 
                    testCell.ElementType != sourceCell.ElementType) {
                    break;
                } else {
                    if (testCell != sourceCell) {
                        result.Add(testCell);
                        col++;
                    } else {
                        if(includeSourceCell) {
                            result.Add(testCell);
                            col++;
                        } else {
                            break;
                        }
                    }
                }
            }

            //move left
            col = finalCell.Column - 1;
            while(col >= 0 ) {
                var testCell = Cells[finalCell.Row, col];
                if(testCell.IsEmpty || testCell.ElementType != sourceCell.ElementType) {
                    break;
                } else {
                    if (testCell != sourceCell) {
                        result.Insert(0, testCell);
                        col--;
                    } else {
                        if(includeSourceCell) {
                            result.Insert(0, testCell);
                            col--;
                        } else {
                            break;
                        }
                    }
                }
            }
            result = result.Distinct().ToList();
            if(result.Count >= 3 ) {
                return new CellRange(result);
            } else {
                return new CellRange(new List<GridCell>());
            }
        }

        private CellRange ScanRowForMatch(int row, int startColumn ) {

            int cursor = startColumn;
            while(cursor < NumColumns ) {
                var c = Cells[row, cursor];
                if(c.IsEmpty) {
                    cursor++;
                } else {
                    break;
                }
            }


            List<GridCell> tempList = new List<GridCell>();

            if (cursor < NumColumns) {
                tempList.Add(Cells[row, cursor]);
                cursor++;
            }

            while (cursor < NumColumns ) {
                GridCell testCell = Cells[row, cursor];
                if(testCell.IsEmpty) {
                    if(tempList.Count >= 3 ) {
                        break;
                    } else {
                        tempList.Clear();
                        cursor++;
                        return ScanRowForMatch(row, cursor);
                    }
                } else {
                    if(tempList.Last().ElementType == testCell.ElementType ) {
                        tempList.Add(testCell);
                        cursor++;
                    } else {
                        if (tempList.Count() >= 3) {
                            break;
                        } else {
                            tempList.Clear();
                            //cursor++;
                            return ScanRowForMatch(row, cursor);
                        }
                    }
                }
            }

            if(tempList.Count >= 3 ) {
                return new CellRange(tempList);
            } else {
                return new CellRange(new List<GridCell>());
            }
        }

        private CellRange ScanColumnForMatch(int startRow, int column ) {
            int cursor = startRow;
            while(cursor >= 0 ) {
                var c = Cells[cursor, column];
                if(c.IsEmpty) {
                    cursor--;
                } else {
                    break;
                }
            }
            List<GridCell> tempList = new List<GridCell>();
            if(cursor >= 0 ) {
                tempList.Add(Cells[cursor, column]);
                cursor--;
            }
            while(cursor >= 0 ) {
                GridCell testCell = Cells[cursor, column];
                if(testCell.IsEmpty) {
                    if(tempList.Count >= 3 ) {
                        break;
                    } else {
                        tempList.Clear();
                        cursor--;
                        return ScanColumnForMatch(cursor, column);
                    }
                } else {
                    if(tempList.Last().ElementType == testCell.ElementType ) {
                        tempList.Add(testCell);
                        cursor--;
                    } else {
                        if (tempList.Count >= 3) {
                            break;
                        } else {
                            tempList.Clear();
                            //cursor--;
                            return ScanColumnForMatch(cursor, column);
                        }
                    }
                }
            }

            if (tempList.Count >= 3) {
                return new CellRange(tempList);
            } else {
                return new CellRange(new List<GridCell>());
            }
        }

        public void DestroyRange(CellRange range) {
            foreach(var cell in range) {
                cell.SetState(GridCellState.Locked);
            }
            CoroutineService.ExecuteCoroutine(DestroyRangeImpl(range));
        }

        private IEnumerator DestroyRangeImpl(CellRange range) {

            yield return new WaitForSeconds(0.3f);

            foreach (var cell in range) {
                yield return new WaitForSeconds(0.03f);
                cell.DestroyElement(false);
            }

            yield return new WaitForSeconds(0.4f);
            range.ToList().ForEach(c => c.SetState(GridCellState.Normal));
        }

        public void IncrementScore(int count ) {
            GameStateService.Score.Value += count;
        }

        public override string ToString() {
            StringBuilder sb = new StringBuilder();
            for(int i = 0; i < NumRows; i++ ) {
                for(int j = 0; j < NumColumns; j++ ) {
                    sb.Append(Cells[i, j].ToString() + "  ");
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }
    }

    public class SelectionChangedArgs {
        public GridElement Element { get; set; }
    }

    public class MoveElementArgs {
        public Swipe Direction { get; set; }
    }
}
