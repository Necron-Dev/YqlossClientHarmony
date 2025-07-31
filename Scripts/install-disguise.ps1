& "$PSScriptRoot/utils/disguise.ps1"

$root = "$PSScriptRoot/.."

$game = (Get-Content -Path "$root/GameFolder.txt").Trim()
$id = (Get-Content -Path "$root/Disguise.json" | ConvertFrom-Json).Id

Remove-Item -Path "$game/Mods/$id" -Recurse -Force -Exclude "Settings.xml"
New-Item -Path "$game/Mods/$id" -ItemType Directory
Copy-Item -Path "$root/build/files/*" -Destination "$game/Mods/$id" -Force -Recurse
