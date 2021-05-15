using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks.Dataflow;

namespace RaceDirector.Pipeline
{
    /// <remarks>
    /// Dirty use of reflection and dynamic types. At least it's isolated here.
    /// </remarks>
    public static class PipelineBuilder
    {
        /// <summary>
        /// Linking source and target node properties based on their types.
        /// </summary>
        /// <param name="nodes">Nodes to inspect.</param>
        public static void LinkNodes(params INode[] nodes)
        {
            ForEachProperty(typeof(ISourceBlock<>), nodes, (sourceBlockType, sourceBlock) =>
            {
                var targetBlocks = new List<dynamic>();
                ForEachProperty(typeof(ITargetBlock<>), nodes, (targetBlockType, targetBlock) =>
                {
                    if (sourceBlockType.GenericTypeArguments[0].IsAssignableTo(targetBlockType.GenericTypeArguments[0]))
                        targetBlocks.Add(targetBlock);
                });
                if (targetBlocks.Count > 1)
                {
                    var broadcastBlockType = typeof(BroadcastBlock<>).MakeGenericType(sourceBlockType.GenericTypeArguments);
                    // CreateInstance can return null only for Nullable types
                    dynamic broadcastBlock = Activator.CreateInstance(broadcastBlockType, new object?[] { null })!;
                    DataflowBlock.LinkTo(sourceBlock, broadcastBlock);
                    sourceBlock = broadcastBlock;
                }
                foreach (dynamic targetBlock in targetBlocks)
                    DataflowBlock.LinkTo(sourceBlock, targetBlock);
            });
        }

        private static void ForEachProperty(Type genericTypeDefinition, object[] objects, Action<Type, dynamic> action)
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
