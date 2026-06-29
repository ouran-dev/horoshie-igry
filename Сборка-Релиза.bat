@echo off
chcp 65001 >nul
setlocal

cd /d "%~dp0"

set VERSION=%~1
if "%VERSION%"=="" (
  echo.
  echo  Использование: Сборка-Релиза.bat 1.0.0
  echo  Версия берётся из аргумента или по умолчанию 1.0.0
  echo.
  set VERSION=1.0.0
)

echo.
echo  Сборка релиза «Хорошие игры» v%VERSION%...
echo.

dotnet publish HoroshieIgry.csproj -c Release -r win-x64 --self-contained -o publish
if errorlevel 1 goto :failed

where vpk >nul 2>&1
if errorlevel 1 (
  echo  Устанавливаем vpk 1.2.0...
  dotnet tool install -g vpk --version 1.2.0
)

vpk pack -u HoroshieIgry -v %VERSION% -p publish -e HoroshieIgry.exe --packTitle "Хорошие игры" -o Releases
if errorlevel 1 goto :failed

echo.
echo  Готово!
echo  Установщик: Releases\Setup.exe
echo  Для публикации на GitHub:
echo    vpk upload github -o Releases --repoUrl https://github.com/USER/REPO --token TOKEN --publish --tag v%VERSION% --releaseName "Хорошие игры %VERSION%"
echo.
goto :eof

:failed
echo.
echo  Ошибка сборки релиза.
echo.
pause
exit /b 1
