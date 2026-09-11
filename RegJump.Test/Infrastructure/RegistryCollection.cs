namespace RegJumpTest.Infrastructure;

/// <summary>
/// LastKey is a single global value, so the tests that write it must not run concurrently.
/// </summary>
[CollectionDefinition(DisableParallelization = true)]
public sealed class RegistryCollection;
