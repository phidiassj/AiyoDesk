param(
  [Parameter(Mandatory=$true)][string]$ModelUrl,
  [Parameter(Mandatory=$true)][string]$TargetPath
)

# 關閉內建進度條，避免 UI 更新拖慢下載
$ProgressPreference = 'SilentlyContinue'

# 確保目錄存在
$dir = Split-Path $TargetPath
if (!(Test-Path $dir)) {
    New-Item -ItemType Directory -Path $dir | Out-Null
}

Write-Host "下載中: $ModelUrl"
Write-Host "儲存到: $TargetPath"
Write-Host "下載模型需要一些時間，請耐心等候，並請不要強制關閉本視窗。"

# 使用 .NET WebClient 執行下載
$wc = New-Object System.Net.WebClient
# 加上常見的瀏覽器標頭，避免被伺服器拒絕
$wc.Headers.Add(
    'User-Agent',
    'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 ' +
    '(KHTML, like Gecko) Chrome/119.0.0.0 Safari/537.36'
)
try {
    $wc.DownloadFile($ModelUrl, $TargetPath)
    Write-Host "✅ 下載完成"
}
catch {
    Write-Error "❌ 下載失敗：$($_.Exception.Message)"
}
finally {
    $wc.Dispose()
}
