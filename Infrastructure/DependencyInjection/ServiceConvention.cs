using GoldRush.Core.Services;
using StructureMap;
using StructureMap.Graph;
using StructureMap.Graph.Scanning;
using StructureMap.Pipeline;
using System;
using System.Linq;

namespace GoldRush.Infrastructure.DependencyInjection
{
    public class ServiceConvention : IRegistrationConvention
    {
        public void ScanTypes(TypeSet types, Registry registry)
        {
            types.FindTypes(TypeClassification.Concretes | TypeClassification.Closed).ToList().ForEach(type =>
            {
                var entityType = GetGenericParamFor(type, typeof(BaseService<,>));
                if (entityType != null)
                {
                    var genType = typeof(IService<,>).MakeGenericType(entityType);
                    registry.For(genType).Use(new ConfiguredInstance(type));
                }
            });
        }

        private static Type[] GetGenericParamFor(Type typeToInspect, Type genericType)
        {
            var baseType = typeToInspect.BaseType;
            if (baseType != null
                && baseType.IsGenericType
                && baseType.GetGenericTypeDefinition().Equals(genericType))
            {
                return baseType.GetGenericArguments();
            }

            return null;
        }
    }
}
