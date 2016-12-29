#
# Finds cal ratio samples from the master list, and spits them one one at a time
#
[CmdletBinding()]
Param(
  [Parameter(Mandatory=$True, HelpMessage="The tag to use to extract the sampe name")]
   [string]$tags
)

# Helper function
function findDS ($tagName)
{
	$csvFile = "$PSScriptRoot\libDataAccess\Sample Meta Data.csv"
	$fullSampleList = Import-Csv $csvFile
	$l = $fullSampleList | ? {$_.Tags.Contains($tagName)}
	$samples = $l | % {$_."Sample Name"}
	return $samples
}

# Get the samples
$samples = findDS($tags)

# Write them out
foreach ($s in $samples) {
	Write-Output $s
}