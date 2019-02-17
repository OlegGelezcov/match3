
namespace Ozh.Services {
    using Ozh.Game;
    using UnityEngine;
    using UniRx;
    using System.Collections.Generic;
    using System;

    public class ServiceCollection : MonoBehaviour, IServiceCollection {

        private static bool isCreated = false;

        private Dictionary<Type, object> Services { get; } = new Dictionary<Type, object>();

        private void Awake() {
            if(!isCreated) {
                RegisterServices();
                InitializeServices();
                isCreated = true;
            } else {
                Destroy(gameObject);
                return;
            }
        }


        private void RegisterServices() {

            Register<IResourceService, ResourceService>(new ResourceService());
            Register<ISwipeService, SwipeService>(GetComponent<SwipeService>());
            Register<ICoroutineService, CoroutineService>(GetComponent<CoroutineService>());
            Register<IGameStateService, GameStateService>(GetComponent<GameStateService>());
            Register<IViewService, ViewService>(GetComponent<ViewService>());
            Register<IElementSpawner, ElementSpawner>(new ElementSpawner());
            Register<IGridService, Ozh.Game.Grid>(new Ozh.Game.Grid());
        }

        private void InitializeServices() {
            Resolve<IResourceService>().Setup(this);
            Resolve<ISwipeService>().Setup(this);
            Resolve<ICoroutineService>().Setup(this);
            Resolve<IGameStateService>().Setup(this);
            Resolve<IViewService>().Setup(this);
            Resolve<IElementSpawner>().Setup(this);
            Resolve<IGridService>().Setup(this);
        }

        public T Register<T, U>(U instance)
            where T : IGameService
            where U : class, T {
            if(Services.ContainsKey(typeof(T))) {
                Services[typeof(T)] = instance;
            } else {
                Services.Add(typeof(T), instance);
            }
            return instance;
        }

        public T Resolve<T>() where T : IGameService {
            if(Services.ContainsKey(typeof(T))) {
                return (T)(object)Services[typeof(T)];
            }
            throw new Exception($"Unable resolve service {typeof(T).Name}");
        }
    }

}
