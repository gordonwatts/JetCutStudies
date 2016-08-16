# Some utilities

# A function to look for all data sets with a particular tag in the csv file we use to
# drive this analysis.

function findDS ($tagName)
{
	$csvFile = "$PSScriptRoot\libDataAccess\Sample Meta Data.csv"
	$fullSampleList = Import-Csv $csvFile
	$l = $fullSampleList | ? {$_.Tags.Contains($tagName)}
	$samples = $l | % {$_."Sample Name"}
	return $samples
}