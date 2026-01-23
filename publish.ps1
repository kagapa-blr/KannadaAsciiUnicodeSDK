$ErrorActionPreference = "Stop"

# ================================================================
# Configuration
# ================================================================

$ProjectPath   = "Kannada.AsciiUnicode/Kannada.AsciiUnicode.csproj"
$Configuration = "Release"
$PackageSource = "https://api.nuget.org/v3/index.json"
$PackageId     = "KannadaAsciiUnicodeSDK"

Write-Host "Starting NuGet publishing process"
Write-Host "Package ID: $PackageId"

if (-not (Test-Path $ProjectPath)) {
    throw "Project file not found: $ProjectPath"
}

# ================================================================
# Step 1: Pre-flight Build Validation
# ================================================================

Write-Host "================ Pre-flight Build Phase ================"

Write-Host "Running dotnet clean"
dotnet clean $ProjectPath -c $Configuration

Write-Host "Running dotnet restore"
dotnet restore $ProjectPath

Write-Host "Running dotnet build"
dotnet build $ProjectPath -c $Configuration --no-restore

Write-Host "Pre-flight build completed successfully"

# ================================================================
# Step 2: User Inputs
# ================================================================

Write-Host "================ User Inputs ================"

$ReleaseNotes = Read-Host "Enter Release Notes"
if ([string]::IsNullOrWhiteSpace($ReleaseNotes)) {
    throw "Release Notes cannot be empty"
}

$SecureApiKey = Read-Host "Enter NuGet API Key" -AsSecureString
$ApiKey = [Runtime.InteropServices.Marshal]::PtrToStringAuto(
    [Runtime.InteropServices.Marshal]::SecureStringToBSTR($SecureApiKey)
)

if ([string]::IsNullOrWhiteSpace($ApiKey)) {
    throw "NuGet API Key cannot be empty"
}

# ================================================================
# Step 3: Fetch Latest Version from NuGet
# ================================================================

Write-Host "================ Version Resolution ================"

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

# ================================================================
# Step 4: Update Project File (csproj)
# ================================================================

Write-Host "================ Updating Project File ================"

[xml]$csproj = Get-Content $ProjectPath

$propertyGroup = $csproj.Project.PropertyGroup | Where-Object { $_.Version }
if (-not $propertyGroup) {
    throw "<Version> element not found in csproj"
}

# Update Version
$propertyGroup.Version = $newVersion

# Update Description
$propertyGroup.Description =
    "High-performance Kannada Nudi/Baraha ASCII to Unicode SDK. Developed and maintained by Kannada Ganaka Parishat (KAGAPA). $ReleaseNotes"

# Update PackageReleaseNotes using XPath (reliable)
$releaseNotesNode = $csproj.SelectSingleNode("//Project/PropertyGroup/PackageReleaseNotes")
if (-not $releaseNotesNode) {
    throw "<PackageReleaseNotes> element not found in csproj"
}
$releaseNotesNode.InnerText = $ReleaseNotes

# ================================================================
# Step 5: Confirmation
# ================================================================

Write-Host ""
Write-Host "================ Release Summary ================"
Write-Host "Package ID    : $PackageId"
Write-Host "Version       : $newVersion"
Write-Host "Release Notes : $ReleaseNotes"
Write-Host "API Key       : ************"
Write-Host "================================================="
Write-Host ""

$confirm = Read-Host "Proceed with publishing? (Y/N)"
if ($confirm -notin @("Y", "y")) {
    Write-Host "Publishing cancelled"
    exit
}

# ================================================================
# Step 6: Save Project File
# ================================================================

Write-Host "================ Saving Changes ================"

$csproj.Save($ProjectPath)
Write-Host "Project file updated successfully"

# ================================================================
# Step 7: Pack NuGet Package
# ================================================================

Write-Host "================ Packing NuGet Package ================"

dotnet pack $ProjectPath -c $Configuration --no-build

$packagePath = "Kannada.AsciiUnicode/bin/$Configuration/$PackageId.$newVersion.nupkg"
if (-not (Test-Path $packagePath)) {
    throw "NuGet package not found: $packagePath"
}

# ================================================================
# Step 8: Push to NuGet
# ================================================================

Write-Host "================ Publishing to NuGet.org ================"

dotnet nuget push $packagePath `
    --api-key $ApiKey `
    --source $PackageSource `
    --skip-duplicate

Write-Host "Publish completed successfully"
Write-Host "Published version: $newVersion"
