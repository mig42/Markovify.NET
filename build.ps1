#!/usr/bin/env powershell

#requires -Version 6.0

[cmdletbinding(DefaultParameterSetName = 'Task')]
param(
  # Build task(s) to execute
  [parameter(ParameterSetName = 'task', position = 0)]
  [string[]]$Task = 'default',

  # Bootstrap dependencies
  [switch]$Bootstrap,

  # List available build tasks
  [parameter(ParameterSetName = 'Help')]
  [switch]$Help,

  # Optional properties to pass to psake
  [hashtable]$Properties
)

$ErrorActionPreference = 'Stop'

if ($Bootstrap) {
  . "$PSScriptRoot/build/bootstrap.ps1"
}

$psakeFile = "$PSScriptRoot/build/psake.ps1"

if ($PSCmdlet.ParameterSetName -eq 'Help') {
  Get-PSakeScriptTasks -buildFile $psakeFile |
    Format-Table -Property Name, Description, Alias, DependsOn
} else {
  Set-BuildEnvironment -Force -BuildOutput "$PSScriptRoot/artifacts"

  Invoke-psake -buildFile $psakeFile -taskList $Task -nologo -properties $Properties
  exit ([int](-not $psake.build_success))
}
