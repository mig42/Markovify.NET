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
  [switch]$Help
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
  Set-BuildEnvironment -Force

  Invoke-psake -buildFile $psakeFile -taskList $Task -nologo
  exit ([int](-not $psake.build_success))
}
