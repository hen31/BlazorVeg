using Veg.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Veg.Repositories
{
    public class VegRepositoriesRegistration
    {
        public static List<IRepository> GetRepositories()
        {
            List<IRepository> repositories = new List<IRepository>();
            System.Reflection.Assembly ass = System.Reflection.Assembly.GetAssembly(typeof(VegRepositoriesRegistration));
            foreach (System.Reflection.TypeInfo typeInfo in ass.DefinedTypes)
            {
                if (typeInfo.ImplementedInterfaces.Contains(typeof(IRepository)))
                {
                    repositories.Add(ass.CreateInstance(typeInfo.FullName) as IRepository);
                }
            }
            return repositories;
        }

        public static IEnumerable<Type> GetRespositoryImplementationTypes()
        {
            List<Type> repositories = new List<Type>();
            System.Reflection.Assembly ass = System.Reflection.Assembly.GetAssembly(typeof(VegRepositoriesRegistration));
            foreach (System.Reflection.TypeInfo typeInfo in ass.DefinedTypes)
            {
                if (typeInfo.ImplementedInterfaces.Contains(typeof(IRepository)) && !typeInfo.IsAbstract)
                {
                    repositories.Add(typeInfo.AsType());
                }
            }
            return repositories;
        }
    }
}
