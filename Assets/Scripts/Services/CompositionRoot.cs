namespace Ozh.Services {
    using Ozh.Game;
    using Ozh.Tools.IoC;
    using UnityEngine;

    public class CompositionRoot : MonoBehaviour, IServiceCollection {

        private static bool isCreated = false;
        private IContainer Container { get; } = new IoCContainer();

        private void Awake() {
            if(!isCreated) {
                RegisterServices();
                isCreated = true;
            } else {
                Destroy(gameObject);
                return;
            }
        }

        private void Update() {
            if(Input.GetKeyUp(KeyCode.A)) {
                StartRound();
            }
        }

        private void RegisterServices() {
            Container.AddSingleton<IResourceService, ResourceService>().AsNonLazy();

            Container.AddSingleton<IPrefabRepository<string>, PrefabCache<string>>().AsNonLazy().WithFabric(() => {
                return Container.Resolve<IResourceService>().GetRepository<IPrefabRepository<string>>();
            });

            Container.AddSingleton<IGridLevelRepository, GridLevelRepository>().AsNonLazy().WithFabric(() => {
                return Container.Resolve<IResourceService>().GetRepository<IGridLevelRepository>();
            });

            Container.AddSingleton<Ozh.Game.Grid>().AsLazy();

            Container.Build();
        }

        public T Resolve<T>()
            => Container.Resolve<T>();

        private void StartRound() {
            Ozh.Game.Grid grid = Container.Resolve<Ozh.Game.Grid>();
            grid.MakeGrid(gridLevel: 1);
        }

    }

}