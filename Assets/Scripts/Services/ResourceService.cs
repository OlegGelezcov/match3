namespace Ozh.Services {
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public class ResourceService : IResourceService {

        public IPrefabRepository<string> PrefabRepository { get; } = new PrefabCache<string>();
        public IGridLevelRepository GridLevelRepository { get; } = new GridLevelRepository();

        private Dictionary<Type, IRepository> Repositories { get; } = new Dictionary<Type, IRepository>();

        public void Setup(IServiceCollection services) {

            Repositories.Add(typeof(IPrefabRepository<string>), PrefabRepository);
            Repositories.Add(typeof(IGridLevelRepository), GridLevelRepository);

            PrefabRepository.Load(new Dictionary<string, string> {
                ["gridcell"] = "Prefabs/GridCell",
                ["bagel"] = "Prefabs/Bagel",
                ["cake"] = "Prefabs/Cake",
                ["icecream"] = "Prefabs/IceCream",
                ["pie"] = "Prefabs/Pie",
                ["startgameview"] = "Prefabs/UI/StartGameView",
                ["scoreview"] = "Prefabs/UI/ScoreView",
                ["endgameview"] = "Prefabs/UI/EndGameView"
            });
            GridLevelRepository.Load("Data/grid_levels");

            Debug.Log($"Setup resource service, prefabs loaded: {PrefabRepository.IsLoaded}");
        }


        public GameObject GetPrefab(string key) => PrefabRepository.GetPrefab(key);




        public T GetRepository<T>() where T : class, IRepository {
            Type targetType = typeof(T);
            if(Repositories.ContainsKey(targetType)) {
                return Repositories[targetType] as T;
            }
            throw new ArgumentException($"{nameof(targetType)}");
        }
    }

}