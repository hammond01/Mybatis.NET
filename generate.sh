#!/bin/bash
# MyBatis.NET Mapper Interface Generator Script

cd "$(dirname "$0")"

if [ "$1" == "" ]; then
    echo "Usage: ./generate.sh <command> [options]"
    echo ""
    echo "Commands:"
    echo "  gen <xml-file>              - Generate from single XML file"
    echo "  gen-all [directory]         - Generate from all XML files in directory"
    echo "  help                        - Show help"
    echo ""
    exit 1
fi

dotnet run --project Tools/MyBatis.Tools.csproj -- "$@"
