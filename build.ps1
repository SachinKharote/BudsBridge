$projectPath = Join-Path $PSScriptRoot "src\\UniversalTWSSync.App\\UniversalTWSSync.App.csproj"
$msbuildPath = "C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\BuildTools\\MSBuild\\Current\\Bin\\MSBuild.exe"

if (-not (Test-Path $msbuildPath)) {
    throw "MSBuild was not found at '$msbuildPath'."
}

& $msbuildPath $projectPath /t:Build /p:Configuration=Debug /m

if ($LASTEXITCODE -ne 0) {
    throw "Build failed with exit code $LASTEXITCODE."
}
