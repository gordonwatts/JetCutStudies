#
# Summary of input and output samples
#

$jobName = "DiVertAnalysis"
$jobVersion = 6

# Get teh list of samples from the csv file

$samples = Get-Content ".\libDataAccess\Sample Meta Data.csv"  | % {$_.Split(",")[0]} | Select-Object -Skip 1


foreach ($s in $samples) {
	$outName = Get-GRIDJobInfo -DatasetName $s -JobName $jobName -JobVersion $jobVersion -OutputContainerNames
	Write-Output $s
	Write-Output "  -> $outName"
}