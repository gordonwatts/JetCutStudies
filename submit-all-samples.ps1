#
# Submit all the samples to run
#

$jobName = "DiVertAnalysis"
$jobVersion = 8

# Rucio command used to search for some of these:
# -bash-4.1$ rucio list-dids mc15_13TeV:*AOD*e5102* | grep CONTAINER

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

$samples = findDS("limit")

# Submit each one for processing
foreach ($s in $samples) {
	$jinfo = Invoke-GRIDJob -JobName $jobName -JobVersion $jobVersion $s
	$h = @{DataSet = $jinfo.Name
			Task = $jinfo.ID
			Status = Get-GRIDJobInfo -JobStatus $jinfo.ID
			}
	New-Object PSObject -Property $h
}