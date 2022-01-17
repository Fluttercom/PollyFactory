# Introduction 
A library that simplifies usage and configuration of Polly policies.

# Getting Started
1.	Create PollyConfig section in your config file
2.	In Startup.cs call `services.AddSingleton(PollyFactory.LoadRegistryFromConfig(Configuration, "PollyConfig"));`
3.	Import PolicyRegistry instance in your service
4.  Get your Policy by name: `_retrier = registry.Get<RetryPolicy>("MyRetrier");`
---
If you need a RetryPolicy with OnRetry handler, call `PollyFaсtory.CreateRetrierWithHandler(configSection, onRetry)`

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
* (Async)Retrier - retries forever with the "Waits delays.
* Simple(Async)Retrier - retries "Retries" times with "Waits" delays.
* (Async)CircuitBreaker - after "Retries" times fails for "Waits[0]" seconds.