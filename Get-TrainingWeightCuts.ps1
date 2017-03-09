#
# Finds cal ratio samples from the master list, and spits them one one at a time
#
[CmdletBinding()]
Param(
  [Parameter(Mandatory=$True, HelpMessage="the multi-class jenkins job number to download")]
   [int]$jobID
)

#
# Get the log file for this job
#
$jobUri = "http://higgs.phys.washington.edu:8080/job/CalRatio2016/job/JetMVAClassifierTraining/"

# Get the log file
$log = Get-JenkinsBuildLogfile -JobUri $jobUri -JobId $jobID

# Get the lines that hold the numbers we want.
$lines = $log | ? {$_.Contains("The MVA cut ")}

# Search for the numbers we car about
$info = $lines | % {$_ -match '^The MVA cut (?<sample>[^ ]+) efficiency of 0.9 is (?<cut>[0-9\.]+)$'} | % {"$($Matches["sample"]), $($Matches["cut"])" }

Write-Output $info
