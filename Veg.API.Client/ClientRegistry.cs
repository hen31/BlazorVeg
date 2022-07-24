using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Veg.API.Client
{
    public class ClientRegistry
    {
        public static IEnumerable<Type> GetClientImplementationTypes()
        {
            List<Type> repositories = new List<Type>();
            System.Reflection.Assembly ass = System.Reflection.Assembly.GetAssembly(typeof(ClientRegistry));
            foreach (System.Reflection.TypeInfo typeInfo in ass.DefinedTypes)
            {
                if (typeInfo.ImplementedInterfaces.Contains(typeof(IClient)) && !typeInfo.IsAbstract)
                {
                    repositories.Add(typeInfo.AsType());
                }
            }
            return repositories;
        }
    }
}
