@echo off
chcp 65001 >nul
setlocal EnableExtensions

cd /d "%~dp0"

echo.
echo  ═══════════════════════════════════════════════════════
echo   Сборка и публикация «Хорошие игры»
echo  ═══════════════════════════════════════════════════════
echo.

for /f "usebackq delims=" %%V in (`powershell -NoProfile -Command "(Select-Xml -Path 'HoroshieIgry.csproj' -XPath '//Version').Node.InnerText"`) do set VERSION=%%V

if not "%~1"=="" set VERSION=%~1

if "%VERSION%"=="" (
  echo  Не удалось прочитать версию из HoroshieIgry.csproj
  goto :failed
)

echo  Версия: %VERSION%
echo.

powershell -NoProfile -ExecutionPolicy Bypass -File "Tools\Ensure-AppIcon.ps1"
if errorlevel 1 goto :failed

echo  [1/5] Публикация основной версии (с автообновлением)...
dotnet publish HoroshieIgry.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=false -o publish
if errorlevel 1 goto :failed

echo  [2/5] Публикация офлайн-версии (без автообновления)...
dotnet publish HoroshieIgry.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=false -p:OfflineDistribution=true -o publish-offline
if errorlevel 1 goto :failed

where vpk >nul 2>&1
if errorlevel 1 (
  echo  Устанавливаем vpk 1.2.0...
  dotnet tool install -g vpk --version 1.2.0
)

if not exist Releases mkdir Releases

echo  [3/5] Упаковка Setup.exe (Velopack, автообновление)...
vpk pack -u HoroshieIgry -v %VERSION% -p publish -e HoroshieIgry.exe ^
  --packTitle "Хорошие игры" ^
  --packAuthors "Хорошево" ^
  --icon "Assets\Brand\AppIcon.ico" ^
  --splashImage "Assets\Brand\logo.png" ^
  --instLocation Either ^
  --shortcuts None ^
  --instWelcome "Installer\ru\welcome-online.txt" ^
  --instConclusion "Installer\ru\conclusion-online.txt" ^
  -o Releases
if errorlevel 1 goto :failed

if exist "Releases\HoroshieIgry-win-Setup.exe" (
  copy /Y "Releases\HoroshieIgry-win-Setup.exe" "Releases\Setup.exe" >nul
)

echo  [4/5] Сборка офлайн-пакета для флешки...
set OFFLINE_DIR=Releases\Offline\HoroshieIgry-%VERSION%-win-x64
if exist "%OFFLINE_DIR%" rmdir /s /q "%OFFLINE_DIR%"
mkdir "%OFFLINE_DIR%"

xcopy /E /I /Y "publish-offline\*" "%OFFLINE_DIR%\" >nul
copy /Y "Installer\Install-Offline.ps1" "%OFFLINE_DIR%\" >nul
copy /Y "Installer\Установить-офлайн.bat" "%OFFLINE_DIR%\" >nul

powershell -NoProfile -Command "Compress-Archive -Path '%OFFLINE_DIR%\*' -DestinationPath 'Releases\HoroshieIgry-Offline-%VERSION%-win-x64.zip' -Force"
if errorlevel 1 goto :failed

echo  [5/5] Публикация на GitHub Releases...
if "%GITHUB_TOKEN%"=="" (
  echo.
  echo  GITHUB_TOKEN не задан — сборка готова локально, загрузка пропущена.
  echo  Чтобы опубликовать:
  echo    set GITHUB_TOKEN=ваш_токен
  echo    vpk upload github -o Releases --repoUrl https://github.com/ouran-dev/horoshie-igry --token %%GITHUB_TOKEN%% --publish --tag v%VERSION% --releaseName "Хорошие игры %VERSION%"
  echo.
  goto :done
)

vpk upload github -o Releases --repoUrl https://github.com/ouran-dev/horoshie-igry --token %GITHUB_TOKEN% --publish --tag v%VERSION% --releaseName "Хорошие игры %VERSION%"
if errorlevel 1 goto :failed

echo.
echo  Релиз v%VERSION% опубликован на GitHub.
goto :done

:done
echo.
echo  ═══════════════════════════════════════════════════════
echo   Готово!
echo  ═══════════════════════════════════════════════════════
echo.
echo   Онлайн (автообновление):  Releases\Setup.exe
echo                             (тот же файл: HoroshieIgry-win-Setup.exe)
echo   Офлайн (запасной):        Releases\HoroshieIgry-Offline-%VERSION%-win-x64.zip
echo   Распаковать офлайн:       Releases\Offline\HoroshieIgry-%VERSION%-win-x64\
echo                             Запустить Установить-офлайн.bat
echo.
goto :eof

:failed
echo.
echo  Ошибка сборки или публикации.
echo.
pause
exit /b 1
