#
# Finds cal ratio samples from the master list, and spits them one one at a time
#
[CmdletBinding()]
Param(
  [Parameter(Mandatory=$True, HelpMessage="the multi-class jenkins job number to download")]
   [int]$jobID,
  [Parameter(HelpMessage="If present, extract averages rather than MVA cut")]
   [switch]$Average
)

#
# Get the log file for this job
#
$jobUri = "http://higgs.phys.washington.edu:8080/job/CalRatio2016/job/JetMVAClassifierTraining/"

# Get the log file
$log = Get-JenkinsBuildLogfile -JobUri $jobUri -JobId $jobID

# Get the lines that hold the numbers we want.
if ($Average) {
	$search = "The MVA average error "
} else {
	$search = "The MVA cut "
}
$lines = $log | ? {$_.Contains($search)}
$lines | Write-Host

# Search for the numbers we car about
if ($Average) {
	$matchPattern = '^The MVA average error for (?<sample>[^ ]+) is (?<cut>[0-9\.]+)$'
} else {
	$matchPattern = '^The MVA cut (?<sample>[^ ]+) efficiency of 0.9 is (?<cut>[0-9\.]+)$'
}
Write-Host $matchPattern

$info = $lines | % {$_.Trim() -match $matchPattern} | % {"$($Matches["sample"]), $($Matches["cut"])" }
Write-Output $info
