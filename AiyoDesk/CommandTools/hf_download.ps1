param(
  [Parameter(Mandatory=$true)][string]$ModelUrl,
  [Parameter(Mandatory=$true)][string]$TargetPath
)

$dir = Split-Path $TargetPath
if (!(Test-Path $dir)) {
    New-Item -ItemType Directory -Path $dir | Out-Null
}

Write-Host "下載中: $ModelUrl"
Write-Host "儲存到: $TargetPath"

# Invoke-WebRequest -Uri $ModelUrl -OutFile $TargetPath -UseBasicParsing
Invoke-WebRequest -Uri $ModelUrl -OutFile $TargetPath -Headers @{ "User-Agent" = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/119.0.0.0 Safari/537.36" }

Write-Host "✅ 下載完成"
