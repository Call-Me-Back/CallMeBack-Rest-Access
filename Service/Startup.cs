using System.Net.Http.Formatting;
using System.Reflection;
using System.Web.Http;

using Basics.AspNet.WebApi;
using Basics.Containers;

using FullStackTraining.CallMeBack.Rest.Access;

using Microsoft.Owin;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

using Owin;

[assembly: OwinStartup(typeof(Startup))]
[assembly: GlobalRoutePrefix("api")]

namespace FullStackTraining.CallMeBack.Rest.Access
{
    public sealed class Startup
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public void Configuration(IAppBuilder app)
        {
            IContainer container = InitializeContainer();

            HttpConfiguration config = GetWebApiConfiguration(container);
            app.UseWebApi(config);
        }

        private static IContainer InitializeContainer()
        {
            IContainerBuilder builder = Ioc.CreateBuilder();
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());
            CallMeBack.Domain.ContainerSetup.Setup(builder);
            IContainer container = Ioc.CreateContainer(builder);
            return container;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private static HttpConfiguration GetWebApiConfiguration(IContainer container)
        {
            var config = new HttpConfiguration { DependencyResolver = new BasicsDependencyResolver(container) };

            config.MapHttpAttributeRoutes(/*new GlobalPrefixProvider()*/);

            config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Default;
            //config.UseGlobalExceptionFilter(new GlobalExceptionFilter());
            //config.UseGlobalExceptionHandler(new GlobalExceptionHandler());

            config.Formatters.Clear();
            var jsonFormatter = new JsonMediaTypeFormatter {
                SerializerSettings = {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    DateTimeZoneHandling = DateTimeZoneHandling.Utc
                }
            };
            jsonFormatter.SerializerSettings.Converters.Add(new UnixEpochDateTimeConverter());
            jsonFormatter.SerializerSettings.Converters.Add(new StringEnumConverter());
            config.Formatters.Add(jsonFormatter);
            return config;
        }
    }
}
