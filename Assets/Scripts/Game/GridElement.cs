namespace Ozh.Game {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class GridElement : MonoBehaviour {

        public ElementType elementType;
        public GameObject particles;


        public bool IsSelected { get; private set; } = false;
        public GridCell Cell { get; private set; } = null;
        public Grid Grid { get; private set; } = null;
        
        public void Construct(Grid grid, CellPosition position ) {
            Grid = grid;
            transform.localPosition = grid.GetWorldGridCellPosition(position);
        }

        public void AttachToCell(GridCell cell, Action onMoveCompleted = null) {
            Cell = cell;
            MoveToCell(cell, Grid.ElementVerticalSpeed, onMoveCompleted);
        }

        public void DetachFromCell() {
            Cell = null;
        }

        public void MoveToCell(GridCell targetCell, float speed, Action onMoveCompleted = null) {

            Vector3 start = transform.localPosition;
            Vector3 end = targetCell.transform.localPosition;

            transform.MoveTo(end, MoveVerticalInterval(start, end, speed), EaseType.EaseInSin, () => {
                onMoveCompleted?.Invoke();
            });
            
        }

        public void DestroySelf() {
            particles.Activate();
            GetComponent<Animator>().SetTrigger("scale");
            Destroy(gameObject, 0.3f);
            switch(elementType) {
                case ElementType.Bagel:
                    Grid.IncrementScore(1);
                    break;
                case ElementType.Cake:
                    Grid.IncrementScore(2);
                    break;
                case ElementType.IceCream:
                    Grid.IncrementScore(3);
                    break;
                case ElementType.Pie:
                    Grid.IncrementScore(4);
                    break;
            }
        }


        private float MoveVerticalInterval(Vector3 start, Vector3 end, float speed) {
            start.z = 0;
            end.z = 0;
            float distance = Vector3.Distance(start, end);
            return distance / speed;
        }

        public void Select() {
            if(!IsSelected) {
                IsSelected = true;
            }
        }

        public void Unselect() {
            if(IsSelected) {
                IsSelected = false;
            }
        }

    }

}