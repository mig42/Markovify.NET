if (-not (Get-Command dotnet)) {
  Write-Output `
    "The .NET Core SDK wasn't found. Please install it from https://dotnet.microsoft.com/download"
}

Get-PackageProvider -Name Nuget -ForceBootstrap > $null
Set-PSRepository -Name PSGallery -InstallationPolicy Trusted

if (-not (Get-Module -Name PSDepend -ListAvailable)) {
    Install-Module -Name PSDepend -Repository PSGallery -Scope CurrentUser -Force
}

Import-Module -Name PSDepend -Verbose:$false
Invoke-PSDepend -Path "$PSScriptRoot/ps-requirements.psd1" -Install -Import -Force -WarningAction SilentlyContinue