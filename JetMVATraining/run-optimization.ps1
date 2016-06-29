#
# Run the optimization on jenkins, produce a csv file with the output
#

# Set ranges - we will loop over all of them
$nBackgroundEvents = @(500000, 1000000, 1500000)
$trainingDepth = @(20, 30, 40)
$leafFraction = @(2, 1, 0.5)

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
