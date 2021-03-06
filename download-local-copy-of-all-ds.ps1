#
# Download a copy of all datasets
#

[CmdletBinding()]
Param(
  [Parameter(Mandatory=$True, HelpMessage="The samples to submit against", Position=1, ValueFromPipeline=$True)]
  [string]$samples,
  [Parameter(HelpMessage="Get a single file, locally")]
  [Switch]$DownloadFileLocally,
  [Parameter(HelpMessage="Get all files to CERN")]
  [Switch]$DownloadToCERN,
  [Parameter(HelpMessage="Get all files to UW")]
  [Switch]$DownloadToUW,
  [Parameter(HelpMessage="Job version to download")]
  [int]$jobVersion=15,
  [Parameter(HelpMessage="The iteration, defaults to zero")]
  [int]$jobIteration = 0,
  [Parameter(HelpMessage="Run right away")]
  [Switch]$DownloadOneAtATime
)

begin {
	# What we will be working with
	$jobName = "DiVertAnalysis"

	# build extra download flags
	$flags = ""
	if ($DownloadFileLocally) {
		$flags = "-nFiles 1 -Location Local"
	}
	if ($DownloadToCERN) {
		$flags = "-Location CERNLLP-linux"
	}
	if ($DownloadToUW) {
		$flags = "-Location UWTeV-linux"
	}
	
	# Hold onto jobs that have been started
	$downloadJobs = @()
}

process {
	# Make sure we aren't trying to do too many things at once.
    while (@(Get-Job -State Running).Count -ge 4) {
        Start-Sleep -Seconds 2
    }
	
	# Kick off a download job if the job is finished.
	$j = Get-GRIDJobInfo -JobStatus -JobName $jobName -JobVersion $jobVersion -JobIteration $jobIteration -DatasetName  $_
	if (($j -ne "finished") -and ($j -ne "done")) {
		Write-Host "Skipping $_ - it is in state $j"
	} else {
		Write-Host "Starting $_"
		$cmd = "Get-GRIDDataset -JobName $jobName -JobVersion $jobVersion -JobIteration $jobIteration -JobSourceDatasetName $_ $flags"
		if ($DownloadOneAtATime) {
			Invoke-Expression $cmd
		} else {
			$job = Start-Job -Name $_ -ScriptBlock {
				param ($cmd)
				Invoke-Expression $cmd
			} -ArgumentList $cmd
			$downloadJobs += $job
		}
	}
}

end {
	# Wait for all jobs to finish.
	$downloadJobs | Wait-Job | Receive-Job
}
