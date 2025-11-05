@echo off
REM MyBatis.NET Mapper Interface Generator Script for Windows

cd /d "%~dp0"

if "%1"=="" (
    echo Usage: generate.bat ^<command^> [options]
    echo.
    echo Commands:
    echo   gen ^<xml-file^>              - Generate from single XML file
    echo   gen-all [directory]         - Generate from all XML files in directory
    echo   help                        - Show help
    echo.
    exit /b 1
)

dotnet run --project Tools\MyBatis.Tools.csproj -- %*
