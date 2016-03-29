$WshShell = New-Object -comObject WScript.Shell
$Shortcut = $WshShell.CreateShortcut("$Home\Desktop\UWPWorkshop.lnk")
$Shortcut.TargetPath = "C:\Labs\Codelabs-UWP\Workshop"
$Shortcut.Save()