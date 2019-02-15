namespace Ozh.Services {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class ObjectCache<K, V> where V : UnityEngine.Object {

        private Dictionary<K, string> objectPathMap = new Dictionary<K, string>();
        private Dictionary<K, V> cache = new Dictionary<K, V>();

        public void Setup(Dictionary<K, string> initialPathDict) {
            objectPathMap.CopyFrom(initialPathDict);
        }

        public V GetObject(K key ) {
            if(!cache.ContainsKey(key)) {
                LoadFromResources(key);
            }
            return cache[key];
        }

        private void LoadFromResources(K key) {
            if(objectPathMap.ContainsKey(key)) {
                V obj = Resources.Load<V>(objectPathMap[key]);
                cache[key] = obj;
            } else {
                throw new KeyNotFoundException($"{key}");
            }
        }
    }
}
