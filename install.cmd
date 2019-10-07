@ECHO OFF

:run
dotnet tool install -g --no-cache --add-source https://kzu.io/index.json dotnet-guit

POPD >NUL
ENDLOCAL
ECHO ON
