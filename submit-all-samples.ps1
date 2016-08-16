#
# Submit all the samples to run
#

$jobName = "DiVertAnalysis"
$jobVersion = 8

# Rucio command used to search for some of these:
# -bash-4.1$ rucio list-dids mc15_13TeV:*AOD*e5102* | grep CONTAINER

. .\script-utils.ps1

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