﻿namespace Ozh {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public static  class Extensions {

        public static T GetOrAddComponent<T>(this GameObject go) where T : Component {
            T component = go.GetComponent<T>();
            if(!component) {
                component = go.AddComponent<T>();
            }
            return component;
        }


        public static void CopyFrom<K, V>(this Dictionary<K, V> target, Dictionary<K, V> source ) {
            target.Clear();
            foreach(var kvp in source) {
                target.Add(kvp.Key, kvp.Value);
            }
        }

        public static GameObject MakeUIInstance(this GameObject prefab, Transform canvasParent) {
            GameObject instance = GameObject.Instantiate<GameObject>(prefab);
            instance.GetComponent<RectTransform>().SetParent(canvasParent, false);
            return instance;
        }

        public static GameObject MakeInstance(this GameObject prefab, Transform parent = null ) {
            GameObject instance = GameObject.Instantiate<GameObject>(prefab, new Vector3(40000, 40000, 0), Quaternion.identity);
            if(parent != null ) {
                instance.transform.SetParent(parent, false);
            }
            return instance;
        }

        public static GameObject MakeInstance(this GameObject prefab,  Transform parent, Vector3 position, Quaternion rotation) {
            GameObject instance = GameObject.Instantiate<GameObject>(prefab, parent, false);
            instance.transform.localPosition = position;
            instance.transform.localRotation = rotation;
            return instance;
        }

        public static void Activate(this GameObject go) {
            if(!go.activeSelf) {
                go.SetActive(true);
            }
        }

        public static void Deactivate(this GameObject go) {
            if(go.activeSelf) {
                go.SetActive(false);
            }
        }

        public static void SetListener(this Button button, UnityEngine.Events.UnityAction action) {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);
        }
    }

}