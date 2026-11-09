global using ApiTests.TestSuite;
global using FluentAssertions;
global using Common.Testing;
global using Common.Testing.Infrastructure;
global using Common.Testing.Utils;
global using QueryString = Microsoft.AspNetCore.Http.QueryString;

[assembly: CollectionBehavior(DisableTestParallelization = false, MaxParallelThreads = 5)]