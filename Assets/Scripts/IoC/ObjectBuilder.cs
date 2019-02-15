using System;
using UnityEngine;

namespace Ozh.Tools.IoC {
    public class ObjectBuilder : IObjectBuilder 
    {

        public Type TypeToResolve { get; private set; }
        public Type TypeConcrete { get; private set; }
        public ObjectLifecycle Lifecycle { get; private set; }

        public object Instance { get; private set; }

        public bool IsLazy { get; private set; }

        private Func<object> Fabric { get; set; }

        public string Id { get; private set; }

        public bool IsMonoBehaviour
            => TypeConcrete.IsSubclassOf(typeof(MonoBehaviour));

        public bool FromFabric => Fabric != null;

        public ObjectBuilder(Type typeToResolve, Type typeConcrete, ObjectLifecycle lifecycle) {
            TypeToResolve = typeToResolve;
            TypeConcrete = typeConcrete;
            Lifecycle = lifecycle;
        }

        public IObjectBuilder AsLazy() {
            IsLazy = true;
            return this;
        }

        public IObjectBuilder AsNonLazy() {
            IsLazy = false;
            return this;
        }

        public IObjectBuilder WithFabric<TConcrete>(Func<TConcrete> fabric) {
            Fabric = () => {
                return fabric();
            };
            return this;
        }

        public IObjectBuilder WithId(string id) {
            Id = id;
            return this;
        }


        public void ConstructTransientInstance(params object[] args ) {
            ForceToNullInvalidMonoBehaviour();
            if(IsMonoBehaviour ) {
                if(Fabric != null ) {
                    Instance = Fabric();

                } else {
                    throw new Exception($"MonoBehaviours allow create only with fabric method");
                }
            } else {
                if (Fabric != null) {
                    Instance = Fabric();
                } else {
                    Instance = Activator.CreateInstance(TypeConcrete, args);
                }
            }
        }


        public bool ConstructSingletonInstance(params object[] args ) {
            ForceToNullInvalidMonoBehaviour();
            if(Instance == null ) {
                if(IsMonoBehaviour ) {
                    if (Fabric != null) {
                        Instance = Fabric();
                        GameObject.DontDestroyOnLoad((Instance as MonoBehaviour).gameObject);
                    } else {
                        throw new Exception($"MonoBehaviours allow create only with fabric method");
                    }
                } else {
                    if(Fabric != null ) {
                        Instance = Fabric();
                    } else {
                        Instance = Activator.CreateInstance(TypeConcrete, args);
                    }
                }
                return true;
            }
            return false;
        }

        private void ForceToNullInvalidMonoBehaviour() {
            if (IsMonoBehaviour) {
                if (Instance != null) {
                    MonoBehaviour behaviour = Instance as MonoBehaviour;
                    if (!behaviour || !behaviour.gameObject) {
                        Instance = null;
                    }
                }
            }
        }

    }
}
