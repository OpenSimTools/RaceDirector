using System;
using System.Collections.Generic;
using System.Reflection;

namespace RaceDirector.Pipeline
{
    /// <remarks>
    /// Dirty use of reflection and dynamic types. At least it's isolated here.
    /// </remarks>
    public static class PipelineBuilder
    {
        /// <summary>
        /// Linking observable and observer node properties based on their types.
        /// </summary>
        /// <param name="nodes">Nodes to inspect.</param>
        public static void LinkNodes(IEnumerable<INode> nodes)
        {
            ForEachProperty(typeof(IObservable<>), nodes, (observableType, observable) =>
            {
                ForEachProperty(typeof(IObserver<>), nodes, (observerType, observer) =>
                {
                    var observableGenericType = observableType.GenericTypeArguments[0];
                    var observerGenericType = observerType.GenericTypeArguments[0];
                    if (observerGenericType.IsAssignableFrom(observableGenericType))
                    {
                        observableType.InvokeMember(nameof(IObservable<object>.Subscribe), BindingFlags.InvokeMethod, null, observable, new[] { observer });
                    }
                });
            });
        }

        private static void ForEachProperty(Type genericTypeDefinition, IEnumerable<object> objects, Action<Type, dynamic> action)
        {
            foreach (var o in objects)
            {
                foreach (var property in o.GetType().GetProperties())
                {
                    var implementedGenericType = property.PropertyType.Implements(genericTypeDefinition);
                    if (implementedGenericType is not null)
                    {
                        dynamic? propertyValue = property.GetValue(o);
                        if (propertyValue is not null)
                            action(implementedGenericType, propertyValue);
                    }
                }
            }
        }

        private static Type? Implements(this Type type, Type genericTypeDefinition)
        {
            if (!genericTypeDefinition.IsInterface)
                throw new ArgumentException("Currently working only for interface types");
            if (type.IsInterface && type.IsGenericType && type.GetGenericTypeDefinition() == genericTypeDefinition)
                return type;
            foreach (var i in type.GetInterfaces())
                if (i.IsGenericType && i.GetGenericTypeDefinition() == genericTypeDefinition)
                    return i;
            return null;
        }
    }
}
