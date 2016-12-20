#
# Submit a list of DataSet samples to run
#
[CmdletBinding()]
Param(
  [Parameter(Mandatory=$True, HelpMessage="The samples to submit against", Position=1, ValueFromPipeline=$True)]
   [string]$samples
)

#
# Now, process everything
#
process {

	$jobName = "DiVertAnalysis"
	$jobVersion = 13

	# Submit each one for processing
	$samples `
		| Invoke-GRIDJob -JobName $jobName -JobVersion $jobVersion `
		| % {@{Dataset = $_.Name
				Task = $_.ID
				Status = Get-GridJobInfo -JobStatus $_.ID
				}} `
		| % { New-Object PSObject -Property $_ }
}

