# MyBatis.NET.SqlServer v1.0.0-preview.1 - Release Summary

## Release Info

- Package: `MyBatis.NET.SqlServer`
- Version: `1.0.0-preview.1`
- Release date: `2026-03-11`

## Highlights

- First public preview baseline for the v1 package line
- Synced package metadata and repository documentation with current naming/versioning
- Re-validated quality gate before preview release

## Validation

- `dotnet build MyBatis.NET.csproj -c Release`
- `dotnet build Tools/MyBatis.Tools.csproj -c Release`
- `dotnet test Tests/MyBatis.NET.Tests.csproj -c Release --filter "Category!=Integration"`
- `dotnet pack MyBatis.NET.csproj -c Release --no-build --output nuget-packages`

## Notes

- Runtime package is preview-tagged to support controlled rollout and feedback.
- Tool package remains separate and unchanged.
