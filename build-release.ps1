# Build script for Replace Stuff (Continued)
# Run this from the project root directory

$ModName = "Replace_Stuff_Continued"
$ReleaseDir = "Release/$ModName"

Write-Host "Building Replace Stuff (Continued) for release..." -ForegroundColor Green

# Clean previous release
if (Test-Path "Release") {
    Remove-Item "Release" -Recurse -Force
}
if (Test-Path "$ModName.zip") {
    Remove-Item "$ModName.zip" -Force
}

# Build the project
Write-Host "Building project..." -ForegroundColor Yellow
dotnet build "Source/Replace_Stuff_Continued.csproj" --configuration Release --no-restore

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}

# Create directory structure
Write-Host "Creating mod folder structure..." -ForegroundColor Yellow
New-Item -ItemType Directory -Force -Path "$ReleaseDir/About" | Out-Null
New-Item -ItemType Directory -Force -Path "$ReleaseDir/1.6/Assemblies" | Out-Null
New-Item -ItemType Directory -Force -Path "$ReleaseDir/1.6/Textures" | Out-Null
New-Item -ItemType Directory -Force -Path "$ReleaseDir/Defs" | Out-Null
New-Item -ItemType Directory -Force -Path "$ReleaseDir/Languages" | Out-Null
New-Item -ItemType Directory -Force -Path "$ReleaseDir/Patches" | Out-Null
New-Item -ItemType Directory -Force -Path "$ReleaseDir/Textures" | Out-Null
New-Item -ItemType Directory -Force -Path "$ReleaseDir/News" | Out-Null

# Copy files
Write-Host "Copying mod files..." -ForegroundColor Yellow

# About folder
Copy-Item "About/*" "$ReleaseDir/About/" -Recurse -Force

# Assembly (from build output)
$dllPath = "Source/bin/Release/Replace_Stuff_Continued.dll"
if (Test-Path $dllPath) {
    Copy-Item $dllPath "$ReleaseDir/1.6/Assemblies/"
    Write-Host "✓ Copied DLL from $dllPath" -ForegroundColor Green
} else {
    Write-Host "✗ DLL not found at $dllPath" -ForegroundColor Red
    Write-Host "Available files in Source/bin/Release/:" -ForegroundColor Yellow
    Get-ChildItem "Source/bin/Release/" -ErrorAction SilentlyContinue
    exit 1
}

# Icon to version folder
Copy-Item "Textures/TDReplaceStuffIcon.png" "$ReleaseDir/1.6/Textures/" -ErrorAction SilentlyContinue

# Other folders (if they exist)
if (Test-Path "Defs") { Copy-Item "Defs/*" "$ReleaseDir/Defs/" -Recurse -Force -ErrorAction SilentlyContinue }
if (Test-Path "Languages") { Copy-Item "Languages/*" "$ReleaseDir/Languages/" -Recurse -Force -ErrorAction SilentlyContinue }
if (Test-Path "Patches") { Copy-Item "Patches/*" "$ReleaseDir/Patches/" -Recurse -Force -ErrorAction SilentlyContinue }
if (Test-Path "News") { Copy-Item "News/*" "$ReleaseDir/News/" -Recurse -Force -ErrorAction SilentlyContinue }

# Textures (excluding icon since it goes in version folder)
if (Test-Path "Textures") { 
    Get-ChildItem "Textures/*" -Exclude "TDReplaceStuffIcon.png" | Copy-Item -Destination "$ReleaseDir/Textures/" -Recurse -Force -ErrorAction SilentlyContinue 
}

# LoadFolders.xml if it exists
if (Test-Path "LoadFolders.xml") { Copy-Item "LoadFolders.xml" "$ReleaseDir/" -ErrorAction SilentlyContinue }

# Create ZIP
Write-Host "Creating ZIP file..." -ForegroundColor Yellow
Compress-Archive -Path "Release/$ModName/*" -DestinationPath "$ModName.zip" -Force

Write-Host "" -ForegroundColor Green
Write-Host "✓ Release build complete!" -ForegroundColor Green
Write-Host "✓ Mod folder: Release/$ModName" -ForegroundColor Green
Write-Host "✓ ZIP file: $ModName.zip" -ForegroundColor Green
Write-Host "" -ForegroundColor Green
Write-Host "You can now:" -ForegroundColor White
Write-Host "1. Copy 'Release/$ModName' to your RimWorld Mods folder" -ForegroundColor White
Write-Host "2. Upload '$ModName.zip' to Steam Workshop or distribute it" -ForegroundColor White