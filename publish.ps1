$ErrorActionPreference = "Stop"

$ProjectPath   = "Kannada.AsciiUnicode/Kannada.AsciiUnicode.csproj"
$Configuration = "Release"
$PackageSource = "https://api.nuget.org/v3/index.json"
$PackageId     = "KannadaAsciiUnicodeSDK"

Write-Host "Starting NuGet publishing process"
Write-Host "Package ID: $PackageId"

if (-not (Test-Path $ProjectPath)) {
    throw "Project file not found"
}

# Prompt for release notes
$ReleaseNotes = Read-Host "Enter Release Notes"
if ([string]::IsNullOrWhiteSpace($ReleaseNotes)) {
    throw "Release Notes cannot be empty"
}

# Prompt for API key securely
$SecureApiKey = Read-Host "Enter NuGet API Key" -AsSecureString
$ApiKey = [Runtime.InteropServices.Marshal]::PtrToStringAuto(
    [Runtime.InteropServices.Marshal]::SecureStringToBSTR($SecureApiKey)
)

if ([string]::IsNullOrWhiteSpace($ApiKey)) {
    throw "NuGet API Key cannot be empty"
}

# Fetch latest version from NuGet
Write-Host "Fetching latest version from NuGet.org"

$indexUrl = "https://api.nuget.org/v3-flatcontainer/$($PackageId.ToLower())/index.json"

try {
    $response = Invoke-RestMethod -Uri $indexUrl -Method Get -TimeoutSec 15
    $versions = $response.versions
}
catch {
    Write-Host "Package not found on NuGet. Assuming first release."
    $versions = @()
}

if ($versions.Count -eq 0) {
    $newVersion = "1.0.0"
    Write-Host "No existing versions found. Using version $newVersion"
}
else {
    $latestVersion = ($versions | Sort-Object {
        [version]($_ -replace '-.*$', '')
    } | Select-Object -Last 1)

    Write-Host "Latest published version: $latestVersion"

    $ver = [version]($latestVersion -replace '-.*$', '')
    $newVersion = "$($ver.Major).$($ver.Minor).$($ver.Build + 1)"

    Write-Host "Next version will be: $newVersion"
}

# Load and update csproj
Write-Host "Updating project file"

[xml]$csproj = Get-Content $ProjectPath
$propertyGroup = $csproj.Project.PropertyGroup | Where-Object { $_.Version }

if (-not $propertyGroup) {
    throw "<Version> element not found in csproj"
}

$propertyGroup.Version = $newVersion
$propertyGroup.Description =
    "High-performance Kannada Nudi/Baraha ASCII to Unicode SDK. Developed and maintained by Kannada Ganaka Parishat (KAGAPA). $ReleaseNotes"
$propertyGroup.PackageReleaseNotes = $ReleaseNotes

# Summary
Write-Host ""
Write-Host "Release Summary"
Write-Host "--------------------------------"
Write-Host "Package ID    : $PackageId"
Write-Host "Version       : $newVersion"
Write-Host "Release Notes : $ReleaseNotes"
Write-Host "API Key       : ************"
Write-Host "--------------------------------"
Write-Host ""

$confirm = Read-Host "Proceed with publishing? (Y/N)"
if ($confirm -notin @("Y", "y")) {
    Write-Host "Publishing cancelled"
    exit
}

# Save csproj
Write-Host "Saving project file"
$csproj.Save($ProjectPath)

# Clean, restore, build, pack
Write-Host "Running dotnet clean"
dotnet clean $ProjectPath -c $Configuration

Write-Host "Running dotnet restore"
dotnet restore $ProjectPath

Write-Host "Running dotnet build"
dotnet build $ProjectPath -c $Configuration --no-restore

Write-Host "Packing NuGet package"
dotnet pack $ProjectPath -c $Configuration --no-build

$packagePath = "Kannada.AsciiUnicode/bin/$Configuration/$PackageId.$newVersion.nupkg"

if (-not (Test-Path $packagePath)) {
    throw "NuGet package not found: $packagePath"
}

# Push
Write-Host "Pushing package to NuGet.org"
dotnet nuget push $packagePath `
    --api-key $ApiKey `
    --source $PackageSource `
    --skip-duplicate

Write-Host "Publish completed successfully"
Write-Host "Published version: $newVersion"
