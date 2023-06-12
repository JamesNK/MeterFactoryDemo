Demo using `IMeterFactory` to unit test .NET metrics.

Tests include:

* **BasicTests.cs** - Test default ASP.NET Core metrics.
* **CustomTests.cs** - Test a custom meter and counter.
  * The custom counter is added by an `IHostService`.
  * The service listens to a default ASP.NET Core counter.
  * Unwanted tags are removed and the duration seconds is converted to milliseconds.
