#
# Scan a set of parameters and get output files for easy comparison
[CmdletBinding()]
Param(
)

runit(200)

function runit ([int] $nTrees)
{
	C:\Users\Gordon\Documents\Code\calratio2017\JetCutStudies\JetMVAClassifierTraining\bin\x86\Debug\JetMVAClassifierTraining.exe --BDTMaxDepth 50 --BDTLeafMinFraction 1 --TrainingEvents 1500000 "" --TrainEventsBIB16 0 --TrainEventsBIB15 -1 --SmallTestingMenu --nTrees $nTrees
	cp JetMVAClassifierTraining.root JetMVAClassifierTraining-nT$nTrees.root
}
