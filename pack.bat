@echo off

set /p VERSION=Enter package version: 

REM Strip leading "v" if entered, e.g. v1.2.3 -> 1.2.3
if /i "%VERSION:~0,1%"=="v" set "VERSION=%VERSION:~1%"

dotnet restore RegJump/RegJump.csproj
dotnet build RegJump/RegJump.csproj -c Release --no-restore
dotnet pack RegJump/RegJump.csproj -c Release --no-build -o ./artifacts -p:Version=%VERSION%
