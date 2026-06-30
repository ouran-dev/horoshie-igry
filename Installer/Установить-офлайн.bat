@echo off
chcp 65001 >nul
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0Install-Offline.ps1" -SourceDir "%~dp0"
pause
