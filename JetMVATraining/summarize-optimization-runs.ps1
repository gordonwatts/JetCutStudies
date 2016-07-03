#
# Given a list of jobs, extract the run optimization numbers from the log files.
# This will require accessing the internet and the installation of the Jenkins powershell
# access package (see https://github.com/gordonwatts/JenkinsAccess).
#
# Example command script to generate a csv file:
# .\summarize-optimization-runs.ps1 | %{ New-Object psobject -Property $_ } | Export-Csv -NoTypeInformation -Path test.csv

# The optimization jobs to scan over.
$firstJob = 418
$lastJob = 451

# Some helper functions
function Clean-PropertyValue
{
	if ($args[0] -eq """""") { "" } else { $args[0] }
}

# Grab the results from each job, cache them locally

$results = @()
foreach ($jobId in $firstJob..$lastJob) {
	$jobInfo = Find-JenkinsJob -JobUri http://jenks-higgs.phys.washington.edu:8080/view/LLP/job/CalR-JetMVATraining/ -JobId $jobId
	if ($jobInfo.JobState -eq "Success") {
		$jobResults = @{ JobId = $jobId }
		$jobInfo.Parameters.Keys | % {$jobResults[$_] = Clean-PropertyValue $jobInfo.Parameters[$_]}
		$jobInfo `
			| Get-JenkinsBuildLogfile `
			| select -Last 80 `
			| % { [regex]::match($_, "efficiency for BDT ([^:]+): (.+)\.$") } `
			| ? { $_.Success} `
			| % { @{ "Sample" = $_.Groups[1].Value; "Performance" = Clean-PropertyValue $_.Groups[2].Value} } `
			| % { $jobResults + $_ } `
			| % { Write-Output $_ }
	}
}
