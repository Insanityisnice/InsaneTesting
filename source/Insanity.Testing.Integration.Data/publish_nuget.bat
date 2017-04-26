nuget pack .\Insanity.Testing.Integration.Data.csproj -properties Configuration=Release
nuget push .\Insanity.Testing.Integration.Data.1.0.0.nupkg -Source https://www.nuget.org/api/v2/package