& "$PSScriptRoot/copy-files.ps1"

$root = "$PSScriptRoot/../.."

$info = Get-Content -Path "$root/Info.json" -Raw | ConvertFrom-Json
$disguise = Get-Content -Path "$root/Disguise.json" -Raw | ConvertFrom-Json
$disguise.AssemblyName = $info.AssemblyName
$disguise.EntryMethod = $info.EntryMethod
Set-Content -Path "$root/build/files/Info.json" -Value (ConvertTo-Json -InputObject $disguise)
