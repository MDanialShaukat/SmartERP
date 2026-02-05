#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Creates a SmartERP application icon
.DESCRIPTION
    Generates a professional icon file for SmartERP application
    This script creates a basic icon file that can be used for the application
#>

param(
    [string]$IconPath = ".\SmartERP.UI\Assets\SmartERP.ico"
)

Write-Host "SmartERP Icon Generator" -ForegroundColor Cyan
Write-Host "========================" -ForegroundColor Cyan
Write-Host ""

# Function to create a simple icon from an image file
function Create-IconFromImage {
    param(
        [string]$ImagePath,
        [string]$IconPath
    )
    
    # Check if image exists
    if (-not (Test-Path $ImagePath)) {
        Write-Host "? Image file not found: $ImagePath" -ForegroundColor Red
        return $false
    }
    
    # Ensure output directory exists
    $IconDir = Split-Path -Parent $IconPath
    if (-not (Test-Path $IconDir)) {
        New-Item -ItemType Directory -Force -Path $IconDir | Out-Null
        Write-Host "? Created directory: $IconDir" -ForegroundColor Green
    }
    
    try {
        # Load image
        [System.Reflection.Assembly]::LoadWithPartialName("System.Drawing") | Out-Null
        
        $image = [System.Drawing.Image]::FromFile((Get-Item $ImagePath).FullName)
        
        # Create icon from image
        $icon = [System.Drawing.Icon]::FromHandle($image.GetHicon())
        
        # Save as ICO file
        $fileStream = [System.IO.File]::Create($IconPath)
        $icon.Save($fileStream)
        $fileStream.Close()
        
        Write-Host "? Icon created successfully: $IconPath" -ForegroundColor Green
        return $true
    }
    catch {
        Write-Host "? Error creating icon: $_" -ForegroundColor Red
        return $false
    }
}

# Check if an image file is available
$imagePaths = @(
    ".\SmartERP.UI\Assets\SmartERP.png",
    ".\SmartERP.UI\Assets\SmartERP.jpg",
    ".\SmartERP.UI\Assets\icon.png",
    ".\SmartERP.UI\Assets\icon.jpg"
)

Write-Host "Looking for image files to convert to icon..." -ForegroundColor Yellow
Write-Host ""

$imageFound = $false
foreach ($imagePath in $imagePaths) {
    if (Test-Path $imagePath) {
        Write-Host "? Found image: $imagePath" -ForegroundColor Green
        Write-Host "Converting to icon..." -ForegroundColor Yellow
        
        if (Create-IconFromImage -ImagePath $imagePath -IconPath $IconPath) {
            $imageFound = $true
            Write-Host ""
            Write-Host "Icon creation successful!" -ForegroundColor Green
            break
        }
    }
}

if (-not $imageFound) {
    Write-Host ""
    Write-Host "??  No image file found for icon creation" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Steps to create an icon:" -ForegroundColor Yellow
    Write-Host "1. Create or obtain a PNG image (256x256 pixels recommended)" -ForegroundColor Gray
    Write-Host "2. Save it as SmartERP.UI\Assets\SmartERP.png" -ForegroundColor Gray
    Write-Host "3. Run this script again" -ForegroundColor Gray
    Write-Host ""
    Write-Host "Or manually convert an image to .ico format:" -ForegroundColor Yellow
    Write-Host "- Use online tools: https://convertio.co/png-ico/" -ForegroundColor Gray
    Write-Host "- Use ImageMagick: convert image.png icon.ico" -ForegroundColor Gray
    Write-Host "- Use GIMP or Photoshop" -ForegroundColor Gray
    Write-Host ""
    Write-Host "Alternative: Use a default Windows icon" -ForegroundColor Yellow
    Write-Host "  Copy: C:\Windows\System32\imageres.dll (icon #67)" -ForegroundColor Gray
    
    # Create a basic white icon as fallback
    Write-Host ""
    Write-Host "Creating a basic fallback icon..." -ForegroundColor Yellow
    
    try {
        [System.Reflection.Assembly]::LoadWithPartialName("System.Drawing") | Out-Null
        
        $IconDir = Split-Path -Parent $IconPath
        if (-not (Test-Path $IconDir)) {
            New-Item -ItemType Directory -Force -Path $IconDir | Out-Null
        }
        
        # Create a 256x256 bitmap with a blue gradient
        $bitmap = New-Object System.Drawing.Bitmap(256, 256)
        $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
        $graphics.Clear([System.Drawing.Color]::White)
        
        # Draw a simple design
        $brush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(65, 105, 225))
        $graphics.FillEllipse($brush, 50, 50, 156, 156)
        
        # Add text
        $font = New-Object System.Drawing.Font("Arial", 80, [System.Drawing.FontStyle]::Bold)
        $textBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::White)
        $graphics.DrawString("S", $font, $textBrush, 85, 70)
        
        $graphics.Dispose()
        
        # Convert to icon
        $icon = [System.Drawing.Icon]::FromHandle($bitmap.GetHicon())
        $fileStream = [System.IO.File]::Create($IconPath)
        $icon.Save($fileStream)
        $fileStream.Close()
        
        Write-Host "? Fallback icon created: $IconPath" -ForegroundColor Green
        Write-Host ""
        Write-Host "Note: This is a basic placeholder icon. Replace it with your custom icon." -ForegroundColor Yellow
    }
    catch {
        Write-Host "? Could not create fallback icon: $_" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "Icon path: $IconPath" -ForegroundColor Cyan
Write-Host ""
