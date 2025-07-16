& "$PSScriptRoot/utils/disguise.ps1"

$root = "$PSScriptRoot/.."

Compress-Archive -Path "$root/build/files/*" -DestinationPath "$root/build/YCH-Disguise.zip"
