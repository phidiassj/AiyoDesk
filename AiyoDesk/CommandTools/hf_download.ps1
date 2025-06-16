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

Invoke-WebRequest -Uri $ModelUrl -OutFile $TargetPath -UseBasicParsing

Write-Host "✅ 下載完成"
