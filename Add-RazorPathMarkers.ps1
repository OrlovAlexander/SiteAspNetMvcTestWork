<#
.SYNOPSIS
    Добавляет или удаляет в *.cshtml скрытый HTML-тег с путём к представлению от корня решения.
.DESCRIPTION
    Обходит все файлы *.cshtml в решении и вставляет после Razor-преамбулы (@using, @model, @{ ... }, @helper ...)
    скрытый тег <span> или <div> с атрибутом name="razorPath". Текст тега — относительный путь к файлу
    от корня решения (слэши /).

    Повторный запуск добавления пропускает файлы, в которых маркер уже есть.
    Режим -Remove удаляет ранее добавленные маркеры (span и div).
.PARAMETER SolutionRoot
    Корень решения. По умолчанию — каталог, где лежит этот скрипт.
.PARAMETER TagName
    Имя HTML-тега: span или div. По умолчанию span. Используется только при добавлении.
.PARAMETER ExcludePattern
    Regex для исключения путей (bin, obj, packages и т.д.).
.PARAMETER DryRun
    Только показать, какие файлы будут изменены, без записи на диск.
.PARAMETER Remove
    Удалить маркеры razorPath из *.cshtml вместо добавления.
.EXAMPLE
    .\Add-RazorPathMarkers.ps1
    Добавить маркеры во все *.cshtml решения.
.EXAMPLE
    .\Add-RazorPathMarkers.ps1 -DryRun
    Просмотр изменений без записи.
.EXAMPLE
    .\Add-RazorPathMarkers.ps1 -TagName div
    Использовать <div hidden name="razorPath"> вместо <span>.
.EXAMPLE
    .\Add-RazorPathMarkers.ps1 -Remove
    Удалить маркеры из всех *.cshtml.
.EXAMPLE
    .\Add-RazorPathMarkers.ps1 -Remove -DryRun
    Просмотр удаления без записи.
#>

