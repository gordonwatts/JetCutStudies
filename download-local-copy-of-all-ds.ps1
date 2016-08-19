#
# Download a copy of all datasets
#

$jobName = "DiVertAnalysisNP"
$jobVersion = 2

# Get teh list of samples from the csv file

. .\script-utils.ps1

$samples = findDS("limit")

$job = $samples | % {
    while (@(Get-Job -State Running).Count -ge 4) {
        Start-Sleep -Seconds 2
    }
	$j = Get-GRIDJobInfo -JobStatus -JobName $jobName -JobVersion $jobVersion -DatasetName $_
	if (($j -ne "finished") -and ($j -ne "done")) {
		Write-Host "Skipping $_ - it is in state $j"
	} else {
		Write-Host "Starting $_"
		Start-Job -Name $_ -ScriptBlock {
			param ($sample, $jobName, $jobVersion)
			Get-GRIDDataset -JobName $jobName -JobVersion $jobVersion -JobSourceDatasetName $sample
			#-Location Local -nFiles 1 
		}
    } -ArgumentList $_, $jobName, $jobVersion}
$jobsdone = $job | Wait-Job | Receive-Job
