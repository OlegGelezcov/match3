namespace Ozh.Game {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UniRx;

    public class GridCell : MonoBehaviour {

        public GameObject selection;

        public Grid Grid { get; private set; }
        public CellPosition Position { get; private set; }
        public GridElement Element { get; private set; }
        public GridCellState State { get; private set; } = GridCellState.Normal;

        public void AttachElement(GridElement element, Action onMoveCompleted = null) {
            //StartCoroutine(AttachElementImpl(element, onMoveCompleted));
            if (Element != null) {
                Element.DetachFromCell();
            }
            Element = element;
            Element.AttachToCell(this, onMoveCompleted);
        }



        public void Construct(Grid grid, CellPosition position ) {
            Grid = grid;
            SetPosition(position);
            Grid.SelectionChangedObservable.Subscribe(args => {
                ToggleSelection(IsMine(args.Element));
            }).AddTo(gameObject);
            Grid.MoveObservable.Subscribe(args => {
                if(IsSelected ) {
                    var adjacentPosition = Position.GetAdjacentPosition(args.Direction);
                    if(adjacentPosition.IsValid) {
                        float speed = Grid.SlideSpeed(args.Direction);

                        SetState(GridCellState.Locked);
                        adjacentPosition.Cell.SetState(GridCellState.Locked);

                        Element.MoveToCell(adjacentPosition.Cell, speed, () => {
                            Grid.CheckMove(this, adjacentPosition.Cell, () => {
                                Element.MoveToCell(this, speed, () => {
                                    SetState(GridCellState.Normal);
                                });
                                adjacentPosition.Cell.Element?.MoveToCell(adjacentPosition.Cell, speed, () => {
                                    adjacentPosition.Cell.SetState(GridCellState.Normal);
                                });
                            });
                        });                       
                        adjacentPosition.Cell.Element?.MoveToCell(this, speed, () => {});
                    }
                }
            });
        }

        public void ExchangeWith(GridCell other, Action onCompleted) {
            StartCoroutine(ExchangeWithImpl(other, onCompleted));
        }

        private IEnumerator ExchangeWithImpl(GridCell other, Action onCompleted) {
            bool isFirstCompleted = false;
            bool isSecondCompleted = false;

            var firstElement = Element;
            var otherElement = other.Element;
            AttachElement(otherElement, () => isSecondCompleted = true);
            other.AttachElement(firstElement, () => isFirstCompleted = true);
            yield return new WaitUntil(() => isFirstCompleted && isSecondCompleted);
            onCompleted?.Invoke();
        }

        private void SetPosition(CellPosition position) {
            Position = position;
            transform.localPosition = Grid.GetWorldGridCellPosition(Position);
        }

        public void DropElementDownOn(int shiftCount ) {
            if (!IsEmpty) {
                SetState(GridCellState.Locked);
                GridCell targetCell = Grid[Row + shiftCount, Column];
                var element = Element;
                targetCell.SetState(GridCellState.Locked);
                targetCell.AttachElement(element, () => {
                    targetCell.SetState(GridCellState.Normal);
                });                
                Empty();
            }
        }

        public void DropOnMe(GridElement element) {
            if(IsEmpty) {
                SetState(GridCellState.Locked);
                AttachElement(element, () => {
                    SetState(GridCellState.Normal);
                });
            }
        }

        public void SetState(GridCellState state) {
            State = state;
        }

        public int Row => Position.Row;
        public int Column => Position.Column;
        public bool IsEmpty => Element == null;
        public ElementType ElementType => Element.elementType;

        public bool IsSelected {
            get {
                if(!IsEmpty) {
                    return Element.IsSelected;
                }
                return false;
            }
        }

        public bool IsMine(GridElement e) {
            if(!IsEmpty) {
                if(e.Cell != null ) {
                    return (e.Cell == this);
                }
            }
            return false;
        }

        private void Empty() {
            Element = null;
            State = GridCellState.Normal;
        }

        public void DestroyElement(bool resetLock = true) {
            if(!IsEmpty) {
                Element.DetachFromCell();
                Element.DestroySelf();
                Element = null;           
            }
            if (resetLock) {
                State = GridCellState.Normal;
            }
        }

        public void ToggleSelection(bool isSelected) {
            if(isSelected) {
                selection.Activate();
                Element.Select();
            } else {
                selection.Deactivate();
                Element?.Unselect();
            }
        }

        public override string ToString() {
            if(IsEmpty) {
                return $"({Row}, {Column}), EMPTY, {State}|";
            } else {
                return $"({Row}, {Column}), {ElementType}, {State}|";
            }
        }

    }

    public enum GridCellState {
        Locked,
        Normal
    }

}