# dotnet tool restore

# dotnet run --project ./benchmarks/CrossValidation.Benchmarks/CrossValidation.Benchmarks.csproj --configuration Release -- --artifacts ./artifacts/benchmarks --filter *.MustBenchmarks.*

dotnet test --configuration Release --collect:"XPlat Code Coverage;Format=lcov" --results-directory ./artifacts/coverage -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Exclude="[*]CrossValidation.WebApplication*,[*]CrossValidation.Resources*"

dotnet tool run reportgenerator -reports:"./artifacts/coverage/*/coverage.info" -targetdir:"./artifacts/coverage" -reporttypes:Html

Start-Process ./artifacts/coverage/index.html

# Remove-Item -Recurse -Force ./artifacts