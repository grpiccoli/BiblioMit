using System.Reflection;

namespace BiblioMit.Services
{
    public static class ReflectiveEnumerator
    {
        static ReflectiveEnumerator() { }

        public static IEnumerable<T> GetEnumerableOfType<T>(params object[] constructorArgs) where T : class
        {
            List<T> objects = new();
            IEnumerable<Type>? types = Assembly.GetAssembly(typeof(T))?.GetTypes()
                .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(T)));
            if (types != null)
            {
                foreach (Type type in types)
                {
                    object? instance = Activator.CreateInstance(type, constructorArgs);
                    if (instance != null)
                    {
                        objects.Add((T)instance);
                    }
                }
            }

            return objects;
        }
    }
}
