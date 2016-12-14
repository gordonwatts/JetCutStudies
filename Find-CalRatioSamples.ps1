#
# Finds cal ratio samples from the master list, and spits them one one at a time
#
[CmdletBinding()]
Param(
  [Parameter(Mandatory=$True, HelpMessage="The tag to use to extract the sampe name")]
   [string]$tags
)

# Load in tools
. .\script-utils.ps1

# Get the samples
$samples = findDS($tags)

# Write them out
foreach ($s in $samples) {
	Write-Output $s
}