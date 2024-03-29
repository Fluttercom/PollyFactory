# Introduction 
A library that simplifies usage and configuration of Polly policies.

[![Nuget](https://img.shields.io/nuget/v/FlutterEffect.PollyFactory)](https://www.nuget.org/packages/FlutterEffect.PollyFactory/)

# Getting Started
1.  Install FlutterEffect.PollyFactory nuget package
2.	Create PollyConfig section in your config file
3.	In Startup.cs call `services.AddSingleton(PollyFactory.LoadRegistryFromConfig(Configuration, "PollyConfig"));`
4.	Import PolicyRegistry instance in your service
5.  Get your Policy by name: `_retrier = registry.Get<RetryPolicy>("MyRetrier");`
---
If you need a RetryPolicy with OnRetry handler, call `PollyFabric.CreateRetrierWithHandler(configSection, onRetry)`
- Retriers are thread-safe, so it's ok to use the same instance across app.
- Circuit breakers have own state, so use separate CB in each thread.
- To create a new instance of a Policy from config, use `PollyFabric.CreatePolicy(config, configSection, PolicyKey)`

# Config section reference
```
  "PollyConfig": {
    "MyRetrier": { //name of your policy
      "Type": "SimpleRetrier", //Retrier type
      "Waits": [ 10, 60, 600, 1800 ], //array of wait times in ms
      "Retries": 3 //tries count (for simple retrier and CB)
      "Timeout": 1000 //optional timeout to execute a task (note: Policy type would be PolicyWrap),
    }
  }
```

# Available policies
* (Async)Retrier - retries forever with the "Waits" delays.
* Simple(Async)Retrier - retries "Retries" times with the "Waits" delays.
* (Async)CircuitBreaker - after "Retries" times fails for "Waits[0]" seconds.
