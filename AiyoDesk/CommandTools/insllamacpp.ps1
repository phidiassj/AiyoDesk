# 安裝 llama.cpp 自動化 PowerShell 腳本
# 使用方式：
# powershell -ExecutionPolicy Bypass -File install_llama.ps1 -Backend CUDA -CondaPath "C:\Miniforge3" -OutputDir "C:\llama_cpp"

param(
    [Parameter(Mandatory = $true)][string]$Backend,
    [Parameter(Mandatory = $true)][string]$CondaPath,
    [Parameter(Mandatory = $true)][string]$OutputDir
)

$ErrorActionPreference = "Stop"

function Download-File($url, $output) {
    Write-Host "下載: $url"
    Invoke-WebRequest -Uri $url -OutFile $output
}

function Install-Conda {
    $condaExe = Join-Path $CondaPath "Scripts\conda.exe"
    if (Test-Path $condaExe) {
        Write-Host "Conda 已安裝於 $CondaPath"
        return
    }
    $pkgDir = "$PSScriptRoot\packages"
    New-Item -ItemType Directory -Path $pkgDir -Force | Out-Null
    $installerPath = "$pkgDir\Miniforge3-Windows-x86_64.exe"
    if (!(Test-Path $installerPath)) {
        Download-File "https://github.com/conda-forge/miniforge/releases/latest/download/Miniforge3-Windows-x86_64.exe" $installerPath
    }
    Write-Host "開始安裝 Miniforge 到 $CondaPath..."
    Start-Process -FilePath $installerPath -ArgumentList "/InstallationType=JustMe", "/RegisterPython=0", "/S", "/D=$CondaPath" -Wait
}

function Create-Conda-Env {
    $condaExe = Join-Path $CondaPath "Scripts\conda.exe"
    Write-Host "建立 Python 3.11 環境..."
    & $condaExe create -n aiyodesk python=3.11 -y
}

function Download-LLAMACPP {
    $release = Invoke-RestMethod 'https://api.github.com/repos/ggml-org/llama.cpp/releases/latest'
    $pattern = \"llama-.*-bin-$Backend-x64\\.zip\"
    $assets = $release.assets
    $asset = $assets | Where-Object { $_.name -match $pattern } | Sort-Object name | Select-Object -Last 1

    if (-not $asset) {
        throw \"找不到符合後端 '$Backend' 的 llama.cpp 壓縮檔 (模式: $pattern)\"
    }
    $zipPath = "$PSScriptRoot\llama.zip"
    Download-File $asset.browser_download_url $zipPath

    Write-Host "解壓縮 llama.cpp 到 $OutputDir"
    New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
    Expand-Archive -LiteralPath $zipPath -DestinationPath $OutputDir -Force
}

# Main
Write-Host "======== 啟動 llama.cpp 安裝流程 ========"
Install-Conda
Create-Conda-Env
Download-LLAMACPP
Write-Host "======== 安裝完成 ========"
