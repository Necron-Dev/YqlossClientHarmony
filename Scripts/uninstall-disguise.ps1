$root = "$PSScriptRoot/.."

$game = (Get-Content -Path "$root/GameFolder.txt").Trim()
$id = (Get-Content -Path "$root/Disguise.json" | ConvertFrom-Json).Id

Remove-Item -Path "$game/Mods/$id" -Recurse -Force
