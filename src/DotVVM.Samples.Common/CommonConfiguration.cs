using DotVVM.Framework.Compilation.Javascript;
using DotVVM.Framework.Compilation.Javascript.Ast;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.ResourceManagement;
using DotVVM.Samples.BasicSamples;
using DotVVM.Samples.BasicSamples.Controls;
using DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.StaticCommand;
using DotVVM.Samples.Common.Utilities;
using DotVVM.Samples.Common.ViewModels.FeatureSamples.DependencyInjection;
using DotVVM.Samples.Common.ViewModels.FeatureSamples.StaticCommand;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Samples.Common
{
    public static class CommonConfiguration
    {
        public static void ConfigureServices(IDotvvmServiceCollection dotvvmServices)
        {
            var services = dotvvmServices.Services;
            // normally, you'd put that to DotvvmStartup, but I need to test both options
            services.Configure<DotvvmMarkupConfiguration>(config => {
                // import namespaces
                config.ImportedNamespaces.Add(new Framework.Compilation.NamespaceImport("DotVVM.Samples.BasicSamples.TestNamespace1", "TestNamespaceAlias"));
                config.ImportedNamespaces.Add(new Framework.Compilation.NamespaceImport("DotVVM.Samples.BasicSamples.TestNamespace2"));
            });
            services.AddScoped<DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.Redirect.RedirectService>();
            services.Configure<DotvvmResourceRepository>(RegisterResources);

            services.Configure<JavascriptTranslatorConfiguration>(c => {
                c.MethodCollection.AddMethodTranslator(typeof(JavaScriptUtils),
                   nameof(JavaScriptUtils.LimitLength),
                   new GenericMethodCompiler((a) => new JsIdentifierExpression("limitLength").Invoke(a)));
            });

            dotvvmServices.AddDefaultTempStorages("Temp");
            services.AddScoped<ViewModelScopedDependency>();
            services.AddSingleton<IGreetingComputationService, HelloGreetingComputationService>();
            services.AddSingleton<FoodSevice>();
        }

        private static void RegisterResources(DotvvmResourceRepository resources)
        {
            resources.Register("ControlSamples_SpaContentPlaceHolder_testCss", new StylesheetResource(new FileResourceLocation("Content/testResource.css")));
            resources.Register("ControlSamples_SpaContentPlaceHolder_testJs", new ScriptResource(new FileResourceLocation("Scripts/testResource.js")));
            resources.Register("ControlSamples_SpaContentPlaceHolder_MasterPageResource", new ScriptResource(new FileResourceLocation("Scripts/testResource2.js")));

            resources.Register("FeatureSamples_Resources_CdnUnavailableResourceLoad", new ScriptResource() {
                Location = new UrlResourceLocation("~/nonexistentResource.js"),
                LocationFallback = new ResourceLocationFallback("window.dotvvmTestResource", new FileResourceLocation("~/Scripts/testResource.js"))
            });

            resources.Register("FeatureSamples_Resources_CdnScriptPriority", new ScriptResource {
                Location = new UrlResourceLocation("/Scripts/testResource.js"),
                LocationFallback = new ResourceLocationFallback("window.dotvvmTestResource", new FileResourceLocation("~/Scripts/testResource2.js"))
            });

            resources.Register("FeatureSamples_Resources_RequiredOnPostback", new ScriptResource() {
                Location = new UrlResourceLocation("~/nonexistentResource.js"),
                LocationFallback = new ResourceLocationFallback("window.dotvvmTestResource", new FileResourceLocation("~/Scripts/testResource.js"))
            });

            resources.Register("Errors_InvalidLocationFallback", new ScriptResource {
                Location = new FileResourceLocation("~/Scripts/testResource.js"),
                LocationFallback = new ResourceLocationFallback("window.dotvvmTestResource", new FileResourceLocation("~/Scripts/testResource2.js"))
            });

            // resource that triggers the circular dependency check in the render phase
            var circular = new ScriptResource { Location = new FileResourceLocation("~/Scripts/testResource.js") };
            resources.Register("Errors_ResourceCircularDependency", circular);
            var circular2 = new ScriptResource {
                Location = new FileResourceLocation("~/Scripts/testResource2.js"),
                Dependencies = new [] { "Errors_ResourceCircularDependency" }
            };
            resources.Register("Errors_ResourceCircularDependency2", circular2);
            circular.Dependencies = new[] { "Errors_ResourceCircularDependency" };
            

            resources.Register("extenders", new ScriptResource {
                Location = new FileResourceLocation("Scripts/ClientExtenders.js")
            });

            resources.Register(nameof(StopwatchPostbackHandler), new ScriptResource {
                Location = new FileResourceLocation($"~/Scripts/{nameof(StopwatchPostbackHandler)}.js"),
                Dependencies = new[] { "dotvvm.internal" }
            });
            resources.Register(nameof(ErrorCountPostbackHandler), new ScriptResource {
                Location = new FileResourceLocation($"~/Scripts/{nameof(ErrorCountPostbackHandler)}.js"),
                Dependencies = new[] { "dotvvm.internal" }
            });

            // dev files
            resources.SetEmbeddedResourceDebugFile("knockout", "../DotVVM.Framework/Resources/Scripts/knockout-latest.debug.js");
            resources.SetEmbeddedResourceDebugFile("dotvvm.internal", "../DotVVM.Framework/Resources/Scripts/DotVVM.js");
            resources.SetEmbeddedResourceDebugFile("dotvvm.debug", "../DotVVM.Framework/Resources/Scripts/DotVVM.Debug.js");
            resources.SetEmbeddedResourceDebugFile("dotvvm.fileupload-css", "../DotVVM.Framework/Resources/Scripts/DotVVM.FileUploads.css");

            // test debug version of knockout
            //((ScriptResource)config.Resources.FindResource("knockout"))
            //    .Location = new FileResourceLocation("..\\DotVVM.Framework\\Resources\\Scripts\\knockout-latest.debug.js");

        }
    }
}
