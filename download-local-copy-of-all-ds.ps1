#
# Download a copy of all datasets
#

$jobName = "DiVertAnalysis"
$jobVersion = 6

# Get teh list of samples from the csv file

$samples = Get-Content ".\libDataAccess\Sample Meta Data.csv"  | % {$_.Split(",")[0]} | Select-Object -Skip 1


$job = $samples | % {
    while (@(Get-Job -State Running).Count -ge 4) {
        Start-Sleep -Seconds 2
    }
	Write-Host "Starting $_"
    Start-Job -Name $_ -ScriptBlock {
        param ($sample, $jobName, $jobVersion)
		Get-GRIDDataset -nFiles 1 -JobName $jobName -JobVersion $jobVersion -JobSourceDatasetName $sample -Location Local        
    } -ArgumentList $_, $jobName, $jobVersion}
$jobsdone = $job | Wait-Job | Receive-Job
