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
  [Parameter(HelpMessage="Run right away")]
  [Switch]$DownloadOneAtATime
)

begin {
	# What we will be working with
	$jobName = "DiVertAnalysis"

	# build extra download flags
	$flags = ""
	if ($DownloadFileLocally) {
		$flags = "-nFiles 1 -DestinationLocation Local"
	}
	if ($DownloadToCERN) {
		$flags = "-DestinationLocation CERNLLP-linux"
	}
	if ($DownloadToUW) {
		$flags = "-DestinationLocation UWTeV-linux"
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
	Write-Host "Starting $_"
	$cmd = "Copy-GRIDDataset -SourceLocation CERNLLP-linux -DatasetName $_ $flags"
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

end {
	# Wait for all jobs to finish.
	$downloadJobs | Wait-Job | Receive-Job
}
