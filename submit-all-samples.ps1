#
# Submit all the samples to run
#

$jobName = "DiVertAnalysis"
$jobVersion = 5

# Rucio command used to search for some of these:
# -bash-4.1$ rucio list-dids mc15_13TeV:*AOD*e5102* | grep CONTAINER

$samples = (
			# Jet Samples
			"mc15_13TeV.361022.Pythia8EvtGen_A14NNPDF23LO_jetjet_JZ2W.merge.DAOD_EXOT15.e3668_s2576_s2132_r6765_r6282_p2452",
			"mc15_13TeV.361023.Pythia8EvtGen_A14NNPDF23LO_jetjet_JZ3W.merge.DAOD_EXOT15.e3668_s2576_s2132_r6765_r6282_p2452",
			"mc15_13TeV.361024.Pythia8EvtGen_A14NNPDF23LO_jetjet_JZ4W.merge.DAOD_EXOT15.e3668_s2576_s2132_r6765_r6282_p2452",
			"mc15_13TeV.361025.Pythia8EvtGen_A14NNPDF23LO_jetjet_JZ5W.merge.DAOD_EXOT15.e3668_s2576_s2132_r6765_r6282_p2452",
			"mc15_13TeV.361026.Pythia8EvtGen_A14NNPDF23LO_jetjet_JZ6W.merge.DAOD_EXOT15.e3569_s2608_s2183_r6765_r6282_p2452",
			"mc15_13TeV.361027.Pythia8EvtGen_A14NNPDF23LO_jetjet_JZ7W.merge.DAOD_EXOT15.e3668_s2608_s2183_r6765_r6282_p2452",

			# HSS samples (old)
			"mc15_13TeV.304803.MadGraphPythia8EvtGen_A14NNPDF23LO_HSS_LLP_mH100_mS25_lt5m.merge.AOD.e4754_s2698_r7146_r6282",

			"mc15_13TeV.304795.MadGraphPythia8EvtGen_A14NNPDF23LO_HSS_LLP_mH125_mS25_lt5m.merge.AOD.e4754_s2698_r7146_r6282",
			"mc15_13TeV.304799.MadGraphPythia8EvtGen_A14NNPDF23LO_HSS_LLP_mH125_mS25_lt9m.merge.AOD.e4754_s2698_r7146_r6282",
			"mc15_13TeV.304800.MadGraphPythia8EvtGen_A14NNPDF23LO_HSS_LLP_mH125_mS40_lt9m.merge.AOD.e4754_s2698_r7146_r6282",
			"mc15_13TeV.304796.MadGraphPythia8EvtGen_A14NNPDF23LO_HSS_LLP_mH125_mS55_lt5m.merge.AOD.e4754_s2698_r7146_r6282",
			"mc15_13TeV.304801.MadGraphPythia8EvtGen_A14NNPDF23LO_HSS_LLP_mH125_mS55_lt9m.merge.AOD.e4804_s2726_r7146_r6282",

			"mc15_13TeV.304805.MadGraphPythia8EvtGen_A14NNPDF23LO_HSS_LLP_mH200_mS25_lt5m.merge.AOD.e4754_s2698_r7146_r6282",
			"mc15_13TeV.304806.MadGraphPythia8EvtGen_A14NNPDF23LO_HSS_LLP_mH200_mS50_lt5m.merge.AOD.e4754_s2698_r7146_r6282",
			"mc15_13TeV.304809.MadGraphPythia8EvtGen_A14NNPDF23LO_HSS_LLP_mH200_mS50_lt9m.merge.AOD.e4754_s2698_r7146_r6282",

			"mc15_13TeV.304810.MadGraphPythia8EvtGen_A14NNPDF23LO_HSS_LLP_mH400_mS50_lt5m.merge.AOD.e4754_s2698_r7146_r6282",
			#The container is there, but no files are there.
			#"mc15_13TeV.304812.MadGraphPythia8EvtGen_A14NNPDF23LO_HSS_LLP_mH400_mS50_lt9m.merge.AOD.e4754_s2698_r7146_r6282",
			"mc15_13TeV.304811.MadGraphPythia8EvtGen_A14NNPDF23LO_HSS_LLP_mH400_mS100_lt5m.merge.AOD.e4754_s2698_r7146_r6282",
			"mc15_13TeV.304813.MadGraphPythia8EvtGen_A14NNPDF23LO_HSS_LLP_mH400_mS100_lt9m.merge.AOD.e4754_s2698_r7146_r6282",

			"mc15_13TeV.304814.MadGraphPythia8EvtGen_A14NNPDF23LO_HSS_LLP_mH600_mS50_lt5m.merge.AOD.e4754_s2698_r7146_r6282",
			"mc15_13TeV.304816.MadGraphPythia8EvtGen_A14NNPDF23LO_HSS_LLP_mH600_mS50_lt9m.merge.AOD.e4754_s2698_r7146_r6282",
			"mc15_13TeV.304815.MadGraphPythia8EvtGen_A14NNPDF23LO_HSS_LLP_mH600_mS150_lt5m.merge.AOD.e4754_s2698_r7146_r6282",
			"mc15_13TeV.304817.MadGraphPythia8EvtGen_A14NNPDF23LO_HSS_LLP_mH600_mS150_lt9m.merge.AOD.e4754_s2698_r7146_r6282",

			"mc15_13TeV.304818.MadGraphPythia8EvtGen_A14NNPDF23LO_HSS_LLP_mH1000_mS50_lt5m.merge.AOD.e4754_s2698_r7146_r6282",
			"mc15_13TeV.304821.MadGraphPythia8EvtGen_A14NNPDF23LO_HSS_LLP_mH1000_mS50_lt9m.merge.AOD.e4754_s2698_r7146_r6282",
			"mc15_13TeV.304822.MadGraphPythia8EvtGen_A14NNPDF23LO_HSS_LLP_mH1000_mS150_lt9m.merge.AOD.e4754_s2698_r7146_r6282",
			"mc15_13TeV.304819.MadGraphPythia8EvtGen_A14NNPDF23LO_HSS_LLP_mH1000_mS150_lt5m.merge.AOD.e4754_s2698_r7146_r6282",
			"mc15_13TeV.304823.MadGraphPythia8EvtGen_A14NNPDF23LO_HSS_LLP_mH1000_mS400_lt9m.merge.AOD.e4754_s2698_r7146_r6282",
			"mc15_13TeV.304820.MadGraphPythia8EvtGen_A14NNPDF23LO_HSS_LLP_mH1000_mS400_lt5m.merge.AOD.e4754_s2698_r7146_r6282",

			# HSS samples (new)
			"mc15_13TeV:mc15_13TeV.304820.MadGraphPythia8EvtGen_A14NNPDF23LO_HSS_LLP_mH1000_mS400_lt5m.merge.AOD.e5102_s2698_r7146_r6282", 
			"mc15_13TeV:mc15_13TeV.304818.MadGraphPythia8EvtGen_A14NNPDF23LO_HSS_LLP_mH1000_mS50_lt5m.merge.AOD.e5102_s2698_r7146_r6282", 
			"mc15_13TeV:mc15_13TeV.304814.MadGraphPythia8EvtGen_A14NNPDF23LO_HSS_LLP_mH600_mS50_lt5m.merge.AOD.e5102_s2698_r7146_r6282", 
			"mc15_13TeV:mc15_13TeV.304806.MadGraphPythia8EvtGen_A14NNPDF23LO_HSS_LLP_mH200_mS50_lt5m.merge.AOD.e5102_s2698_r7146_r6282", 
			"mc15_13TeV:mc15_13TeV.304810.MadGraphPythia8EvtGen_A14NNPDF23LO_HSS_LLP_mH400_mS50_lt5m.merge.AOD.e5102_s2698_r7146_r6282", 
			"mc15_13TeV:mc15_13TeV.304815.MadGraphPythia8EvtGen_A14NNPDF23LO_HSS_LLP_mH600_mS150_lt5m.merge.AOD.e5102_s2698_r7146_r6282", 
			"mc15_13TeV:mc15_13TeV.304811.MadGraphPythia8EvtGen_A14NNPDF23LO_HSS_LLP_mH400_mS100_lt5m.merge.AOD.e5102_s2698_r7146_r6282", 
			"mc15_13TeV:mc15_13TeV.304805.MadGraphPythia8EvtGen_A14NNPDF23LO_HSS_LLP_mH200_mS25_lt5m.merge.AOD.e5102_s2698_r7146_r6282", 
			"mc15_13TeV:mc15_13TeV.304804.MadGraphPythia8EvtGen_A14NNPDF23LO_HSS_LLP_mH200_mS8_lt5m.merge.AOD.e5102_s2698_r7146_r6282", 

			# Stealth SUSY
			"mc15_13TeV.304903.MadGraphPythia8EvtGen_A14NNPDF23LO_StealthSUSY_mG250.merge.AOD.e4811_s2698_r7146_r6282",
			"mc15_13TeV.304904.MadGraphPythia8EvtGen_A14NNPDF23LO_StealthSUSY_mG500.merge.AOD.e4811_s2698_r7146_r6282",
			"mc15_13TeV.304905.MadGraphPythia8EvtGen_A14NNPDF23LO_StealthSUSY_mG800.merge.AOD.e4811_s2698_r7146_r6282",
			"mc15_13TeV.304906.MadGraphPythia8EvtGen_A14NNPDF23LO_StealthSUSY_mG1200.merge.AOD.e4811_s2698_r7146_r6282",
			"mc15_13TeV.304907.MadGraphPythia8EvtGen_A14NNPDF23LO_StealthSUSY_mG1500.merge.AOD.e4811_s2698_r7146_r6282",
			"mc15_13TeV.304908.MadGraphPythia8EvtGen_A14NNPDF23LO_StealthSUSY_mG2000.merge.AOD.e4811_s2698_r7146_r6282"
		   )

# Submit each one for processing
foreach ($s in $samples) {
	$jinfo = Invoke-GRIDJob -JobName $jobName -JobVersion $jobVersion $s
	$h = @{DataSet = $jinfo.Name
			Task = $jinfo.ID
			Status = Get-GRIDJobInfo -JobStatus $jinfo.ID
			}
	New-Object PSObject -Property $h
}