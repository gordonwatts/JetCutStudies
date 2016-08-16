# Extract from a training log file the correlation tables and dump them
# as something that could be put into text.

# What job should we pull from?
$jobId = 458
$jobUri = "http://jenks-higgs.phys.washington.edu:8080/job/CalR-JetMVATraining/"

# Get the log file
$log = Get-JenkinsBuildLogfile -JobUri $jobUri -JobId $jobId

##########
# Helper functions

# Look for text between some start and finish.
function CheckForStartFinish($startText, $finishText)
{
	Begin {
		$inside = 0
	}
	Process {
		if ($inside) {
			if ($_.Contains($finishText)) {
				$inside = 0
			}
		}
		if ($inside) {
			Write-Output $_
		}
		if (!$inside) {
			if ($_.Contains($startText)) {
				$inside = 1
			}
		}
	}
}

# Remove a bunch of the log file crap
function TrimLogfileChaff
{
	Process {
		if ($_.StartsWith("---")) {
			Write-Output $_.Substring($_.IndexOf(":")+1).Trim()
		}
	}
}

# Convert to tex table
function MakeTexTable
{
	Process
	{
		$cols = $_ -split " +",0,"Regex"
		$result = ""
		foreach ($c in $cols) {
			if ($result) {
				$result += " & "
			}
			$result += $c.Trim(":")
		}
		$result += " \\"
		Write-Output $result
	}
}

function GrabCorrelationTable ($log, $beginText, $endText)
{
	return $log `
		| CheckForStartFinish $beginText $endText `
		| where {!$_.Contains("----------------")} `
		| TrimLogfileChaff `
		| MakeTexTable
}

# First, go after the two large correlation tables.
$sigCor =  GrabCorrelationTable $log "Correlation matrix (Signal):" "Correlation matrix (Background):"
$backCor =  GrabCorrelationTable $log "Correlation matrix (Background):" "current transformation string"
$sigBDTCor = GrabCorrelationTable $log "Correlations between input variables and MVA response (signal):" "Correlations between input variables and MVA response (background):"
$backBDTCor = GrabCorrelationTable $log "Correlations between input variables and MVA response (background):" "matrices contain the fraction of"

Write-Output "Signal Correation:"
Write-Output $sigCor
Write-Output ""
Write-Output "Back Correation:"
Write-Output $backCor
Write-Output ""
Write-Output "Sig BDT Correation:"
Write-Output $sigBDTCor
Write-Output ""
Write-Output "Back BDT Correation:"
Write-Output $backBDTCor
