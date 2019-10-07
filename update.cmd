@ECHO OFF

:run
dotnet tool update -g --no-cache --add-source https://kzu.io/index.json dotnet-guit

POPD >NUL
ENDLOCAL
ECHO ON
