param(
    [Parameter(Mandatory = $true)][string]$Backend,
    [Parameter(Mandatory = $true)][string]$OutputDir
)

$ErrorActionPreference = "Stop"

# 強制載入 HttpClient 所在的 Assembly
Add-Type -AssemblyName System.Net.Http

function Download-File {
    param(
        [Parameter(Mandatory=$true)][string]$Url,
        [Parameter(Mandatory=$true)][string]$OutputPath
    )

    # 確保目錄存在
    $dir = Split-Path $OutputPath
    if (-not (Test-Path $dir)) {
        New-Item -ItemType Directory -Path $dir | Out-Null
    }

    Write-Host "開始下載： $Url"
    Write-Host "存檔至：     $OutputPath`n"

    try {
        $client = [System.Net.Http.HttpClient]::new()
        $client.DefaultRequestHeaders.UserAgent.ParseAdd(
            'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 ' +
            '(KHTML, like Gecko) Chrome/119.0.0.0 Safari/537.36'
        )

        $request  = [System.Net.Http.HttpRequestMessage]::new([System.Net.Http.HttpMethod]::Get, $Url)
        $response = $client.SendAsync(
            $request,
            [System.Net.Http.HttpCompletionOption]::ResponseHeadersRead
        ).GetAwaiter().GetResult()

        if (-not $response.IsSuccessStatusCode) {
            throw "HTTP 錯誤：狀態碼 $($response.StatusCode)"
        }

        $total    = $response.Content.Headers.ContentLength
        $stream   = $response.Content.ReadAsStreamAsync().GetAwaiter().GetResult()
        $file     = [System.IO.File]::OpenWrite($OutputPath)
        $buffer   = New-Object byte[] 81920
        $download = 0
        $lastPct  = -1

        while (($read = $stream.Read($buffer, 0, $buffer.Length)) -gt 0) {
            $file.Write($buffer, 0, $read)
            $download += $read

            if ($total) {
                # 計算當前百分比
                $pct = [int](($download / $total) * 100)

                # 只有當百分比變化才更新一次
                if ($pct -ne $lastPct) {
                    Write-Progress `
                        -Activity "下載中" `
                        -Status "$pct% 完成" `
                        -PercentComplete $pct
                    $lastPct = $pct
                }
            }
        }

        Write-Progress -Activity "下載中" -Completed
        Write-Host "`n✅ 下載完成： $OutputPath"
    }
    catch {
        Write-Error "❌ 下載失敗：$($_.Exception.Message)"
        throw
    }
    finally {
        if ($stream) { $stream.Close() }
        if ($file)   { $file.Close() }
        if ($client) { $client.Dispose() }
    }
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
