# Cleanup script to remove bin, obj, and Docs folders

# Get the workspace root (two levels up from .vscode/scripts)
$WorkspaceRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)

Write-Host "Starting cleanup of bin, obj, and Docs folders..." -ForegroundColor Yellow
Write-Host "Workspace: $WorkspaceRoot" -ForegroundColor Cyan

# Folders to remove
$FoldersToRemove = @("bin", "obj", "Docs")
$TotalRemoved = 0

function Test-IsRootDocsFolder {
    param (
        [Parameter(Mandatory = $true)]
        [System.IO.DirectoryInfo]$Directory,

        [Parameter(Mandatory = $true)]
        [string]$RootPath
    )

    return $Directory.Name -ieq "docs" -and $Directory.Parent.FullName -ieq $RootPath
}

foreach ($FolderName in $FoldersToRemove) {
    Write-Host "Searching for $FolderName folders..." -ForegroundColor White

    $FoundFolders = Get-ChildItem -Path $WorkspaceRoot -Recurse -Directory | Where-Object { $_.Name -eq $FolderName }

    if ($FoundFolders.Count -gt 0) {
        Write-Host "Found $($FoundFolders.Count) $FolderName folder(s)" -ForegroundColor Green

        foreach ($Dir in $FoundFolders) {
            try {
                if (Test-IsRootDocsFolder -Directory $Dir -RootPath $WorkspaceRoot) {
                    Write-Host "Skipping root docs folder: $($Dir.FullName)" -ForegroundColor DarkYellow
                    continue
                }

                Write-Host "Removing: $($Dir.FullName)" -ForegroundColor Red
                Remove-Item -Path $Dir.FullName -Recurse -Force -ErrorAction Stop
                $TotalRemoved++
            } catch {
                Write-Host "Failed to remove: $($Dir.FullName)" -ForegroundColor DarkRed
                Write-Host "Error: $($_.Exception.Message)" -ForegroundColor DarkRed
            }
        }
    } else {
        Write-Host "No $FolderName folders found" -ForegroundColor Gray
    }
}

Write-Host "Cleanup completed!" -ForegroundColor Green
Write-Host "Total folders removed: $TotalRemoved" -ForegroundColor Cyan

if ($TotalRemoved -gt 0) {
    Write-Host "Tip: Run dotnet restore to restore packages if needed" -ForegroundColor Yellow
}
