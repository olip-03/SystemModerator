$operatingDomain;
$baseTreeView;

function Initialize {
	Write-Output "Attempting to set Exectution Policy";
	Set-ExecutionPolicy -ExecutionPolicy Unrestricted -Scope CurrentUser -Force;

	Write-Output "Importing Modules";
	Import-Module ActiveDirectory;

	$operatingDomain = Get-ADDomain -Current LoggedOnUser | Select-Object Forest;
	$baseTreeView = FindAt;

	Write-Output "Application Started";
}

function GetResources {
	return Get-ADComputer -Filter * | Select-Object DistinguishedName, DNSHostName, Enabled, Name, ObjectClass, ObjectGUID, SamAccountName, UserPrincipalName | ConvertTo-Json;
}

function FindAt($searchBase)
{
	$searchData;
	if([string]::IsNullOrEmpty($searchBase))
	{
		$searchData = Get-ADOrganizationalUnit -LDAPFilter '(name=*)' -SearchScope OneLevel | Select-Object Name, ObjectClass, DistinguishedName | ConvertTo-Json;
	}
	else
	{
		$searchData = Get-ADOrganizationalUnit -LDAPFilter '(name=*)' -SearchBase $searchBase -SearchScope OneLevel | Select-Object Name, ObjectClass, DistinguishedName | ConvertTo-Json;
	}

	return $searchData;
}