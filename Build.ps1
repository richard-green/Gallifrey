Param(
  [alias("tc")][string]$TeamCity
)

Add-Type -A 'System.IO.Compression.FileSystem'

Write-Host "Current Version Number Is:"
Get-Content src\CurrentVersion.info
if($TeamCity.toLower() = "y")
{
	Write-Host "Will Not Update"
}
else
{
	$NewVersion = Read-Host "Enter New Version Number"
	New-Item src\CurrentVersion.info -type file -force -value $NewVersion
	Get-ChildItem -Path "src" -Include AssemblyInfo.cs -Recurse | Foreach-Object {
		$newFile = Get-Content $_ -encoding "UTF8" | Foreach-Object {
			if ($_.StartsWith("[assembly: AssemblyVersion")) {
				'[assembly: AssemblyVersion("' + $NewVersion + '")]'
			} else {
				if ($_.StartsWith("[assembly: AssemblyFileVersion")) {
					'[assembly: AssemblyFileVersion("' + $NewVersion + '")]'
				} else {
					$_
				} 
			}
		}
		
		$newFile | set-Content $_ -encoding "UTF8"
	}
	
	Get-ChildItem -Path "src" -Include "*.csproj" -Recurse | Foreach-Object {
		$newFile = Get-Content $_ -encoding "UTF8" | Foreach-Object {
			if ($_.StartsWith("    <MinimumRequiredVersion>")) {
				"    <MinimumRequiredVersion>$NewVersion</MinimumRequiredVersion>"
			} else {
				if ($_.StartsWith("    <ApplicationVersion>")) {
					"    <ApplicationVersion>$NewVersion</ApplicationVersion>"
				} else {
					$_
				} 
			}
		}
		
		$newFile | set-Content $_ -encoding "UTF8"
	}
}

Write-Host "Restoring Packages"
if(Test-Path "Output")
{
	Remove-Item "Output\*" -recurse
}
.paket\paket.bootstrapper.exe
.paket\paket.exe restore
if(-Not ($ENV:PATH -like "*MSBuild*"))
{
	if(Test-Path "${Env:ProgramFiles(x86)}\MSBuild\14.0\bin")
	{
		$ENV:Path = $ENV:Path + ";${Env:ProgramFiles(x86)}\MSBuild\14.0\bin"
	}
	if(Test-Path "${Env:ProgramFiles(x86)}\MSBuild\14.0\bin")
	{
		$ENV:Path = $ENV:Path + ";${Env:ProgramFiles(x86)}\MSBuild\14.0\bin"
	}
}

Write-Host "Doing Build"
msbuild.exe src\Gallifrey.sln /target:Clean,Rebuild,publish /property:Configuration=Release

Write-Host "Copy Published Items"
Copy-Item src\Gallifrey.UI.Modern.Alpha\bin\Release\app.publish\ Output\alpha -recurse
Copy-Item src\Gallifrey.UI.Modern.Beta\bin\Release\app.publish\ Output\beta -recurse
Copy-Item src\Gallifrey.UI.Modern.Stable\bin\Release\app.publish\ Output\stable -recurse

Write-Host "Create App Zips"
[IO.Compression.ZipFile]::CreateFromDirectory('Output\alpha', 'Output\alpha.zip')
[IO.Compression.ZipFile]::CreateFromDirectory('Output\beta', 'Output\beta.zip')
[IO.Compression.ZipFile]::CreateFromDirectory('Output\stable', 'Output\stable.zip')