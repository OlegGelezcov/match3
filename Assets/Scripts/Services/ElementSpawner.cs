namespace Ozh.Services {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class ElementSpawner : IElementSpawner {

        private IPrefabRepository<string> PrefabRepository { get; set; }
        private List<GameObject> ElementPrefabs { get; set; }

        public void Setup(IServiceCollection services) {
            PrefabRepository = services.Resolve<IResourceService>().GetRepository<IPrefabRepository<string>>();
            ElementPrefabs = new List<GameObject> {
                PrefabRepository.GetPrefab("bagel"),
                PrefabRepository.GetPrefab("cake"),
                PrefabRepository.GetPrefab("icecream"),
                PrefabRepository.GetPrefab("pie")
            };
        }


        public GameObject Spawn()
            => ElementPrefabs[Random.Range(0, ElementPrefabs.Count)].MakeInstance();


    }

    public interface IElementSpawner : IGameService {
        GameObject Spawn();
    }
}