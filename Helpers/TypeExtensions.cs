using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace EPQMessenger.Helpers
{
    /// <summary>
    /// Contains static extension methods for the Type class.
    /// </summary>
    static class TypeExtensions
    {
        /// <summary>
        /// Gives a list of the types in the same namespace as that specified.
        /// </summary>
        /// <param name="type">The type to find the siblings of.</param>
        /// <returns>A Type array containing all the sibling types of the type specified.</returns>
        public static Type[] GetTypesInSameNamespace(this Type type)
        {
            string nspace = type.Namespace;
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(t => t.GetTypes())
                .Where(t => t.IsClass && t.Namespace == nspace)
                .ToArray<Type>();
        }
    }
}
