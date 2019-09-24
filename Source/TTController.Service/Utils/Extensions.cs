using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Loader;

namespace TTController.Service.Utils
{
    public static class Extensions
    {
        public static IEnumerable<Type> FindImplementations(this Type type)
        {
            var types = AssemblyLoadContext.All.SelectMany(c => c.Assemblies)
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsClass && !t.IsAbstract);

            if (type.IsInterface)
                return types.Where(t => type.IsAssignableFrom(t));
            else
                return types.Where(t => t.IsSubclassOf(type));
        }
        public static IEnumerable<T> RotateLeft<T>(this IEnumerable<T> enumberable, int rotate) 
            => enumberable.Skip(rotate).Concat(enumberable.Take(rotate));

        public static IEnumerable<T> RotateRight<T>(this IEnumerable<T> enumberable, int rotate)
            => enumberable.RotateLeft(enumberable.Count() - rotate);
    }
}
