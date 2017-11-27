#
# Download a copy of needed datasets
#

[CmdletBinding()]
Param(
  [Parameter(Mandatory=$True, HelpMessage="The file list with the datasamples we need", Position=1)]
  [string]$NeededDatasetFile,
  [Parameter(HelpMessage="Get a single file, locally")]
  [Switch]$DownloadFileLocally,
  [Parameter(HelpMessage="Get all files to CERN")]
  [Switch]$DownloadToCERN,
  [Parameter(HelpMessage="Get all files to UW")]
  [Switch]$DownloadToUW,
  [Parameter(HelpMessage="Run right away")]
  [Switch]$DownloadOneAtATime
)

begin {
	# build extra download flags
	$flags = ""
	if ($DownloadFileLocally) {
		$flags = "-Location Local"
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
	# Download a dataset.
	function download_ds ($jobName, $jobVersion, $nFiles, $ds)
	{
		# Make sure we aren't trying to do too many things at once.
		while (@(Get-Job -State Running).Count -ge 4) {
			Start-Sleep -Seconds 2
		}

		# Kick off a download job if the job is finished.
		$j = Get-GRIDJobInfo -JobStatus -JobName $jobName -JobVersion $jobVersion -DatasetName  $ds
		if (($j -ne "finished") -and ($j -ne "done")) {
			Write-Host "Skipping $_ - it is in state $j"
		} else {
			Write-Host "Starting $ds"
			$cmd = "Get-GRIDDataset -nFiles $nFiles -JobName $jobName -JobVersion $jobVersion -JobSourceDatasetName $ds $flags"
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

	function download_ds_from_string ($info)
	{
		download_ds -jobName $info[0] -jobVersion $info[1] -nFiles $info[2] -ds $info[3]
	}

	# Load in the file, sort it, and remove duplicates.
	Get-Content $NeededDatasetFile | sort | Get-Unique | % {download_ds_from_string $($_ -split ' ')} 
}

end {
	# Wait for all jobs to finish.
	$downloadJobs | Wait-Job | Receive-Job
}
