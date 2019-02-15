using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Ozh.Tools.IoC {
    public class IoCContainer : IContainer
    {
        private readonly Dictionary<Type, List<ObjectBuilder>> registeredObjects = new Dictionary<Type, List<ObjectBuilder>>();



        public IObjectBuilder AddSingleton<ITypeToResolve, TConcrete>() {
            return AddBuilder(typeof(ITypeToResolve), typeof(TConcrete), ObjectLifecycle.Singleton);
        }

        public IObjectBuilder AddSingleton<TConcrete>() {
            return AddSingleton<TConcrete, TConcrete>();
        }

        public IObjectBuilder AddTransient<ITypeToResolve, TConcrete>() {
            return AddBuilder(typeof(ITypeToResolve), typeof(TConcrete), ObjectLifecycle.Transient);
        }

        public IObjectBuilder AddTransient<TConcrete>() {
            return AddTransient<TConcrete, TConcrete>();
        }

        public void Build() {
            foreach(var typedBuilders in registeredObjects ) {
                foreach(var builder in typedBuilders.Value ) {
                    if(!builder.IsLazy) {
                        if(string.IsNullOrEmpty(builder.Id ) ) {
                            ResolveAnyObject(typedBuilders.Key);
                        } else {
                            ResolveObjectWithId(typedBuilders.Key, builder.Id);
                        }
                    }
                }
            }
        }

        public ITypeToResolve Resolve<ITypeToResolve>() {
            return (ITypeToResolve)ResolveAnyObject(typeof(ITypeToResolve));
        }

        public ITypeToResolve Resolve<ITypeToResolve>(string id) {
            return (ITypeToResolve)ResolveObjectWithId(typeof(ITypeToResolve), id);
        }

        private IObjectBuilder AddBuilder(Type typeToResolve, Type tConcrete, ObjectLifecycle lifecycle) {
            ObjectBuilder builder = new ObjectBuilder(typeToResolve, tConcrete, lifecycle);
            if (registeredObjects.ContainsKey(builder.TypeToResolve)) {
                registeredObjects[builder.TypeToResolve].Add(builder);
            } else {
                registeredObjects.Add(builder.TypeToResolve, new List<ObjectBuilder> { builder });
            }
            return builder;
        }

        private object ResolveObject(Type typeToResolve, string id = "") {
            if(string.IsNullOrEmpty(id)) {
                return ResolveAnyObject(typeToResolve);
            } else {
                return ResolveObjectWithId(typeToResolve, id);
            }
        }

        private object ResolveAnyObject(Type typeToResolve ) {
            if(!registeredObjects.ContainsKey(typeToResolve)) {
                throw new Exception($"Not registered type: {typeToResolve.FullName}");
            }
            var builderList = registeredObjects[typeToResolve];
            if(builderList.Count == 0 ) {
                throw new Exception($"No builders for registered type: {typeToResolve.FullName}");
            }
            var builder = builderList[0];
            return ConstructInstanceForBuilder(builder);
        }

        private object ResolveObjectWithId(Type typeToResolve, string id) {
            if (!registeredObjects.ContainsKey(typeToResolve)) {
                throw new Exception($"Not registered type: {typeToResolve.FullName}");
            }
            var builderList = registeredObjects[typeToResolve];
            if (builderList.Count == 0) {
                throw new Exception($"No builders for registered type: {typeToResolve.FullName}");
            }

            ObjectBuilder builder = null;
            foreach(var b in builderList.Where(b => !string.IsNullOrEmpty(b.Id)) ) {
                if(b.Id.ToLower().Trim() == id.ToLower().Trim() ) {
                    builder = b;
                    break;
                }
            }

            if(builder == null ) {
                throw new Exception($"Not found builder for id: {id.ToLower().Trim()}");
            }
            return ConstructInstanceForBuilder(builder);
        }

        private object ConstructInstanceForBuilder(ObjectBuilder builder) {
            switch(builder.Lifecycle) {
                case ObjectLifecycle.Singleton:
                    return ConstructSingletonInstanceForBuilder(builder);
                case ObjectLifecycle.Transient:
                    return ConstructTransientInstanceForBuilder(builder);
                default:
                    throw new Exception($"Unsupported lifecycle: {builder.Lifecycle}");
            }
        }

        private object ConstructTransientInstanceForBuilder(ObjectBuilder builder) {
            var parameters = ResolveConstructorParameters(builder);
            builder.ConstructTransientInstance(parameters.ToArray());
            InvokeConstructMethod(builder);
            Inject(builder.Instance);
            return builder.Instance;
        }

        private object ConstructSingletonInstanceForBuilder(ObjectBuilder builder ) {
            var parameters = ResolveConstructorParameters(builder);
            bool isNewObject = builder.ConstructSingletonInstance(parameters.ToArray());
            if(isNewObject ) {
                InvokeConstructMethod(builder);
            }

            Inject(builder.Instance);
            return builder.Instance;
        }

        private void InvokeConstructMethod(ObjectBuilder builder) {
            var methodInfo = builder.TypeConcrete.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public)
                .FirstOrDefault(m => m.Name == "Construct");
            if(methodInfo != null ) {
                var parameters = ResolveConstructMethodParameters(builder, methodInfo);
                methodInfo.Invoke(builder.Instance, parameters.ToArray());
            }
        }

        private IEnumerable<object> ResolveConstructMethodParameters(ObjectBuilder builder, MethodInfo methodInfo) {
            foreach(var parameter in methodInfo.GetParameters()) {
                string idVal = GetIdAttrValue(parameter);
                if(string.IsNullOrEmpty(idVal)) {
                    yield return ResolveAnyObject(parameter.ParameterType);
                } else {
                    yield return ResolveObjectWithId(parameter.ParameterType, idVal);
                }
            }
        }

        private string GetIdAttrValue(ParameterInfo parameter ) {
            object[] idAttrs = parameter.GetCustomAttributes(typeof(IdAttribute), false);
            if(idAttrs.Length == 0 ) {
                return string.Empty;
            } else {
                return (idAttrs[0] as IdAttribute).Id;
            }
        }

        private string GetIdAttribute(FieldInfo field ) {
            object[] idAttrs = field.GetCustomAttributes(typeof(IdAttribute), false);
            if(idAttrs.Length == 0 ) {
                return string.Empty;
            } else {
                return (idAttrs[0] as IdAttribute).Id;
            }
        }

        private string GetIdAttribute(PropertyInfo property ) {
            object[] idAttrs = property.GetCustomAttributes(typeof(IdAttribute), false);
            if (idAttrs.Length == 0) {
                return string.Empty;
            } else {
                return (idAttrs[0] as IdAttribute).Id;
            }
        }

        private IEnumerable<object> ResolveConstructorParameters(ObjectBuilder builder) {

            if(builder.IsMonoBehaviour || builder.FromFabric) {
                yield break;
            }


            var constructorInfo = builder.TypeConcrete.GetConstructors().FirstOrDefault();
            if (constructorInfo != null) {
                foreach (var parameter in constructorInfo.GetParameters()) {
                    object[] idAttrs = parameter.GetCustomAttributes(typeof(IdAttribute), false);
                    if (idAttrs.Length > 0) {
                        IdAttribute idAttr = idAttrs[0] as IdAttribute;
                        yield return ResolveObjectWithId(parameter.ParameterType, idAttr.Id);
                    } else {
                        yield return ResolveAnyObject(parameter.ParameterType);
                    }
                }
            }
        }

        private void Inject(object obj ) {
            Type objType = obj.GetType();
            FieldInfo[] publicFields = objType.GetFields(BindingFlags.Public | BindingFlags.Instance);
            FieldInfo[] privateFields = objType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
            IEnumerable<FieldInfo> allFields = publicFields.Concat(privateFields);
            InjectFields(obj, allFields.ToArray());

            PropertyInfo[] publicProperties = objType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            PropertyInfo[] privateProperties = objType.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance);
            IEnumerable<PropertyInfo> allProperties = publicProperties.Concat(privateProperties);
            InjectProperties(obj, allProperties.ToArray());
        }

        private void InjectFields(object obj, FieldInfo[] fields ) {
            foreach(FieldInfo field in fields ) {
                var attributes = field.GetCustomAttributes(typeof(InjectAttribute), true);
                if(attributes.Length > 0 ) {
                    string idVal = GetIdAttribute(field);
                    if(string.IsNullOrEmpty(idVal)) {
                        field.SetValue(obj, ResolveAnyObject(field.FieldType));
                    } else {
                        field.SetValue(obj, ResolveObjectWithId(field.FieldType, idVal));
                    }
                }
            }
        }

        private void InjectProperties(object obj, PropertyInfo[] properties ) {
            foreach(PropertyInfo property in properties ) {
                var attributes = property.GetCustomAttributes(typeof(InjectAttribute), true);
                if(attributes.Length > 0 ) {
                    string idVal = GetIdAttribute(property);
                    if (string.IsNullOrEmpty(idVal)) {
                        property.SetValue(obj, ResolveAnyObject(property.PropertyType));
                    } else {
                        property.SetValue(obj, ResolveObjectWithId(property.PropertyType, idVal));
                    }
                }
            }
        }
    }

    public class IdAttribute : Attribute
    {
        public string Id { get; private set; }

        public IdAttribute(string id) {
            Id = id;
        }
    }

    public class InjectAttribute : Attribute { }
}
