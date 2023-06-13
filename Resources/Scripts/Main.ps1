function Initialize {
	Write-Output "Attempting to set Exectution Policy";
	Set-ExecutionPolicy -ExecutionPolicy Unrestricted -Scope CurrentUser -Force;
	Write-Output "Importing Modules";
	Import-Module ActiveDirectory;
	Write-Output "Application Started";
}

function GetResources {
	return Get-ADComputer -Filter * | Select-Object DistinguishedName, DNSHostName, Enabled, Name, ObjectClass, ObjectGUID, SamAccountName, UserPrincipalName | ConvertTo-Json;
}