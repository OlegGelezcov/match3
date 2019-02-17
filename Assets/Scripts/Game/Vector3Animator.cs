namespace Ozh.Game {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public class Vector3Animator : MonoBehaviour {
        private float timer;
        public List<Vector3AnimationInfo> Datas { get; private set; } = new List<Vector3AnimationInfo>();
        private readonly List<Func<float, float, float, float>> easeFuncs = new List<Func<float, float, float, float>>();
        private readonly List<List<AnimationEventInfo<Vector3>>> events = new List<List<AnimationEventInfo<Vector3>>>();
        private readonly List<bool> isStartedStatus = new List<bool>();
        private bool isWaitingForStart = false;

        public bool IsStarted => isStartedStatus.Any(s => s);

        public void Stop() {
            foreach (var data in Datas) {
                data.Data.OnEnd?.Invoke(data.Data.End, data.Data.Target);
            }
            Datas.Clear();
            for (int i = 0; i < isStartedStatus.Count; i++) {
                isStartedStatus[i] = false;
            }
            isStartedStatus.Clear();
        }

        public void StartAnimation(Vector3AnimationData data) {
            StartAnimation(new List<Vector3AnimationData> { data });
        }

        public void StartAnimation(List<Vector3AnimationData> datas ) {
            StartCoroutine(StartAnimationImpl(datas));
        }

        private IEnumerator StartAnimationImpl(List<Vector3AnimationData> datas) {
            if(isWaitingForStart) {
                yield break;
            }
            if(IsStarted) {
                isWaitingForStart = true;
            }
            yield return new WaitUntil(() => !IsStarted);
            isWaitingForStart = false;
            if(!IsStarted ) {
                Datas.Clear();
                datas.ForEach(d => {
                    Datas.Add(new Vector3AnimationInfo {
                        Data = d,
                        Direction = AnimationDirection.Forward
                    });
                });
                timer = 0f;
                easeFuncs.Clear();
                events.Clear();
                Datas.ForEach(d => {
                    easeFuncs.Add(AnimationUtils.GetEaseFunc(d.Data.EaseType));
                    var dataEvents = new List<AnimationEventInfo<Vector3>>();
                    if(d.Data.Events != null ) {
                        d.Data.Events.ForEach(e => {
                            dataEvents.Add(new AnimationEventInfo<Vector3> {
                                Event = e,
                                IsCompleted = false
                            });
                        });
                    }
                    events.Add(dataEvents);
                });
                isStartedStatus.Clear();
                for(int i = 0; i < Datas.Count; i++) {
                    isStartedStatus.Add(true);
                }
                Datas.ForEach(d => {
                    d.Data.OnStart?.Invoke(d.Data.Start, d.Data.Target);
                });
            }
        }

        private void Update() {
            if(!IsStarted) {
                return;
            }
            timer += Time.deltaTime;
            for(int i = 0; i < Datas.Count; i++ ) {
                UpdateAnimationInfo(Datas[i], i);
            }
        }

        private void UpdateAnimationInfo(Vector3AnimationInfo data, int index) {
            float nTimer = Mathf.Clamp01(timer / data.Data.Duration);
            switch(data.Data.AnimationMode) {
                case AnimationMode.Single: {
                        UpdateSingleAnimation(data, nTimer, index);
                    }
                    break;
                case AnimationMode.Loop: {
                        UpdateLoopAnimation(data, nTimer, index);
                    }
                    break;
                case AnimationMode.PingPong: {
                        UpdatePingPongAnimation(data, nTimer, index);
                    }
                    break;
            }
        }

        private void UpdatePingPongAnimation(Vector3AnimationInfo data, float nTimer, int index ) {
            if (nTimer < 1) {
                if (data.Direction == AnimationDirection.Forward) {
                    float x = easeFuncs[index](data.Data.Start.x, data.Data.End.x, nTimer);
                    float y = easeFuncs[index](data.Data.Start.y, data.Data.End.y, nTimer);
                    float z = easeFuncs[index](data.Data.Start.z, data.Data.End.z, nTimer);
                    Vector3 value = new Vector3(x, y, z);
                    data.Data.OnUpdate?.Invoke(value,  data.Data.Target, nTimer);
                    TryCompleteEvents(index, value, nTimer, data.Data);
                } else {
                    float x = easeFuncs[index](data.Data.End.x, data.Data.Start.x, nTimer);
                    float y = easeFuncs[index](data.Data.End.y, data.Data.Start.y, nTimer);
                    float z = easeFuncs[index](data.Data.End.z, data.Data.Start.z, nTimer);
                    Vector3 value = new Vector3(x, y, z);
                    data.Data.OnUpdate?.Invoke(value,  data.Data.Target, nTimer);
                    TryCompleteEvents(index, value, nTimer, data.Data);
                }
            } else {
                TryCompleteEvents(index, data.Data.End, nTimer, data.Data);
                timer = 0f;
                if (data.Direction == AnimationDirection.Forward) {
                    data.Data.OnEnd?.Invoke(data.Data.End, data.Data.Target);
                    data.Direction = AnimationDirection.Backward;
                } else {
                    data.Data.OnStart?.Invoke(data.Data.Start, data.Data.Target);
                    data.Direction = AnimationDirection.Forward;
                }
            }
            HandleWaitingForStart();
        }

        private void UpdateLoopAnimation(Vector3AnimationInfo data, float nTimer, int index ) {
            if(nTimer < 1) {
                float x = easeFuncs[index](data.Data.Start.x, data.Data.End.x, nTimer);
                float y = easeFuncs[index](data.Data.Start.y, data.Data.End.y, nTimer);
                float z = easeFuncs[index](data.Data.Start.z, data.Data.End.z, nTimer);
                Vector3 value = new Vector3(x, y, z);
                data.Data.OnUpdate?.Invoke(value, data.Data.Target, nTimer);
                TryCompleteEvents(index, value, nTimer, data.Data);
            } else {
                TryCompleteEvents(index, data.Data.End, nTimer, data.Data);
                data.Data.OnEnd?.Invoke(data.Data.End, data.Data.Target);
                timer = 0f;
                data.Data.OnStart?.Invoke(data.Data.Start, data.Data.Target);
            }
            HandleWaitingForStart();
        }

        private void UpdateSingleAnimation(Vector3AnimationInfo data, float nTimer, int index) {
            if(nTimer < 1 ) {
                float x = easeFuncs[index](data.Data.Start.x, data.Data.End.x, nTimer);
                float y = easeFuncs[index](data.Data.Start.y, data.Data.End.y, nTimer);
                float z = easeFuncs[index](data.Data.Start.z, data.Data.End.z, nTimer);
                Vector3 value = new Vector3(x, y, z);
                data.Data.OnUpdate?.Invoke(value, data.Data.Target, nTimer);
                TryCompleteEvents(index, value, nTimer, data.Data);
            } else {
                TryCompleteEvents(index, data.Data.End, nTimer, data.Data);
                data.Data.OnEnd?.Invoke(data.Data.End, data.Data.Target);
                isStartedStatus[index] = false;
            }
        }

        private void TryCompleteEvents(int index, Vector3 value, float nTimer, Vector3AnimationData data ) {
            var dataEvents = events[index];
            dataEvents.ForEach(e => {
                if((e.Event.Mode == AnimationEventMode.Single && !e.IsCompleted) ||
                  (e.Event.Mode == AnimationEventMode.Multiple)) {
                    if(e.Event.IsValid?.Invoke(value, nTimer, data.Target) ?? false) {
                        e.Event.OnEvent?.Invoke(value, nTimer, data.Target);
                        e.IsCompleted = true;
                    }
                }
            });
        }

        private void MakeNotStarted() {
            for (int i = 0; i < isStartedStatus.Count; i++) {
                isStartedStatus[i] = false;
            }
        }

        private void HandleWaitingForStart() {
            if (isWaitingForStart) {
                MakeNotStarted();
                isWaitingForStart = false;
            }
        }
    }

    public class Vector3AnimationInfo {
        public Vector3AnimationData Data { get; set; }
        public AnimationDirection Direction { get; set; }
    }

    public class Vector3AnimationData {
        public Vector3 Start { get; set; }
        public Vector3 End { get; set; }
        public float Duration { get; set; }
        public GameObject Target { get; set; }
        public EaseType EaseType { get; set; }
        public Action<Vector3, GameObject> OnStart { get; set; }
        public Action<Vector3, GameObject, float> OnUpdate { get; set; }
        public Action<Vector3, GameObject> OnEnd { get; set; }
        public List<AnimationEvent<Vector3>> Events { get; set; }
        public AnimationMode AnimationMode { get; set; } = AnimationMode.Single;
    }
}