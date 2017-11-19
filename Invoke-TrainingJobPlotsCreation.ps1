#
# From a Jenkins job, pull the csv files and generate the plots with python that we have for a job.
#
[CmdletBinding()]
Param(
  [Parameter(Mandatory=$True, HelpMessage="the multi-class jenkins job number to fetch data from")]
    [int]$jobID,
  [Parameter(HelpMessage="Directory where we store job output, etc.")]
	[string]$locationPath = "..\MCAResultsData"
)

# By default we expect to run from a certain location. The directory must exist, or things are gonig to go wrong.
if (-not $(Test-Path $locationPath)) {
	throw [System.IO.FileNotFoundException] "$locationPath was not found! Please create folder first."
}

# Get the job output path
$jobDownloadLocation = "$location\$jobID"
if (-not (Test-Path "$jobDownloadLocation\*" -Include *.csv)) {
	Write-Progress -Activity "Creating Training Plots" -Status "Downloading Job Artifact"
	$f = Get-JenkinsArtifact -JobUri http://higgs.phys.washington.edu:8080/job/CalRatio2016/job/JetMVAClassifierTraining -JobId $jobID -ArtifactName all-csv-.zip
	Write-Progress -Activity "Creating Training Plots" -Status "Uncompressing Job Datafiles"
	Expand-Archive -Path $f.FullName -DestinationPath $jobDownloadLocation
}

# Next, run the python stuff. This might take a while.
Write-Progress -Activity "Creating Training Plots" -Status "Generating Plots"
$resultsLocation = "$locationPath\results"
python .\notebooks\training_job_dumper.py "$jobDownloadLocation" "$resultsLocation" $jobId
