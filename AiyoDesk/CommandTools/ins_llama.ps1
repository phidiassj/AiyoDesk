param(
    [Parameter(Mandatory = $true)][string]$Backend,
    [Parameter(Mandatory = $true)][string]$OutputDir
)

$ErrorActionPreference = "Stop"

function Download-File($url, $output) {
    Write-Host "下載: $url"
    Invoke-WebRequest -Uri $url -OutFile $output
}

function Download-LLAMACPP {
    $release = Invoke-RestMethod 'https://api.github.com/repos/ggml-org/llama.cpp/releases/latest'
    $pattern = "llama-.*-bin-win-$Backend-x64\.zip"
    $assets = $release.assets
    $asset = $assets | Where-Object { $_.name -match $pattern } | Sort-Object name | Select-Object -Last 1

    if (-not $asset) {
        throw "找不到符合後端 '$Backend' 的 llama.cpp 壓縮檔 (模式: $pattern)"
    }

    $zipPath = "$PSScriptRoot\llama.zip"
    Download-File $asset.browser_download_url $zipPath

    Write-Host "解壓縮 llama.cpp 到 $OutputDir"
    New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
    Expand-Archive -LiteralPath $zipPath -DestinationPath $OutputDir -Force
}

# Main
Write-Host "======== 啟動 llama.cpp 安裝流程 ========"
Download-LLAMACPP
Write-Host "======== 安裝完成 ========"
