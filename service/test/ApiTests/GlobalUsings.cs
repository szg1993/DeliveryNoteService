global using ApiTests.TestSuite;
global using ApiTests.TestSuite.Messaging;
global using FluentAssertions;
global using Medusa.Enap.Common.Testing;
global using Medusa.Enap.Common.Testing.Infrastructure;
global using Medusa.Enap.Common.Testing.Utils;
global using QueryString = Microsoft.AspNetCore.Http.QueryString;

[assembly: CollectionBehavior(DisableTestParallelization = false, MaxParallelThreads = 5)]