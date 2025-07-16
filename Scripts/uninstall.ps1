$root = "$PSScriptRoot/.."

$game = (Get-Content -Path "$root/GameFolder.txt").Trim()

Remove-Item -Path "$game/Mods/YCH" -Recurse -Force