param(
    [string] $SolutionRoot = (Split-Path -Parent $PSCommandPath),
    [ValidateSet('span', 'div')]
    [string] $TagName = 'span',
    [string] $ExcludePattern = '\\(bin|obj|packages|node_modules|\.git|_decompiled)\\',
    [switch] $DryRun,
    [switch] $Remove
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Test-HasRazorPathMarker {
    param([string] $Content)

    return [regex]::IsMatch($Content, 'name\s*=\s*[''"]razorPath[''"]', [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
}

function Get-RazorPathMarkerLinePattern {
    return '^\s*<(span|div)\s+hidden\s+name\s*=\s*[''"]razorPath[''"][^>]*>.*?</\1>\s*$'
}

function Get-NormalizedFileLines {
    param([string] $Text)

    $normalizedNewlines = $Text -replace "`r`n", "`n" -replace "`r", "`n"
    $hadTrailingNewline = $normalizedNewlines.EndsWith("`n")
    $lines = $normalizedNewlines -split "`n", -1

    if ($lines.Count -gt 0 -and $lines[-1] -eq '' -and $hadTrailingNewline) {
        $lines = $lines[0..($lines.Count - 2)]
    }

    return [pscustomobject]@{
        Lines = $lines
        HadTrailingNewline = $hadTrailingNewline
    }
}

function Join-FileLines {
    param(
        [string[]] $Lines,
        [bool] $HadTrailingNewline
    )

    $newText = ($Lines -join "`r`n")
    if ($HadTrailingNewline -and -not $newText.EndsWith("`r`n")) {
        $newText += "`r`n"
    }

    return $newText
}

function Get-RelativeCshtmlPath {
    param(
        [string] $FilePath,
        [string] $RootPath
    )

    $normalizedRoot = [System.IO.Path]::GetFullPath($RootPath).TrimEnd('\', '/')
    $normalizedFile = [System.IO.Path]::GetFullPath($FilePath)
    $relativePath = $normalizedFile.Substring($normalizedRoot.Length).TrimStart('\', '/')
    return ($relativePath -replace '\\', '/')
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

    $relativePath = Get-RelativeCshtmlPath -FilePath $FilePath -RootPath $RootPath

    $fileLines = Get-NormalizedFileLines -Text $originalText
    $lines = $fileLines.Lines
    $hadTrailingNewline = $fileLines.HadTrailingNewline

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

    $newText = Join-FileLines -Lines $newLines.ToArray() -HadTrailingNewline $hadTrailingNewline

    if ($DryRun) {
        Write-Host "[DryRun] $relativePath" -ForegroundColor Cyan
        return 'would-update'
    }

    Write-TextFile -Path $FilePath -Text $newText -Encoding $fileInfo.Encoding
    Write-Host "Updated: $relativePath" -ForegroundColor Green
    return 'updated'
}

function Remove-RazorPathMarkerFromFile {
    param(
        [string] $FilePath,
        [string] $RootPath,
        [switch] $DryRun
    )

    $fileInfo = Read-TextFile -Path $FilePath
    $originalText = $fileInfo.Text

    if (-not (Test-HasRazorPathMarker -Content $originalText)) {
        return 'skipped'
    }

    $relativePath = Get-RelativeCshtmlPath -FilePath $FilePath -RootPath $RootPath

    $fileLines = Get-NormalizedFileLines -Text $originalText
    $lines = $fileLines.Lines
    $hadTrailingNewline = $fileLines.HadTrailingNewline

    $markerLinePattern = Get-RazorPathMarkerLinePattern
    $markerIndex = -1

    for ($i = 0; $i -lt $lines.Count; $i++) {
        if ($lines[$i] -match $markerLinePattern) {
            $markerIndex = $i
            break
        }
    }

    if ($markerIndex -lt 0) {
        return 'skipped'
    }

    $removeIndices = [System.Collections.Generic.HashSet[int]]::new()
    [void]$removeIndices.Add($markerIndex)

    # Пустая строка после маркера добавлялась, если следующая строка не была пустой
    if ($markerIndex -lt ($lines.Count - 1) -and $lines[$markerIndex + 1] -match '^\s*$') {
        [void]$removeIndices.Add($markerIndex + 1)
    }
    # Иначе пустая строка перед маркером добавлялась, если предыдущая строка не была пустой
    elseif ($markerIndex -gt 0 -and $lines[$markerIndex - 1] -match '^\s*$') {
        [void]$removeIndices.Add($markerIndex - 1)
    }

    $newLines = New-Object System.Collections.Generic.List[string]
    for ($i = 0; $i -lt $lines.Count; $i++) {
        if (-not $removeIndices.Contains($i)) {
            [void]$newLines.Add($lines[$i])
        }
    }

    $newText = Join-FileLines -Lines $newLines.ToArray() -HadTrailingNewline $hadTrailingNewline

    if ($DryRun) {
        Write-Host "[DryRun] $relativePath" -ForegroundColor Cyan
        return 'would-remove'
    }

    Write-TextFile -Path $FilePath -Text $newText -Encoding $fileInfo.Encoding
    Write-Host "Removed: $relativePath" -ForegroundColor Yellow
    return 'removed'
}

$solutionRootFull = [System.IO.Path]::GetFullPath($SolutionRoot)
if (-not (Test-Path -LiteralPath $solutionRootFull -PathType Container)) {
    throw "Solution root not found: $SolutionRoot"
}

Write-Host "Solution root: $solutionRootFull"
if ($Remove) {
    Write-Host "Mode: remove razorPath markers"
}
else {
    Write-Host "Mode: add markers"
    Write-Host "Tag: <$TagName hidden name=`"razorPath`">..."
}
Write-Host ""

$cshtmlFiles = @(Get-ChildItem -LiteralPath $solutionRootFull -Filter '*.cshtml' -Recurse -File -ErrorAction SilentlyContinue |
    Where-Object { $_.FullName -notmatch $ExcludePattern })

$stats = @{
    Updated = 0
    Removed = 0
    Skipped = 0
    WouldUpdate = 0
    WouldRemove = 0
}

foreach ($file in $cshtmlFiles) {
    if ($Remove) {
        $result = Remove-RazorPathMarkerFromFile -FilePath $file.FullName -RootPath $solutionRootFull -DryRun:$DryRun
    }
    else {
        $result = Add-RazorPathMarkerToFile -FilePath $file.FullName -RootPath $solutionRootFull -HtmlTag $TagName -DryRun:$DryRun
    }

    switch ($result) {
        'updated' { $stats.Updated++ }
        'removed' { $stats.Removed++ }
        'skipped' { $stats.Skipped++ }
        'would-update' { $stats.WouldUpdate++ }
        'would-remove' { $stats.WouldRemove++ }
    }
}

Write-Host ""
Write-Host "Total *.cshtml files: $($cshtmlFiles.Count)"
if ($Remove) {
    if ($DryRun) {
        Write-Host "Would remove: $($stats.WouldRemove)"
    }
    else {
        Write-Host "Removed: $($stats.Removed)"
    }
    Write-Host "Skipped (no marker): $($stats.Skipped)"
}
else {
    if ($DryRun) {
        Write-Host "Would update: $($stats.WouldUpdate)"
    }
    else {
        Write-Host "Updated: $($stats.Updated)"
    }
    Write-Host "Skipped (already marked): $($stats.Skipped)"
}
