# Declare a param for the dotnet configuration name (typically Debug or Release)
# Run script with "-Configuration Debug" for Debug or anything else you want
param (
    [string]$ProjectFilePath,
    [string]$OutputPath
)

# Get the path to the project root
$RootDir = Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path)

# Load the csproj file to pull info from
$ProjectXml = [xml](Get-Content $ProjectFilePath)
$PluginName = (Select-Xml -Xml $ProjectXml -XPath "//AssemblyName").Node.InnerText
$PluginTitle = (Select-Xml -Xml $ProjectXml -XPath "//AssemblyTitle").Node.InnerText
$PluginVersion = (Select-Xml -Xml $ProjectXml -XPath "//Version").Node.InnerText
$PluginDescription = (Select-Xml -Xml $ProjectXml -XPath "//Description").Node.InnerText
$PluginUrl = (Select-Xml -Xml $ProjectXml -XPath "//PackageProjectUrl").Node.InnerText
$PluginAuthor = (Select-Xml -Xml $ProjectXml -XPath "//Authors").Node.InnerText

# Also try to find any additional files to include from the csproj file
$ExtraFiles = (Select-Xml -Xml $ProjectXml -XPath "//CopyToOutputDirectory") |
                Select-Object -ExpandProperty Node |
                Where-Object { $_.InnerText -ne "Never" } |
                Select-Object -ExpandProperty ParentNode |
                Select-Object -ExpandProperty Update

# Make a temporary folder to write our files into
$TempDir = Join-Path $RootDir "temp/"
Remove-Item $TempDir -Recurse -ErrorAction Ignore
New-Item -ItemType Directory -Path $TempDir | Out-Null

# Copy all our files into it
Copy-Item (Join-Path $RootDir "manifest.json") $TempDir
Copy-Item (Join-Path $RootDir "icon.png") $TempDir
Copy-Item (Join-Path $RootDir "README.md") $TempDir
Copy-Item (Join-Path $RootDir "LICENSE") $TempDir -ErrorAction Ignore
Copy-Item "${OutputPath}${PluginName}.dll" $TempDir
Copy-Item "${OutputPath}${PluginName}.dll.mdb" $TempDir -ErrorAction Ignore

# Copy all our extra files into the output dir
foreach ($ExtraFile in $ExtraFiles)
{
    # Powershell won't automatically make subfolder for us so have to do this.
    $DestinationFile = Join-Path $TempDir $ExtraFile
    $DestinationDirectory = Split-Path -Parent $DestinationFile
    if (!(Test-Path $DestinationDirectory)) { New-Item -ItemType Directory -Path $DestinationDirectory | Out-Null }
    Copy-Item "${OutputPath}${ExtraFile}" $DestinationFile
}

# Replace values in the manifest with our project info
$ManifestPath = Join-Path $TempDir "manifest.json"
$ManifestContent = Get-Content $ManifestPath
$ManifestContent = $ManifestContent.replace("{{NAME}}", $PluginTitle.replace(" ", "_"))
$ManifestContent = $ManifestContent.replace("{{VERSION}}", $PluginVersion)
$ManifestContent = $ManifestContent.replace("{{DESCRIPTION}}", $PluginDescription)
$ManifestContent = $ManifestContent.replace("{{URL}}", $PluginUrl)
$ManifestContent = $ManifestContent.replace("{{AUTHOR}}", $PluginAuthor)
Set-Content $ManifestPath $ManifestContent

# Make a zip archive from the folder
$OutputPath = Join-Path $OutputPath "${PluginName}.zip"
Compress-Archive -Path "${TempDir}\*" -DestinationPath $OutputPath -Force

# Delete the temp folder and we're done!
Remove-Item $TempDir -Recurse
