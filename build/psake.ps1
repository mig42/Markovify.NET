$projectName = $env:BHProjectName
$projectPath = $env:BHProjectPath

$paths = @{
  Root = $projectPath
  Output = Join-Path -Path $projectPath -ChildPath "artifacts"
  Library = Join-Path -Path $projectPath -ChildPath "src" -AdditionalChildPath $projectName
  Tests = Join-Path -Path $projectPath -ChildPath "tests" -AdditionalChildPath "$projectName.Tests"
  ModuleInfo = Join-Path -Path $projectPath -ChildPath "src" -AdditionalChildPath "$projectName.psd1"
}

function Get-CsprojName {
  param (
    
  )
  $dirName = (Get-Item $PWD).Name
  "$dirName.csproj"
}

$moduleInfo = Import-PowerShellDataFile $paths.ModuleInfo

Task default -depends Test

Task Test -description "Run the unit tests" {
  Exec -workingDirectory "$($paths.Tests)" -cmd {
    dotnet test "$(Get-CsprojName)"
  }
}

Task Build -description "Build the module artifacts" {
  $buildNumber = if ($env:BHBuildNumber) { $env:BHBuildNumber } else { $moduleInfo.ModuleVersion }

  Exec -workingDirectory "$($paths.Library)" -cmd {
    dotnet build "$(Get-CsprojName)" `
      -o $paths.Output `
      /p:Author=$moduleInfo.Author `
      /p:Version=$buildNumber
  }
}

Task Clean -description "Clean built artifacts" {
  Exec -workingDirectory $paths.Root -cmd {
    dotnet clean "$($paths.Root)/$projectName.sln" -o $paths.Output
  }
}

Task Publish -depends Clean, Build -description "Publish the module to the NuGet Gallery" {
  "Publish"
}