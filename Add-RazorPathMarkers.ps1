<#
.SYNOPSIS
    Добавляет в *.cshtml скрытый HTML-тег с путём к представлению от корня решения.
.DESCRIPTION
    Обходит все файлы *.cshtml в решении и вставляет после Razor-преамбулы (@using, @model, @{ ... }, @helper ...)
    скрытый тег <span> или <div> с атрибутом name="razorPath". Текст тега — относительный путь к файлу
    от корня решения (слэши /).

    Повторный запуск пропускает файлы, в которых маркер уже есть.
.PARAMETER SolutionRoot
    Корень решения. По умолчанию — каталог, где лежит этот скрипт.
.PARAMETER TagName
    Имя HTML-тега: span или div. По умолчанию span.
.PARAMETER ExcludePattern
    Regex для исключения путей (bin, obj, packages и т.д.).
.PARAMETER DryRun
    Только показать, какие файлы будут изменены, без записи на диск.
.EXAMPLE
    .\Add-RazorPathMarkers.ps1
    Добавить маркеры во все *.cshtml решения.
.EXAMPLE
    .\Add-RazorPathMarkers.ps1 -DryRun
    Просмотр изменений без записи.
.EXAMPLE
    .\Add-RazorPathMarkers.ps1 -TagName div
    Использовать <div hidden name="razorPath"> вместо <span>.
#>

