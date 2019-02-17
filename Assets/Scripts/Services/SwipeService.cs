using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

namespace Ozh.Services {

    public static class MoveVectors {
        public static readonly Vector2 Up = new Vector2(0, 1);
        public static readonly Vector2 Down = new Vector2(0, -1);
        public static readonly Vector2 Right = new Vector2(1, 0);
        public static readonly Vector2 Left = new Vector2(-1, 0);

    }

    public enum Swipe {
        None,
        Up,
        Down,
        Left,
        Right
    };

    public enum SwipePhase {
        Start,
        Continue,
        End
    }

    public class SwipeInfo {
        public SwipePhase Phase { get; set; }
        public Vector2 InputPosition { get; set; }
        public Swipe Direction { get; set; }
        public Vector2 Velocity { get; set; }

        public override string ToString() {
            return $"Phase: {Phase}, Direction: {Direction}, Position: {InputPosition}, Velocity: {Velocity}";
        }
    }

    public class SwipeService : MonoBehaviour, ISwipeService  {

        const float fourDirAngle = 0.5f;
        const float defaultDPI = 72f;
        const float dpcmFactor = 2.54f;

        public float minSwipeLength = 0.5f;

        Dictionary<Swipe, Vector2> directions = new Dictionary<Swipe, Vector2> {
            [Swipe.Up] = MoveVectors.Up,
            [Swipe.Down] = MoveVectors.Down,
            [Swipe.Left] = MoveVectors.Left,
            [Swipe.Right] = MoveVectors.Right
        };

        public Subject<SwipeInfo> SwipeObservable { get; } = new Subject<SwipeInfo>();

        private Vector2 swipeVelocity;
        private float dpcm;
        private float swipeStartTime;
        private float swipeEndTime;
        private bool autoDetectSwipes;
        private bool swipeEnded;
        private Swipe swipeDirection;
        private Vector2 firstPressPos;
        private Vector2 secondPressPos;


        public void Setup(IServiceCollection services ) { }

        private void Awake() {
            float dpi = (Screen.dpi == 0) ? defaultDPI : Screen.dpi;
            dpcm = dpi / dpcmFactor;
        }

        private void Update() {
            DetectSwipe();
        }

        void DetectSwipe() {
            if(GetTouchInput() || GetMouseInput() ) {
                if(swipeEnded) { return; }

                Vector2 currentSwipe = secondPressPos - firstPressPos;
                float swipeCm = currentSwipe.magnitude / dpcm;

                if(swipeCm < minSwipeLength ) {
                    return;
                }

                swipeEndTime = Time.time;
                swipeVelocity = currentSwipe * (swipeEndTime - swipeStartTime);
                swipeDirection = GetSwipeDirByTouch(currentSwipe);
                swipeEnded = true;
                SwipeObservable.OnNext(new SwipeInfo {
                    Direction = swipeDirection,
                    InputPosition = secondPressPos,
                    Phase = SwipePhase.End,
                    Velocity = swipeVelocity
                });
            } else {
                swipeDirection = Swipe.None;
            }
        }

        private bool GetTouchInput() {
            if(Input.touches.Length > 0 ) {
                Touch t = Input.GetTouch(0);

                if(t.phase == TouchPhase.Began ) {
                    firstPressPos = t.position;
                    swipeStartTime = Time.time;
                    swipeEnded = false;
                    SwipeObservable.OnNext(new SwipeInfo {
                        Direction = Swipe.None,
                        InputPosition = t.position,
                        Phase = SwipePhase.Start,
                        Velocity = Vector2.zero });
                } else if(t.phase == TouchPhase.Ended ) {
                    secondPressPos = t.position;
                    return true;
                } else {
                    if (!swipeEnded) {
                        secondPressPos = t.position;
                        Vector2 currentSwipe = secondPressPos - firstPressPos;
                        float swipeCm = currentSwipe.magnitude / dpcm;

                        if (swipeCm >= minSwipeLength) {
                            SwipeObservable.OnNext(new SwipeInfo {
                                Direction = GetSwipeDirByTouch(t.position - firstPressPos),
                                InputPosition = t.position,
                                Phase = SwipePhase.Continue,
                                Velocity = (Time.time - swipeStartTime) * (t.position - firstPressPos)
                            });
                        }
                    }
                    return true;
                }
            }
            return false;
        }

        private bool GetMouseInput() {
            if(Input.GetMouseButtonDown(0)) {
                firstPressPos = (Vector2)Input.mousePosition;
                swipeStartTime = Time.time;
                swipeEnded = false;
                SwipeObservable.OnNext(new SwipeInfo {
                    Direction = Swipe.None,
                    InputPosition = Input.mousePosition,
                    Phase = SwipePhase.Start,
                    Velocity = Vector2.zero });
            } else if(Input.GetMouseButtonUp(0)) {
                secondPressPos = (Vector2)Input.mousePosition;
                return true;
            } else {
                if (!swipeEnded) {
                    secondPressPos = Input.mousePosition;
                    Vector2 currentSwipe = secondPressPos - firstPressPos;
                    float swipeCm = currentSwipe.magnitude / dpcm;
                    if (swipeCm >= minSwipeLength) {
                        SwipeObservable.OnNext(new SwipeInfo {
                            Direction = GetSwipeDirByTouch((Vector2)Input.mousePosition - firstPressPos),
                            InputPosition = Input.mousePosition,
                            Phase = SwipePhase.Continue,
                            Velocity = (Time.time - swipeStartTime) * ((Vector2)Input.mousePosition - firstPressPos)
                        });
                    }
                }
                return true;
            }
            return false;
        }

        private bool IsDirection(Vector2 direction, Vector2 cardinalDirection ) {
            return Vector2.Dot(direction, cardinalDirection) > fourDirAngle;
        }

        private Swipe GetSwipeDirByTouch(Vector2 currentSwipe ) {
            currentSwipe.Normalize();
            var swipeDir = directions.FirstOrDefault(dir => IsDirection(currentSwipe, dir.Value));
            return swipeDir.Key;
        }

        private bool IsSwipingDirection(Swipe swipeDir) {
            DetectSwipe();
            return swipeDirection == swipeDir;
        }
    }

    public interface ISwipeService : IGameService {
        Subject<SwipeInfo> SwipeObservable { get; }
    }
}