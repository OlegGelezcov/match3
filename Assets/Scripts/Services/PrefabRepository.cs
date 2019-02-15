namespace Ozh.Services {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class PrefabCache<K> : ObjectCache<K, GameObject>, IPrefabRepository<K> {

        public bool IsLoaded { get; private set; } = false;

        public void Load(object obj = null) {
            if(!IsLoaded ) {
                base.Setup(obj as Dictionary<K, string>);
                IsLoaded = true;
            }
        }

        public GameObject GetPrefab(K key) => GetObject(key);
    }

    public interface IPrefabRepository<K> : IRepository {
        GameObject GetPrefab(K key);
    }
}