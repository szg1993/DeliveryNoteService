@ECHO OFF

REM initialization once for repository, creates .config/dotnet-tools.json
REM dotnet new tool-manifest
REM dotnet tool install Swashbuckle.AspNetCore.Cli --version 6.9.0

dotnet tool restore
dotnet build -c Debug

REM working directory has to be where the dll is, to find the appsettings
SET ORIG_DIR=%CD%
PUSHD "src\WebApp\bin\Debug\net8.0" 2>NUL

REM use appsettings.development.json and ignore if rabbitMQ is not available
SET ASPNETCORE_ENVIRONMENT=Development

SET WEB_APP_DLL=%~1

dotnet tool run swagger tofile --output "%ORIG_DIR%\openapi.json" "%WEB_APP_DLL%" v1

POPD
