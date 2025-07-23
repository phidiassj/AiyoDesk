param(
    [Parameter(Mandatory = $true)][string]$OutputDir
)

# 偵測 GPU 廠商
function Get-GpuVendor {
    $gpu = Get-WmiObject Win32_VideoController | Select-Object -First 1 -ExpandProperty AdapterCompatibility
    if ($gpu -match 'NVIDIA') { return 'nvidia' }
    if ($gpu -match 'AMD')    { return 'amd' }
    if ($gpu -match 'Intel')  { return 'intel' }
    return 'cpu'
}

$vendor = Get-GpuVendor
Write-Host "偵測到 GPU 廠商：$vendor"

# 安裝對應版本 PyTorch
switch ($vendor) {
    'nvidia' {
        Write-Host "安裝支援 CUDA 的 PyTorch..."
        pip install torch torchvision torchaudio --index-url https://download.pytorch.org/whl/cu121
    }
    'amd' {
        Write-Host "安裝支援 AMD 的 PyTorch..."
        pip install torch torchvision torchaudio --index-url https://download.pytorch.org/whl/rocm6.0
    }
    'intel' {
        pip install torch torchvision torchaudio --index-url https://download.pytorch.org/whl/xpu
    }
    default {
        Write-Host "安裝 CPU-only PyTorch..."
        pip install torch torchvision torchaudio --index-url https://download.pytorch.org/whl/cpu
    }
}

# 下載並解壓 ComfyUI
$zipUrl    = 'https://github.com/comfyanonymous/ComfyUI/archive/refs/heads/master.zip'
$outputZip = "$PSScriptRoot\ComfyUI-main.zip"
$targetDir = "$PSScriptRoot\ComfyUI"

Write-Host "下載 ComfyUI 原始碼 ZIP..."
Invoke-WebRequest -Uri $zipUrl -OutFile $outputZip -UseBasicParsing -Verbose

Write-Host "解壓縮 ComfyUI 到 $OutputDir"
New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
Expand-Archive -LiteralPath $outputZip -DestinationPath $OutputDir -Force

# 執行 requirements.txt 必要安裝
$extracted = Join-Path $OutputDir 'ComfyUI-master'

# 3. 安裝 requirements.txt
$reqFile = Join-Path $extracted 'requirements.txt'
if (Test-Path $reqFile) {
    Write-Host "`n開始執行 pip install -r requirements.txt ..." -ForegroundColor Cyan
    Push-Location $extracted
    pip install -r $reqFile
    Pop-Location
    Write-Host "`n所有 Python 相依套件已安裝完成。" -ForegroundColor Green
}
else {
    Write-Host "`n警告：找不到 requirements.txt，請檢查 ComfyUI 版本。" -ForegroundColor Yellow
}

Write-Host "ComfyUI 安裝成功"
