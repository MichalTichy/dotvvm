using DotVVM.Framework.Configuration;
using DotVVM.Framework.Controls;
using DotVVM.Framework.ResourceManagement;
using DotVVM.Framework.Testing;
using DotVVM.Framework.ViewModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace DotVVM.Framework.Tests.Binding
{
    [TestClass]
    public class IntegrityCheckTests
    {
        private const string _integrityHash = "sha256-hwg4gsxgFZhOsEEamdOYGBf13FyQuiTwlAQgxVSNgt4=";

        private string RenderResource(DotvvmConfiguration configuration, ScriptResource jquery)
        {
            var context = new TestDotvvmRequestContext()
            {
                Configuration = configuration,
                ResourceManager = new ResourceManager(configuration),
                ViewModel = new DotvvmViewModelBase()
            };

            using (var text = new StringWriter())
            {
                var html = new HtmlWriter(text, context);
                jquery.RenderLink(jquery.Location, html, context, ResourceConstants.JQueryResourceName);

                return text.GetStringBuilder().ToString();
            }
        }

        [TestMethod]
        public void IntegrityCheck_NoCheck()
        {
            //Arrange
            var configuration = DotvvmTestHelper.CreateConfiguration();

            configuration.Resources.Register(ResourceConstants.JQueryResourceName, new ScriptResource { Location = new UrlResourceLocation("https://cdnjs.cloudflare.com/ajax/libs/jquery/3.2.1/jquery.min.js"), RenderPosition = ResourceRenderPosition.Head, VerifyResourceIntegrity = false });

            var jquery = configuration.Resources.FindResource(ResourceConstants.JQueryResourceName) as ScriptResource;

            //Act
            string scriptTag = RenderResource(configuration, jquery);

            //Assert
            Assert.IsFalse(scriptTag.Contains("integrity"));
            Assert.IsFalse(scriptTag.Contains(_integrityHash));

        }

        [TestMethod]
        public void IntegrityCheck_NoCheck_WithIntegrityHash()
        {
            //Arrange
            var configuration = DotvvmTestHelper.CreateConfiguration();

            configuration.Resources.Register(ResourceConstants.JQueryResourceName, new ScriptResource { Location = new UrlResourceLocation("https://cdnjs.cloudflare.com/ajax/libs/jquery/3.2.1/jquery.min.js"), RenderPosition = ResourceRenderPosition.Head, VerifyResourceIntegrity = false, IntegrityHash = _integrityHash });

            var jquery = configuration.Resources.FindResource(ResourceConstants.JQueryResourceName) as ScriptResource;

            //Act
            string scriptTag = RenderResource(configuration, jquery);

            //Assert
            Assert.IsFalse(scriptTag.Contains("integrity"));
            Assert.IsFalse(scriptTag.Contains(_integrityHash));
        }

        [TestMethod]
        public void IntegrityCheck_ShouldFail()
        {
            //Arrange
            var configuration = DotvvmTestHelper.CreateConfiguration();

            configuration.Resources.Register(ResourceConstants.JQueryResourceName, new ScriptResource { Location = new UrlResourceLocation("https://cdnjs.cloudflare.com/ajax/libs/jquery/3.2.1/jquery.min.js"), RenderPosition = ResourceRenderPosition.Head, VerifyResourceIntegrity = true, IntegrityHash = "123" });

            var jquery = configuration.Resources.FindResource(ResourceConstants.JQueryResourceName) as ScriptResource;

            //Act
            string scriptTag = RenderResource(configuration, jquery);

            //Assert
            Assert.IsTrue(scriptTag.Contains("integrity"));
            Assert.IsFalse(scriptTag.Contains(_integrityHash));
        }

        [TestMethod]
        public void IntegrityCheck_ShouldSucceed()
        {
            //Arrange
            var configuration = DotvvmTestHelper.CreateConfiguration();

            configuration.Resources.Register(ResourceConstants.JQueryResourceName, new ScriptResource { Location = new UrlResourceLocation("https://cdnjs.cloudflare.com/ajax/libs/jquery/3.2.1/jquery.min.js"), RenderPosition = ResourceRenderPosition.Head, VerifyResourceIntegrity = true, IntegrityHash = _integrityHash });

            var jquery = configuration.Resources.FindResource(ResourceConstants.JQueryResourceName) as ScriptResource;

            //Act
            string scriptTag = RenderResource(configuration, jquery);

            //Assert
            Assert.IsTrue(scriptTag.Contains("integrity"));
            Assert.IsTrue(scriptTag.Contains(_integrityHash));
        }
    }
}
