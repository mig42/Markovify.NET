$root = "$PSScriptRoot/.."

Task default -depends Test

Task Test -description "Run the unit tests" {
  Exec -workingDirectory "$root/tests/Markovify.NET.Tests" -cmd {
    dotnet test Markovify.NET.Tests.csproj
  }
}

Task Build -description "Build the module artifacts" {
  Exec -workingDirectory "$root/src/Markovify.NET" -cmd {
    dotnet build Markovify.NET.csproj
  }
}

Task Clean -description "Clean built artifacts" {
  Exec -workingDirectory "$root/src/Markovify.NET" -cmd {
    dotnet build Markovify.NET.csproj
  }
}

task Publish -description "Publish the module to the NuGet Gallery" `
    -depends Clean, Build {
  "Publish"
}