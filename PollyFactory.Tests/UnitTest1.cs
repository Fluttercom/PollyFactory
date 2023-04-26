using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Wrap;
using System;


namespace FlutterEffect.PollyFactory.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void LoadConfigs()
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            var registry = PollyFactory.LoadRegistryFromConfig(config, "PollyConfig");
            registry.Get<PolicyWrap>("Retrier1");
            registry.Get<RetryPolicy>("Retrier2");
            registry.Get<AsyncRetryPolicy>("Retrier3");
            registry.Get<CircuitBreakerPolicy>("CB1");
            registry.Get<AsyncCircuitBreakerPolicy>("CB2");
        }

        [TestMethod]
        public void RetrierWithHandler()
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            var cs = config.GetSection("PollyConfig:Retrier2");
            var retrier = PollyFactory.CreateRetrierWithHandler(cs, OnRetry) as RetryPolicy;
            Assert.IsNotNull(retrier);
            Assert.ThrowsException<Exception>(() => retrier.Execute(() => throw new Exception("test")));
        }

        [TestMethod]
        public void ReturnValue()
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            var registry = PollyFactory.LoadRegistryFromConfig(config, "PollyConfig");
            var retrier = registry.Get<RetryPolicy>("Retrier2");
            Assert.IsTrue(retrier.Execute(() => { return true; }));
            var cb = registry.Get<CircuitBreakerPolicy>("CB1");
            Assert.IsTrue(cb.Execute(() => { return true; }));
        }


        private void OnRetry(Exception e, TimeSpan t)
        {

        }
    }
}
