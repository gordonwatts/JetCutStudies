#
# Given a list of jobs, extract the run optimization numbers from the log files.
# This will require accessing the internet and the installation of the Jenkins powershell
# access package (see https://github.com/gordonwatts/JenkinsAccess).
#
# Example command script to generate a csv file:
# .\summarize-optimization-runs.ps1 | %{ New-Object psobject -Property $_ } | Export-Csv -NoTypeInformation -Path test.csv

[CmdletBinding()]
Param(
	[Parameter(Mandatory=$true, HelpMessage="The job number to start scanning")]
	[int] $FirstJob,
	[Parameter(Mandatory=$true, HelpMessage="The job number to stop scanning with")]
	[int] $LastJob
)

# Some helper functions
function Clean-PropertyValue
{
	if ($args[0] -eq """""") { "" } else { $args[0] }
}

# Grab the results from each job, cache them locally

$results = @()
foreach ($jobId in $FirstJob..$LastJob) {
	$jobInfo = Find-JenkinsJob -JobUri http://higgs.phys.washington.edu:8080/job/CalRatio2016/job/JetMVATraining/ -JobId $jobId
	if ($jobInfo.JobState -eq "Success") {
		$jobResults = @{ JobId = $jobId }
		$jobInfo.Parameters.Keys | % {$jobResults[$_] = Clean-PropertyValue $jobInfo.Parameters[$_]}
		$jobInfo `
			| Get-JenkinsBuildLogfile `
			| select -Last 80 `
			| % { [regex]::match($_, "efficiency for BDT ([^ ]+) (.+)$") } `
			| ? { $_.Success} `
			| % { @{ "Sample" = $_.Groups[1].Value; "Performance" = Clean-PropertyValue $_.Groups[2].Value} } `
			| % { $jobResults + $_ } `
			| % { Write-Output $_ }
	}
}
