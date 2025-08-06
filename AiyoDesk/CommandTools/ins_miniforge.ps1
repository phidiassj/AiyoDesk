param(
    [Parameter(Mandatory = $true)][string]$CondaPath
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
    Write-Host "建立 Python 3.12 環境..."
    & $condaExe create -n aiyodesk python=3.12 -y
}

# Main
Install-Conda
Create-Conda-Env
