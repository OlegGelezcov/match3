namespace Ozh.Services {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class CoroutineService : MonoBehaviour, ICoroutineService {

        public void Setup(IServiceCollection services) {

        }

        public void ExecuteCoroutine(IEnumerator coroutine) {
            StartCoroutine(coroutine);
        }
    }

    public interface ICoroutineService : IGameService {
        void ExecuteCoroutine(IEnumerator coroutine);
    }
}