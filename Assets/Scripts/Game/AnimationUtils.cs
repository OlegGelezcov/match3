namespace Ozh.Game {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public static class AnimationUtils {

        public static System.Func<float, float, float, float> GetEaseFunc(EaseType type) {
            return easeFunctionMap.ContainsKey(type) ? easeFunctionMap[type] :
                (start, end, timer) => start;
        }

        private static readonly Dictionary<EaseType, System.Func<float, float, float, float>> easeFunctionMap =
        new Dictionary<EaseType, System.Func<float, float, float, float>> {
            [EaseType.Linear] = Linear,
            [EaseType.EaseInQuad] = EaseInQuad,
            [EaseType.EaseOutQuad] = EaseOutQuad,
            [EaseType.EaseInOutQuad] = EaseInOutQuad,
            [EaseType.EaseInCubic] = EaseInCubic,
            [EaseType.EaseOutCubic] = EaseOutCubic,
            [EaseType.EaseInOutCubic] = EaseInOutCubic,
            [EaseType.EaseInQuartic] = EaseInQuartic,
            [EaseType.EaseOutQuartic] = EaseOutQuartic,
            [EaseType.EaseInOutQuartic] = EaseInOutQuartic,
            [EaseType.EaseInQuintic] = EaseInQuintic,
            [EaseType.EaseOutQuintic] = EaseOutQuintic,
            [EaseType.EaseInOutQuintic] = EaseInOutQuintic,
            [EaseType.EaseInSin] = EaseInSin,
            [EaseType.EaseOutSin] = EaseOutSin,
            [EaseType.EaseInOutSin] = EaseInOutSin
        };

        public static float Linear(float startValue, float endValue, float timer01) {
            float change = endValue - startValue;
            return change * timer01 + startValue;
        }

        public static float EaseInQuad(float startValue, float endValue, float timer01) {
            float change = endValue - startValue;
            return change * timer01 * timer01 + startValue;
        }

        public static float EaseOutQuad(float startValue, float endValue, float timer01) {
            float change = endValue - startValue;
            return -change * timer01 * (timer01 - 2.0f) + startValue;
        }

        public static float EaseInOutQuad(float startValue, float endValue, float timer01) {
            float change = endValue - startValue;
            timer01 *= 2.0f;
            if (timer01 < 1f) {
                return change * 0.5f * timer01 * timer01 + startValue;
            }
            timer01--;
            return -change * 0.5f * (timer01 * (timer01 - 2f) - 1) + startValue;
        }

        public static float EaseInCubic(float startValue, float endValue, float timer01) {
            float change = endValue - startValue;
            return change * timer01 * timer01 * timer01 + startValue;
        }

        public static float EaseOutCubic(float startValue, float endValue, float timer01) {
            float change = endValue - startValue;
            timer01--;
            return change * (timer01 * timer01 * timer01 + 1f) + startValue;
        }

        public static float EaseInOutCubic(float startValue, float endValue, float timer01) {
            float change = endValue - startValue;
            timer01 *= 2f;
            if (timer01 < 1f) {
                return change * 0.5f * timer01 * timer01 * timer01 + startValue;
            }
            timer01 -= 2f;
            return change * 0.5f * (timer01 * timer01 * timer01 + 2f) + startValue;
        }

        public static float EaseInQuartic(float startValue, float endValue, float timer01) {
            float change = endValue - startValue;
            return change * timer01 * timer01 * timer01 * timer01 + startValue;
        }

        public static float EaseOutQuartic(float startValue, float endValue, float timer01) {
            float change = endValue - startValue;
            timer01--;
            return -change * (timer01 * timer01 * timer01 * timer01 - 1.0f) + startValue;
        }

        public static float EaseInOutQuartic(float startValue, float endValue, float timer01) {
            float change = endValue - startValue;
            timer01 *= 2f;
            if (timer01 < 1f) {
                return change * 0.5f * timer01 * timer01 * timer01 * timer01 + startValue;
            }
            timer01 -= 2f;
            return -change * 0.5f * (timer01 * timer01 * timer01 * timer01 - 2f) + startValue;
        }

        public static float EaseInQuintic(float startValue, float endValue, float timer01) {
            float change = endValue - startValue;
            return change * timer01 * timer01 * timer01 * timer01 * timer01 + startValue;
        }

        public static float EaseOutQuintic(float startValue, float endValue, float timer01) {
            float change = endValue - startValue;
            timer01--;
            return change * (timer01 * timer01 * timer01 * timer01 * timer01 + 1f) + startValue;
        }

        public static float EaseInOutQuintic(float startValue, float endValue, float timer01) {
            float change = endValue - startValue;
            timer01 *= 2f;
            if (timer01 < 1f) {
                return change * 0.5f * timer01 * timer01 * timer01 * timer01 * timer01 + startValue;
            }
            timer01 -= 2f;
            return change * 0.5f * (timer01 * timer01 * timer01 * timer01 * timer01 + 2f) + startValue;
        }

        public static float EaseInSin(float startValue, float endValue, float timer01) {
            float change = endValue - startValue;
            return -change * Mathf.Cos(timer01 * Mathf.PI * 0.5f) + change + startValue;
        }

        public static float EaseOutSin(float startValue, float endValue, float timer01) {
            float change = endValue - startValue;
            return change * Mathf.Sin(timer01 * Mathf.PI * 0.5f) + startValue;
        }

        public static float EaseInOutSin(float startValue, float endValue, float timer01) {
            float change = endValue - startValue;
            return -change * 0.5f * (Mathf.Cos(timer01 * Mathf.PI) - 1f) + startValue;
        }
    }

    public enum EaseType {
        Linear,
        EaseInQuad,
        EaseOutQuad,
        EaseInOutQuad,
        EaseInCubic,
        EaseOutCubic,
        EaseInOutCubic,
        EaseInQuartic,
        EaseOutQuartic,
        EaseInOutQuartic,
        EaseInQuintic,
        EaseOutQuintic,
        EaseInOutQuintic,
        EaseInSin,
        EaseOutSin,
        EaseInOutSin
    }

    public enum AnimationMode {
        Single,
        PingPong,
        Loop
    }

    public enum AnimationDirection {
        Forward,
        Backward
    }

    public enum AnimationEventMode {
        Single,
        Multiple
    }

    public class AnimationEventInfo<T> {
        public bool IsCompleted { get; set; }
        public AnimationEvent<T> Event { get; set; }
    }

    public class AnimationEvent<T> {
        public Func<T, float, GameObject, bool> IsValid { get; set; }
        public Action<T, float, GameObject> OnEvent { get; set; }
        public AnimationEventMode Mode { get; set; }
    }

    public static class AnimationExtensions {

        public static Action<Vector3, GameObject> StartEndPositionUpdater(this Transform trs, Action action = null)
            => (pos, go) => {
                trs.localPosition = pos;
                action?.Invoke();
            };

        public static Action<Vector3, GameObject, float> MidPositionUpdater(this Transform trs)
            => (pos, go, timer) => trs.localPosition = pos;


        public static void MoveTo(this Transform trs, Vector3 end, float duration, EaseType easeType, Action onMoveCompleted = null) {
            Vector3AnimationData animData = new Vector3AnimationData {
                AnimationMode = AnimationMode.Single,
                Duration = duration,
                Start = trs.localPosition,
                End = end,
                EaseType = easeType,
                Target = trs.gameObject,
                OnStart = trs.StartEndPositionUpdater(),
                OnUpdate = trs.MidPositionUpdater(),
                OnEnd = trs.StartEndPositionUpdater(onMoveCompleted)
            };
            trs.gameObject.GetOrAddComponent<Vector3Animator>().StartAnimation(animData);
        }
    }
}