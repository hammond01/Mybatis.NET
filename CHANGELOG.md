# Changelog

All notable changes to MyBatis.NET.SqlServer will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Planned

- Continue feature roadmap toward broader MyBatis Java parity
- Add provider abstraction groundwork for future `MyBatis.NET.Postgres`, `MyBatis.NET.MySql`, and more

## [1.0.0-preview.1] - 2026-03-11

### Changed

- Prepared first public preview baseline for `MyBatis.NET.SqlServer`
- Aligned repository documentation to the v1 package line and release flow

### Quality

- Verified Release build and non-integration test suite before preview packaging

### Added

- SQL Server package identity: `MyBatis.NET.SqlServer`
- XML-based SQL mapping with mapper namespace + statement id lookup
- Dynamic SQL support for `<if>`, `<where>`, `<set>`, `<choose>/<when>/<otherwise>`, `<foreach>`, `<trim>`
- Mandatory `returnSingle` contract for `<select>` to make return intent explicit
- Runtime mapper proxy generation via `DispatchProxy`
- Sync and async execution APIs in `SqlSession`
- SQL and parameter logging via `SqlSessionConfiguration`
- Mapper autoload from directories and embedded assembly resources
- Interface generator tool (`mybatis-gen`) for XML-to-C# mapper interfaces
- Result mapping based on compiled expression trees
- Result mapper cache optimized by type + column schema with stats APIs:
  - `ResultMapper.GetCacheStats()`
  - `ResultMapper.ClearCache()`

### Notes

- This is the first public preview for the renamed SQL Server package line.

## [1.0.0] - Planned

- First stable release after preview feedback and API freeze.

[1.0.0-preview.1]: https://github.com/hammond01/MyBatis.NET/releases/tag/v1.0.0-preview.1
[1.0.0]: https://github.com/hammond01/MyBatis.NET/releases/tag/v1.0.0
