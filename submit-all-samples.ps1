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
begin {
  $allSamples = @()
}

# Accumulate everything we are going to submit.
# We are making the assumption that the generation of these sample names is much slower than the submission time.
# Submission can take 5 minutes. And doing them at once can make the process more efficient.
process {
  $allSamples += $samples
}

end {

	$jobName = "DiVertAnalysis"
	$jobVersion = 15

	$v = $PSBoundParameters['Verbose'] -eq $true

	# Submit each one for processing
	$allSamples `
		| Invoke-GRIDJob -JobName $jobName -JobVersion $jobVersion -Verbose:$v `
		| % {@{Dataset = $_.Name
				Task = $_.ID
				Status = Get-GridJobInfo -Verbose:$v -JobStatus $_.ID
				}} `
		| % { New-Object PSObject -Property $_ }
}