param(
    [string] $SolutionRoot = (Split-Path -Parent $PSCommandPath),
    [ValidateSet('span', 'div')]
    [string] $TagName = 'span',
    [string] $ExcludePattern = '\\(bin|obj|packages|node_modules|\.git|_decompiled)\\',
    [switch] $DryRun
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Test-HasRazorPathMarker {
    param([string] $Content)

    return [regex]::IsMatch($Content, 'name\s*=\s*[''"]razorPath[''"]', [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
}

function Get-RazorPreambleEndIndex {
    param([string[]] $Lines)

    $index = 0
    $count = $Lines.Count

    while ($index -lt $count) {
        $line = $Lines[$index]

        if ($line -match '^\s*$') {
            $index++
            continue
        }

        if ($line -match '^\s*@(using|model|inherits|inject|page|layout|functions)\b') {
            $index++
            continue
        }

        if ($line -match '^\s*@(\{|helper\b)') {
            $depth = 0
            $started = $false

            while ($index -lt $count) {
                $blockLine = $Lines[$index]

                foreach ($ch in $blockLine.ToCharArray()) {
                    if ($ch -eq '{') {
                        $depth++
                        $started = $true
                    }
                    elseif ($ch -eq '}') {
                        $depth--
                    }
                }

                $index++

                if ($started -and $depth -le 0) {
                    break
                }
            }

            continue
        }

        break
    }

    return $index
}

function New-RazorPathMarker {
    param(
        [string] $RelativePath,
        [string] $HtmlTag
    )

    $escapedPath = [System.Net.WebUtility]::HtmlEncode($RelativePath)
    return "<$HtmlTag hidden name=""razorPath"">$escapedPath</$HtmlTag>"
}

function Get-FileEncoding {
    param([byte[]] $Bytes)

    if ($Bytes.Length -ge 3 -and $Bytes[0] -eq 0xEF -and $Bytes[1] -eq 0xBB -and $Bytes[2] -eq 0xBF) {
        return [System.Text.UTF8Encoding]::new($true)
    }

    if ($Bytes.Length -ge 2 -and $Bytes[0] -eq 0xFF -and $Bytes[1] -eq 0xFE) {
        return [System.Text.UnicodeEncoding]::new($false, $true)
    }

    if ($Bytes.Length -ge 2 -and $Bytes[0] -eq 0xFE -and $Bytes[1] -eq 0xFF) {
        return [System.Text.UnicodeEncoding]::new($true, $true)
    }

    return [System.Text.UTF8Encoding]::new($false)
}

function Read-TextFile {
    param([string] $Path)

    $bytes = [System.IO.File]::ReadAllBytes($Path)
    $encoding = Get-FileEncoding -Bytes $bytes
    $text = $encoding.GetString($bytes)

    # Убрать BOM из строки, если он попал в декодированный текст
    if ($text.Length -gt 0 -and [int][char]$text[0] -eq 0xFEFF) {
        $text = $text.Substring(1)
    }

    return [pscustomobject]@{
        Text = $text
        Encoding = $encoding
    }
}

function Write-TextFile {
    param(
        [string] $Path,
        [string] $Text,
        [System.Text.Encoding] $Encoding
    )

    [System.IO.File]::WriteAllText($Path, $Text, $Encoding)
}

function Add-RazorPathMarkerToFile {
    param(
        [string] $FilePath,
        [string] $RootPath,
        [string] $HtmlTag,
        [switch] $DryRun
    )

    $fileInfo = Read-TextFile -Path $FilePath
    $originalText = $fileInfo.Text

    if (Test-HasRazorPathMarker -Content $originalText) {
        return 'skipped'
    }

    $normalizedRoot = [System.IO.Path]::GetFullPath($RootPath).TrimEnd('\', '/')
    $normalizedFile = [System.IO.Path]::GetFullPath($FilePath)
    $relativePath = $normalizedFile.Substring($normalizedRoot.Length).TrimStart('\', '/')
    $relativePath = $relativePath -replace '\\', '/'

    $normalizedNewlines = $originalText -replace "`r`n", "`n" -replace "`r", "`n"
    $hadTrailingNewline = $normalizedNewlines.EndsWith("`n")
    $lines = $normalizedNewlines -split "`n", -1

    if ($lines.Count -gt 0 -and $lines[-1] -eq '' -and $hadTrailingNewline) {
        $lines = $lines[0..($lines.Count - 2)]
    }

    $insertIndex = Get-RazorPreambleEndIndex -Lines $lines
    $marker = New-RazorPathMarker -RelativePath $relativePath -HtmlTag $HtmlTag

    $newLines = New-Object System.Collections.Generic.List[string]
    for ($i = 0; $i -lt $insertIndex; $i++) {
        [void]$newLines.Add($lines[$i])
    }

    if ($insertIndex -gt 0 -and $lines[$insertIndex - 1] -notmatch '^\s*$') {
        [void]$newLines.Add('')
    }

    [void]$newLines.Add($marker)

    if ($insertIndex -lt $lines.Count -and $lines[$insertIndex] -notmatch '^\s*$') {
        [void]$newLines.Add('')
    }

    for ($i = $insertIndex; $i -lt $lines.Count; $i++) {
        [void]$newLines.Add($lines[$i])
    }

    $newText = ($newLines -join "`r`n")
    if ($hadTrailingNewline -and -not $newText.EndsWith("`r`n")) {
        $newText += "`r`n"
    }

    if ($DryRun) {
        Write-Host "[DryRun] $relativePath" -ForegroundColor Cyan
        return 'would-update'
    }

    Write-TextFile -Path $FilePath -Text $newText -Encoding $fileInfo.Encoding
    Write-Host "Updated: $relativePath" -ForegroundColor Green
    return 'updated'
}

$solutionRootFull = [System.IO.Path]::GetFullPath($SolutionRoot)
if (-not (Test-Path -LiteralPath $solutionRootFull -PathType Container)) {
    throw "Solution root not found: $SolutionRoot"
}

Write-Host "Solution root: $solutionRootFull"
Write-Host "Tag: <$TagName hidden name=`"razorPath`">..."
Write-Host ""

$cshtmlFiles = @(Get-ChildItem -LiteralPath $solutionRootFull -Filter '*.cshtml' -Recurse -File -ErrorAction SilentlyContinue |
    Where-Object { $_.FullName -notmatch $ExcludePattern })

$stats = @{
    Updated = 0
    Skipped = 0
    WouldUpdate = 0
}

foreach ($file in $cshtmlFiles) {
    $result = Add-RazorPathMarkerToFile -FilePath $file.FullName -RootPath $solutionRootFull -HtmlTag $TagName -DryRun:$DryRun

    switch ($result) {
        'updated' { $stats.Updated++ }
        'skipped' { $stats.Skipped++ }
        'would-update' { $stats.WouldUpdate++ }
    }
}

Write-Host ""
Write-Host "Total *.cshtml files: $($cshtmlFiles.Count)"
if ($DryRun) {
    Write-Host "Would update: $($stats.WouldUpdate)"
}
else {
    Write-Host "Updated: $($stats.Updated)"
}
Write-Host "Skipped (already marked): $($stats.Skipped)"
