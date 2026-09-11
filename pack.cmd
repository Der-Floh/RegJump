@echo off
setlocal

cd /d "%~dp0"

set "VERSION=%~1"
if "%VERSION%"=="" set /p VERSION=Enter package version:

REM Strip leading "v" if entered, e.g. v1.2.3 -> 1.2.3
if /i "%VERSION:~0,1%"=="v" set "VERSION=%VERSION:~1%"

if "%VERSION%"=="" (
    echo No version entered.
    exit /b 1
)

dotnet restore RegJump.slnx || exit /b 1
dotnet build RegJump.slnx -c Release --no-restore -p:Version=%VERSION% || exit /b 1
dotnet test RegJump.Test/RegJump.Test.csproj -c Release --no-build || exit /b 1
dotnet pack RegJump/RegJump.csproj -c Release --no-build -p:Version=%VERSION% -o ./artifacts || exit /b 1

echo.
echo Packed RegJump %VERSION% into .\artifacts
