#
# Submit all the samples to run
#

$jobName = "DiVertAnalysis"
$jobVersion = 9

# Rucio command used to search for some of these:
# -bash-4.1$ rucio list-dids mc15_13TeV:*AOD*e5102* | grep CONTAINER

. .\script-utils.ps1

$samples = findDS("mc15c+signal")

# Submit each one for processing
foreach ($s in $samples) {
	$jinfo = Invoke-GRIDJob -JobName $jobName -JobVersion $jobVersion $s
	$h = @{DataSet = $jinfo.Name
			Task = $jinfo.ID
			Status = Get-GRIDJobInfo -JobStatus $jinfo.ID
			}
	$o = New-Object PSObject -Property $h
	Write-Output $o
}