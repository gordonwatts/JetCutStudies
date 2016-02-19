#
# Submit all the samples to run
#

$jobName = "DiVertAnalysis"
$jobVersion = 4

$samples = ("mc15_13TeV.361022.Pythia8EvtGen_A14NNPDF23LO_jetjet_JZ2W.merge.DAOD_EXOT15.e3668_s2576_s2132_r6765_r6282_p2452",
			"mc15_13TeV.361023.Pythia8EvtGen_A14NNPDF23LO_jetjet_JZ3W.merge.DAOD_EXOT15.e3668_s2576_s2132_r6765_r6282_p2452",
			"mc15_13TeV.361024.Pythia8EvtGen_A14NNPDF23LO_jetjet_JZ4W.merge.DAOD_EXOT15.e3668_s2576_s2132_r6765_r6282_p2452",
			"mc15_13TeV.304805.MadGraphPythia8EvtGen_A14NNPDF23LO_HSS_LLP_mH200_mS25_lt5m.merge.AOD.e4754_s2698_r7146_r6282",
			"mc15_13TeV.304813.MadGraphPythia8EvtGen_A14NNPDF23LO_HSS_LLP_mH400_mS100_lt9m.merge.AOD.e4754_s2698_r7146_r6282",
			"mc15_13TeV.304817.MadGraphPythia8EvtGen_A14NNPDF23LO_HSS_LLP_mH600_mS150_lt9m.merge.AOD.e4754_s2698_r7146_r6282"
		   )

foreach ($s in $samples) {
	$jinfo = Invoke-GRIDJob -JobName $jobName -JobVersion $jobVersion $s
	$h = @{DataSet = $jinfo.Name
			Task = $jinfo.ID
			Status = Get-GRIDJobInfo -JobStatus $jinfo.ID
			}
	New-Object PSObject -Property $h
}