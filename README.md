# SharpLang
Fibers and events for .Net

Sharplang is a fiber, event, threading, and messaging library for .Net. It is heavily
inspired by Retlang: https://www.nuget.org/packages/retlang

For the impatient, differences from Retlang are:
- Support for async code
- Fibers run within the Task system
- Multicast event syntax (via +=)
- Timespans to represent time
- Ability to "lock" if needed
- Ability to assert that code is running on a fiber