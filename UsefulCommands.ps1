dotnet tool restore

# Run benchmark
sudo pwsh
dotnet run --project ./benchmarks/CrossValidation.Benchmarks/CrossValidation.Benchmarks.csproj --configuration Release -- --artifacts ./artifacts/benchmarks --filter *.MustBenchmarks.*

# Get code coverage
dotnet test --configuration Release --collect:"XPlat Code Coverage;Format=lcov" --results-directory ./artifacts/coverage -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Exclude="[*]CrossValidation.WebApplication*,[*]CrossValidation.Resources*"
dotnet tool run reportgenerator -reports:"./artifacts/coverage/*/lcov.info" -targetdir:"./artifacts/coverage" -reporttypes:Html
Start-Process ./artifacts/coverage/index.html
Remove-Item -Recurse -Force ./artifacts

# Test NuGet packages locally
dotnet pack --configuration Release --output ./artifacts -p:Version=0.4.1 -p:SymbolPackageFormat=snupkg --include-symbols
dotnet new nugetconfig
mkdir localNuGetPackages
dotnet nuget add source ./localNuGetPackages --name LocalNuGetPackages