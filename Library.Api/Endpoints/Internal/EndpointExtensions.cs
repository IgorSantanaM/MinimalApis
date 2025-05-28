using Microsoft.Extensions.Configuration;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Library.Api.Endpoints.Internal
{
    public static  class EndpointExtensions
    {
        public static void AddEndpoints<TMarker>(this IServiceCollection services, IConfiguration configuration)
        {
            AddEndpoints(services, typeof(TMarker), configuration);
        }

        public static void AddEndpoints(this IServiceCollection services, Type typeMarker, IConfiguration configuration)
        {
            IEnumerable<TypeInfo> endpointType = GetEndpointsTypesFromAssemblyContaining(typeMarker);

            foreach (TypeInfo type in endpointType)
            {
                type.GetMethod(nameof(IEndpoints.AddServices))?.Invoke(null, new object[] { services, configuration });
            }
        }

        public static void UseEndpoints<TMarker>(this IApplicationBuilder app)
        {
            UseEndpoints(app, typeof(TMarker));
        }
        public static void UseEndpoints(this IApplicationBuilder app, Type typeMarker)
        {
            IEnumerable<TypeInfo> endpointType = GetEndpointsTypesFromAssemblyContaining(typeMarker);

            foreach (TypeInfo type in endpointType)
            {
                type.GetMethod(nameof(IEndpoints.AddServices))?.Invoke(null, new object[] { app });
            }

        }
        private static IEnumerable<TypeInfo> GetEndpointsTypesFromAssemblyContaining(Type typeMarker)
        {
            return typeMarker.Assembly.DefinedTypes.Where(x => !x.IsAbstract && !x.IsInterface && typeof(IEndpoints).IsAssignableFrom(x));
        }
    }
}
