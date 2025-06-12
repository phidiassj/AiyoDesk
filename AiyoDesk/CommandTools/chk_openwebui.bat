@echo off
setlocal

REM 取得 pip list 中是否有 open-webui
pip list | findstr open-webui >nul

if %errorlevel%==0 (
    echo YES
) else (
    echo NO
)

endlocal