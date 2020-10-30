# SharpLang
Fibers and events for .Net

Sharplang is a fiber, event, threading, and messaging library for .Net. It is heavily
inspired by Retlang: https://www.nuget.org/packages/retlang

SHARPLANG IS UNDER HEAVY DEVELOPMENT AND IS INCOMPLETE. I PREVIOUSLY PUBLISHED A SIMILAR
LIBRARY WITH THE SAME NAME, WHICH IS NOW REMOVED FROM GITHUB.

Differences from Retlang will be:
- Support for async code
- Multicast event syntax (via +=)
- Allow synchronous use of fibers
- Timespans to represent time
- Ability to "lock" if needed

SharpLang will also build on some lessons learned from working with Retlang for almost a decade:
- Better debugging support
- Better introspection and fiber saftey