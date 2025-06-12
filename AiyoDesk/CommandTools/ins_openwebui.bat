setlocal enabledelayedexpansion

set "CondaPath=%~1"
set "CondaEnv=%~2"

echo [訊息] 啟動 Conda 環境中...
cd /d "%CondaPath%"
call activate.bat "%CondaEnv%"

python -m pip install open-webui