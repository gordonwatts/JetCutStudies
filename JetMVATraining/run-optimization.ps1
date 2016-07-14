#
# Run the optimization on jenkins, produce a csv file with the output
#

# Set ranges - we will loop over all of them
$nBackgroundEvents = @(1000000, 1500000, 2000000)
$trainingDepth = @(30, 40, 50)
$leafFraction = @(1, 0.5, 0.25)

# Now, invoke them all!

$tsets = @()

foreach ($nb in $nBackgroundEvents) {
	foreach($td in $trainingDepth) {
		foreach($lf in $leafFraction) {
			$tsets = $tsets + (@{"BDTrainingTreeDepth" = [string] $td; "BDTLeafMinFraction" = [string] $lf; "TrainingEvents" = [string] $nb})
		}
	}
}
$tsets | Invoke-JenkinsJob http://jenks-higgs.phys.washington.edu:8080/view/LLP/job/CalR-JetMVATraining/
