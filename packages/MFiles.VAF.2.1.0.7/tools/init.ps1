param($installPath, $toolsPath, $package, $project)
$project.Object.References | Where-Object { $_.Name -eq "Interop.MFilesAPI" } |  ForEach-Object { $_.EmbedInteropTypes = $false }