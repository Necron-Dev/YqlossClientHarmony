& "$PSScriptRoot/utils/copy-files.ps1"

$root = "$PSScriptRoot/.."

$game = (Get-Content -Path "$root/GameFolder.txt").Trim()

Remove-Item -Path "$game/Mods/YCH" -Recurse -Force -Exclude "Settings.xml"
New-Item -Path "$game/Mods/YCH" -ItemType Directory
Copy-Item -Path "$root/build/files/*" -Destination "$game/Mods/YCH" -Force -Recurse
