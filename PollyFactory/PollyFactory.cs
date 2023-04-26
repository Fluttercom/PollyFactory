using FlutterEffect.PollyFactory.ConfigSections;
using Microsoft.Extensions.Configuration;
using Polly;
using Polly.Registry;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FlutterEffect.PollyFactory
{
    public class PollyFactory
    {
        public static PolicyRegistry LoadRegistryFromConfig(IConfiguration config, string configSection)
        {
            var _pollyConfig = new PollyConfigSection();
            config.GetSection(configSection).Bind(_pollyConfig);
            return CreatePolicyRegistry(_pollyConfig);
        }

        public static PolicyRegistry CreatePolicyRegistry(PollyConfigSection config)
        {
            var registry = new PolicyRegistry();
            foreach (var entry in config)
            {
                registry.Add(entry.Key, CreatePolicy(entry.Value));
            }
            return registry;
        }

        public static IsPolicy CreatePolicy(IConfiguration config, string configSection, string key)
        {
            var pollyConfig = new PollyConfigSection();
            config.GetSection(configSection).Bind(pollyConfig);
            if (pollyConfig.TryGetValue(key, out var entry))
            {
                return CreatePolicy(entry);
            }
            else
                throw new Exception($"Cannot find section with key {key}");
        }

        public static IsPolicy CreatePolicy(PollyConfigEntry entry)
        {
            PolicyBuilder pbuilder = Policy.Handle<Exception>();
            switch (entry.Type)
            {
                case "Retrier":
                    IsPolicy policy = pbuilder.WaitAndRetryForever((attempt) => RetryTimeoutProvider(attempt, entry));
                    if (entry.Timeout > 0)
                        policy = Policy.Wrap(policy as ISyncPolicy, Policy.Timeout(entry.Timeout, Polly.Timeout.TimeoutStrategy.Pessimistic));
                    return policy;

                case "AsyncRetrier":
                    policy = pbuilder.WaitAndRetryForeverAsync((attempt) => RetryTimeoutProvider(attempt, entry));
                    if (entry.Timeout > 0)
                        policy = Policy.WrapAsync(policy as IAsyncPolicy, Policy.TimeoutAsync(entry.Timeout));
                    return policy;

                case "SimpleAsyncRetrier":
                    policy = pbuilder.WaitAndRetryAsync(entry.Retries, (attempt) => RetryTimeoutProvider(attempt, entry));
                    if (entry.Timeout > 0)
                        policy = Policy.WrapAsync(policy as IAsyncPolicy, Policy.TimeoutAsync(entry.Timeout));
                    return policy;

                case "SimpleRetrier":
                    policy = pbuilder.WaitAndRetry(entry.Retries, (attempt) => RetryTimeoutProvider(attempt, entry));
                    if (entry.Timeout > 0)
                        policy = Policy.Wrap(policy as ISyncPolicy, Policy.Timeout(entry.Timeout, Polly.Timeout.TimeoutStrategy.Pessimistic));
                    return policy;

                case "CircuitBreaker":
                    policy = pbuilder.CircuitBreaker(entry.Retries, TimeSpan.FromSeconds(entry.Waits[0]));
                    return policy;
                case "AsyncCircuitBreaker":
                    policy = pbuilder.CircuitBreakerAsync(entry.Retries, TimeSpan.FromSeconds(entry.Waits[0]));
                    return policy;
                default:
                    throw new Exception($"{entry.Type} was not found");
            }
        }

        /// <summary>
        /// Create retrier with onRetry handler
        /// </summary>
        /// <param name="configSection">config section that points to PollyConfigEntry</param>
        /// <param name="onRetry">Handler</param>
        /// <returns></returns>
        public static IsPolicy CreateRetrierWithHandler(IConfigurationSection configSection, Action<Exception, TimeSpan> onRetry)
        {
            PollyConfigEntry entry = new PollyConfigEntry();
            configSection.Bind(entry);
            PolicyBuilder pbuilder = Policy.Handle<Exception>();
            switch (entry.Type)
            {
                case "Retrier":
                    IsPolicy policy = pbuilder.WaitAndRetryForever((attempt) => RetryTimeoutProvider(attempt, entry), onRetry);
                    if (entry.Timeout > 0)
                        policy = Policy.Wrap(policy as ISyncPolicy, Policy.Timeout(entry.Timeout, Polly.Timeout.TimeoutStrategy.Pessimistic));
                    return policy;

                case "AsyncRetrier":
                    policy = pbuilder.WaitAndRetryForeverAsync((attempt) => RetryTimeoutProvider(attempt, entry), onRetry);
                    if (entry.Timeout > 0)
                        policy = Policy.WrapAsync(policy as IAsyncPolicy, Policy.TimeoutAsync(entry.Timeout));
                    return policy;

                case "SimpleAsyncRetrier":
                    policy = pbuilder.WaitAndRetryAsync(entry.Retries, (attempt) => RetryTimeoutProvider(attempt, entry), onRetry);
                    if (entry.Timeout > 0)
                        policy = Policy.WrapAsync(policy as IAsyncPolicy, Policy.TimeoutAsync(entry.Timeout));
                    return policy;

                case "SimpleRetrier":
                    policy = pbuilder.WaitAndRetry(entry.Retries, (attempt) => RetryTimeoutProvider(attempt, entry), onRetry);
                    if (entry.Timeout > 0)
                        policy = Policy.Wrap(policy as ISyncPolicy, Policy.Timeout(entry.Timeout, Polly.Timeout.TimeoutStrategy.Pessimistic));
                    return policy;

                default:
                    throw new Exception($"{entry.Type} was not found");
            }
        }

        static TimeSpan RetryTimeoutProvider(int attempt, PollyConfigEntry entry)
        {
            if (attempt > entry.Waits.Count)
                return TimeSpan.FromSeconds(entry.Waits[entry.Waits.Count - 1]);
            else
                return TimeSpan.FromSeconds(entry.Waits[attempt - 1]);
        }
    }
}
