# dotnet tool restore

# dotnet run --project .\benchmarks\CrossValidation.Benchmarks\CrossValidation.Benchmarks.csproj --configuration Release -- --artifacts ./generated/benchmarks --filter *.MustBenchmarks.*

dotnet test --configuration Release --collect:"XPlat Code Coverage;Format=lcov" --results-directory ./generated/coverage -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Exclude="[*]CrossValidation.WebApplication*,[*]CrossValidation.Resources*"

dotnet tool run reportgenerator -reports:"./generated/coverage/*/coverage.info" -targetdir:"./generated/coverage" -reporttypes:Html

Start-Process ./generated/coverage/index.html

# Remove-Item -Recurse -Force ./generated