// Add this attribute to the assembly-level to enable parallel execution
[assembly: Parallelizable(ParallelScope.Fixtures)]
[assembly: LevelOfParallelism(10)] // Adjust the level of parallelism as needed
