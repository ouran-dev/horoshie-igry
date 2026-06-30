# Офлайн-установщик: без Velopack и без автообновления.
param(
    [string]$SourceDir = $PSScriptRoot
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

function Show-Error([string]$Message) {
    [System.Windows.Forms.MessageBox]::Show($Message, 'Хорошие игры', 'OK', 'Error') | Out-Null
}

$exeName = 'HoroshieIgry.exe'
$sourceExe = Join-Path $SourceDir $exeName
if (-not (Test-Path $sourceExe)) {
    Show-Error "Не найден $exeName рядом с установщиком.`nПапка: $SourceDir"
    exit 1
}

$iconPath = Join-Path $SourceDir 'Assets\Brand\AppIcon.ico'
$logoPath = Join-Path $SourceDir 'Assets\Brand\logo.png'

$form = New-Object System.Windows.Forms.Form
$form.Text = 'Установка — Хорошие игры (офлайн)'
$form.Width = 520
$form.Height = 420
$form.FormBorderStyle = 'FixedDialog'
$form.MaximizeBox = $false
$form.StartPosition = 'CenterScreen'

if (Test-Path $logoPath) {
    $picture = New-Object System.Windows.Forms.PictureBox
    $picture.Image = [System.Drawing.Image]::FromFile($logoPath)
    $picture.SizeMode = 'Zoom'
    $picture.SetBounds(200, 12, 96, 96)
    $form.Controls.Add($picture)
}

$title = New-Object System.Windows.Forms.Label
$title.Text = 'Хорошие игры — офлайн-версия'
$title.Font = New-Object System.Drawing.Font('Segoe UI', 12, [System.Drawing.FontStyle]::Bold)
$title.AutoSize = $true
$title.SetBounds(110, 112, 300, 28)
$form.Controls.Add($title)

$hint = New-Object System.Windows.Forms.Label
$hint.Text = 'Автообновление отключено. Все файлы будут скопированы в выбранную папку.'
$hint.AutoSize = $false
$hint.SetBounds(24, 142, 460, 36)
$form.Controls.Add($hint)

$installLabel = New-Object System.Windows.Forms.Label
$installLabel.Text = 'Папка установки:'
$installLabel.AutoSize = $true
$installLabel.SetBounds(24, 188, 200, 20)
$form.Controls.Add($installLabel)

$installBox = New-Object System.Windows.Forms.TextBox
$installBox.SetBounds(24, 210, 360, 24)
$installBox.Text = Join-Path $env:LOCALAPPDATA 'Хорошие игры (офлайн)'
$form.Controls.Add($installBox)

$installBrowse = New-Object System.Windows.Forms.Button
$installBrowse.Text = 'Обзор…'
$installBrowse.SetBounds(392, 208, 88, 28)
$installBrowse.Add_Click({
    $dialog = New-Object System.Windows.Forms.FolderBrowserDialog
    $dialog.Description = 'Куда установить игру?'
    $dialog.SelectedPath = $installBox.Text
    if ($dialog.ShowDialog() -eq 'OK') { $installBox.Text = $dialog.SelectedPath }
})
$form.Controls.Add($installBrowse)

$shortcutCheck = New-Object System.Windows.Forms.CheckBox
$shortcutCheck.Text = 'Создать ярлык'
$shortcutCheck.Checked = $true
$shortcutCheck.AutoSize = $true
$shortcutCheck.SetBounds(24, 248, 200, 24)
$form.Controls.Add($shortcutCheck)

$shortcutLabel = New-Object System.Windows.Forms.Label
$shortcutLabel.Text = 'Папка для ярлыка:'
$shortcutLabel.AutoSize = $true
$shortcutLabel.SetBounds(24, 276, 200, 20)
$form.Controls.Add($shortcutLabel)

$shortcutBox = New-Object System.Windows.Forms.TextBox
$shortcutBox.SetBounds(24, 298, 360, 24)
$shortcutBox.Text = [Environment]::GetFolderPath('Desktop')
$form.Controls.Add($shortcutBox)

$shortcutBrowse = New-Object System.Windows.Forms.Button
$shortcutBrowse.Text = 'Обзор…'
$shortcutBrowse.SetBounds(392, 296, 88, 28)
$shortcutBrowse.Add_Click({
    $dialog = New-Object System.Windows.Forms.FolderBrowserDialog
    $dialog.Description = 'Куда положить ярлык «Хорошие игры»?'
    $dialog.SelectedPath = $shortcutBox.Text
    if ($dialog.ShowDialog() -eq 'OK') { $shortcutBox.Text = $dialog.SelectedPath }
})
$form.Controls.Add($shortcutBrowse)

$installButton = New-Object System.Windows.Forms.Button
$installButton.Text = 'Установить'
$installButton.SetBounds(300, 340, 180, 32)
$installButton.Add_Click({
    try {
        $target = $installBox.Text.Trim()
        if ([string]::IsNullOrWhiteSpace($target)) {
            Show-Error 'Укажите папку установки.'
            return
        }

        New-Item -ItemType Directory -Path $target -Force | Out-Null

        $exclude = @('Установить-офлайн.bat', 'Установить-офлайн.ps1', 'Install-Offline.ps1')
        Get-ChildItem -Path $SourceDir -Force | Where-Object {
            $exclude -notcontains $_.Name
        } | ForEach-Object {
            $dest = Join-Path $target $_.Name
            if ($_.PSIsContainer) {
                Copy-Item $_.FullName $dest -Recurse -Force
            } else {
                Copy-Item $_.FullName $dest -Force
            }
        }

        $marker = Join-Path $target '.shortcut-setup-done'
        Set-Content -Path $marker -Value (Get-Date).ToString('o') -Encoding UTF8

        if ($shortcutCheck.Checked) {
            $shortcutDir = $shortcutBox.Text.Trim()
            if ([string]::IsNullOrWhiteSpace($shortcutDir)) {
                Show-Error 'Укажите папку для ярлыка.'
                return
            }

            New-Item -ItemType Directory -Path $shortcutDir -Force | Out-Null
            $linkPath = Join-Path $shortcutDir 'Хорошие игры.lnk'
            $exePath = Join-Path $target $exeName
            $icon = if (Test-Path (Join-Path $target 'Assets\Brand\AppIcon.ico')) {
                Join-Path $target 'Assets\Brand\AppIcon.ico'
            } else { $exePath }

            $shell = New-Object -ComObject WScript.Shell
            $shortcut = $shell.CreateShortcut($linkPath)
            $shortcut.TargetPath = $exePath
            $shortcut.WorkingDirectory = $target
            $shortcut.IconLocation = $icon
            $shortcut.Description = 'Хорошие игры'
            $shortcut.Save()
        }

        [System.Windows.Forms.MessageBox]::Show(
            "Игра установлена в:`n$target`n`nМожно запускать «Хорошие игры».",
            'Готово',
            'OK',
            'Information') | Out-Null
        $form.DialogResult = 'OK'
        $form.Close()
    }
    catch {
        Show-Error $_.Exception.Message
    }
})
$form.Controls.Add($installButton)

[void]$form.ShowDialog()
