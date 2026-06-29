@echo off
chcp 65001 >nul
title Хорошие игры
cd /d "%~dp0"
echo.
echo  Запуск «Хорошие игры»...
echo.
taskkill /IM HoroshieIgry.exe /F >nul 2>&1
dotnet build
if errorlevel 1 goto :failed
dotnet run --no-build
if errorlevel 1 goto :failed
goto :eof

:failed
echo.
echo  Ошибка запуска. Убедитесь, что установлен .NET 8 SDK.
echo  Скачать: https://dotnet.microsoft.com/download/dotnet/8.0
echo.
pause
