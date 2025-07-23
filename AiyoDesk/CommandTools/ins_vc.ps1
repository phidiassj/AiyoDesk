<#
.SYNOPSIS
    檢查並自動下載、安裝最新版 Microsoft Visual C++ 2022 x64 Redistributable

.DESCRIPTION
    1. 檢查註冊表「Installer\Dependencies」中是否已有 vc,redist.x64,amd64, 開頭的項目
       若有，表示已安裝過，腳本直接結束
    2. 若未安裝，使用 Invoke-WebRequest 下載 https://aka.ms/vs/17/release/vc_redist.x64.exe
    3. 下載完成後執行安裝：vc_redist.x64.exe /install /norestart
    4. 全程顯示進度與詳細訊息

.EXAMPLE
    PS> .\Install-VCRedist.ps1
#>

[CmdletBinding()]
param(
    [string]$DownloadUrl = 'https://aka.ms/vs/17/release/vc_redist.x64.exe',
    [string]$OutputPath  = "$env:TEMP\vc_redist.x64.exe"
)

function Test-VCRedistInstalled {
    <#
    .SYNOPSIS
        判斷 x64 版 VC++ Redistributable 是否已安裝
    .OUTPUTS
        [bool] 已安裝回傳 $true，否則 $false
    #>
    $depKey = 'HKLM:\SOFTWARE\Classes\Installer\Dependencies'
    if (-not (Test-Path $depKey)) {
        return $false
    }

    # 找出以 vc,redist.x64,amd64, 開頭的子金鑰
    $match = Get-ChildItem $depKey -ErrorAction SilentlyContinue |
             Where-Object { $_.PSChildName -match '^vc,redist\.x64,amd64,' }

    return ($match.Count -gt 0)
}

try {
    Write-Host "檢查是否已安裝過 x64 版 Visual C++ Redistributable..." -ForegroundColor Cyan
    if (Test-VCRedistInstalled) {
        Write-Host "系統已偵測到已安裝過 x64 VC++ Redistributable，腳本結束。" -ForegroundColor Yellow
        return
    }

    Write-Host "未偵測到安裝，開始下載安裝程式..." -ForegroundColor Cyan
    Invoke-WebRequest `
        -Uri $DownloadUrl `
        -OutFile $OutputPath `
        -UseBasicParsing `
        -Verbose

    Write-Host "`n下載完成，檔案位置： $OutputPath" -ForegroundColor Green

    Write-Host "`n開始執行安裝 (/install /norestart)..." -ForegroundColor Cyan
    $proc = Start-Process `
        -FilePath $OutputPath `
        -ArgumentList '/install','/norestart' `
        -NoNewWindow `
        -Wait `
        -PassThru

    if ($proc.ExitCode -eq 0) {
        Write-Host "`n安裝成功！" -ForegroundColor Green
    }
    else {
        Write-Host "`n安裝結束，ExitCode：$($proc.ExitCode)" -ForegroundColor Yellow
    }
}
catch {
    Write-Host "`n發生錯誤：" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit 1
}
