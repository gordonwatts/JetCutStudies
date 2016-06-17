/////////////////////////////////////////////////////////////////////////
//   This class has been automatically generated 
//   (at Fri Jun 17 00:47:16 2016 by ROOT version 5.34/36)
//   from TTree recoTree/recoTree
//   found on file: E:\GRIDDS\user.gwatts.361024.Pythia8EvtGen_A14NNPDF23LO_jetjet_JZ4W.DAOD_EXOT15.r6765_r6282_p2452.DiVertAnalysis_v5_1BC42831_hist\user.gwatts\user.gwatts.8578406._000001.hist-output.root
/////////////////////////////////////////////////////////////////////////


#ifndef ntuple_recoTree_h
#define ntuple_recoTree_h

// System Headers needed by the proxy
#if defined(__CINT__) && !defined(__MAKECINT__)
   #define ROOT_Rtypes
   #define ROOT_TError
#endif
#include <TROOT.h>
#include <TChain.h>
#include <TFile.h>
#include <TPad.h>
#include <TH1.h>
#include <TSelector.h>
#include <TBranchProxy.h>
#include <TBranchProxyDirector.h>
#include <TBranchProxyTemplate.h>
#include <TFriendProxy.h>
using namespace ROOT;

// forward declarations needed by this particular proxy


// Header needed by this particular proxy
#include <vector>


class junk_macro_parsettree_recoTree_Interface {
   // This class defines the list of methods that are directly used by ntuple_recoTree,
   // and that can be overloaded in the user's script
public:
   void junk_macro_parsettree_recoTree_Begin(TTree*) {}
   void junk_macro_parsettree_recoTree_SlaveBegin(TTree*) {}
   Bool_t junk_macro_parsettree_recoTree_Notify() { return kTRUE; }
   Bool_t junk_macro_parsettree_recoTree_Process(Long64_t) { return kTRUE; }
   void junk_macro_parsettree_recoTree_SlaveTerminate() {}
   void junk_macro_parsettree_recoTree_Terminate() {}
};


class ntuple_recoTree : public TSelector, public junk_macro_parsettree_recoTree_Interface {
public :
   TTree          *fChain;         //!pointer to the analyzed TTree or TChain
   TH1            *htemp;          //!pointer to the histogram
   TBranchProxyDirector fDirector; //!Manages the proxys

   // Optional User methods
   TClass         *fClass;    // Pointer to this class's description

   // Proxy for each of the branches, leaves and friends of the tree
   TIntProxy                                event_count;
   TIntProxy                                eventNumber;
   TIntProxy                                runNumber;
   TUIntProxy                               backgroundFlags;
   TIntProxy                                lumiBlock;
   TDoubleProxy                             eventWeight;
   TIntProxy                                actualIntPerCrossing;
   TIntProxy                                averageIntPerCrossing;
   TDoubleProxy                             event_HT;
   TIntProxy                                event_NJets;
   TDoubleProxy                             event_HTMiss;
   TDoubleProxy                             event_Meff;
   TBoolProxy                               hasGoodPV;
   TBoolProxy                               isGoodLAr;
   TBoolProxy                               isGoodTile;
   TBoolProxy                               isGoodSCT;
   TBoolProxy                               isCompleteEvent;
   TIntProxy                                jetsPassingCalREmul;
   TDoubleProxy                             dPhiLeadingCalibJets;
   TDoubleProxy                             dRLeadingCalibJets;
   TDoubleProxy                             event_tauRoI_maxET;
   TBoolProxy                               event_passCalRatio_TAU30;
   TBoolProxy                               event_passCalRatio_LLPNOMATCH;
   TBoolProxy                               event_passCalRatio_TAU60;
   TBoolProxy                               event_passCalRatio_cleanLLP_TAU60;
   TBoolProxy                               event_passCalRatio_LLPNOMATCH_noiso;
   TBoolProxy                               event_passCalRatio_TAU60_noiso;
   TBoolProxy                               event_passCalRatio_cleanLLP_TAU60_noiso;
   TBoolProxy                               event_passMuvtx;
   TBoolProxy                               event_passMuvtx_noiso;
   TBoolProxy                               event_passL1TAU30;
   TBoolProxy                               event_passL1TAU40;
   TBoolProxy                               event_passL1TAU60;
   TBoolProxy                               event_passL1LLPNOMATCH;
   TBoolProxy                               event_passJ25;
   TBoolProxy                               event_passJ100;
   TBoolProxy                               event_passJ400;
   TStlSimpleProxy<vector<bool> >           CalibJet_isGoodStand;
   TStlSimpleProxy<vector<bool> >           CalibJet_isGoodLLP;
   TStlSimpleProxy<vector<double> >         CalibJet_E;
   TStlSimpleProxy<vector<double> >         CalibJet_ET;
   TStlSimpleProxy<vector<double> >         CalibJet_pT;
   TStlSimpleProxy<vector<double> >         CalibJet_eta;
   TStlSimpleProxy<vector<double> >         CalibJet_phi;
   TStlSimpleProxy<vector<double> >         CalibJet_width;
   TStlSimpleProxy<vector<double> >         CalibJet_logRatio;
   TStlSimpleProxy<vector<double> >         CalibJet_EMF;
   TStlSimpleProxy<vector<double> >         CalibJet_time;
   TStlSimpleProxy<vector<double> >         CalibJet_BDT;
   TStlSimpleProxy<vector<bool> >           CalibJet_isCRHLTJet;
   TStlSimpleProxy<vector<int> >            CalibJet_indexLLP;
   TStlSimpleProxy<vector<unsigned int> >   CalibJet_nTrk;
   TStlSimpleProxy<vector<double> >         CalibJet_sumTrkpT;
   TStlSimpleProxy<vector<double> >         CalibJet_maxTrkpT;
   TStlSimpleProxy<vector<double> >         CalibJet_minDRTrkpt2;
   TStlSimpleProxy<vector<bool> >           CalibJet_matchPassEmul;
   TStlSimpleProxy<vector<int> >            CalibJet_cluster_nClusters;
   TStlSimpleProxy<vector<double> >         CalibJet_cluster_lead_r;
   TStlSimpleProxy<vector<double> >         CalibJet_cluster_lead_x;
   TStlSimpleProxy<vector<double> >         CalibJet_cluster_lead_y;
   TStlSimpleProxy<vector<double> >         CalibJet_cluster_lead_z;
   TStlSimpleProxy<vector<double> >         CalibJet_cluster_lead_Lxy;
   TStlSimpleProxy<vector<double> >         CalibJet_cluster_lead_ET;
   TStlSimpleProxy<vector<double> >         CalibJet_cluster_lead_pT;
   TStlSimpleProxy<vector<double> >         CalibJet_cluster_lead_eta;
   TStlSimpleProxy<vector<double> >         CalibJet_cluster_lead_phi;
   TStlSimpleProxy<vector<double> >         CalibJet_cluster_lead_calibET;
   TStlSimpleProxy<vector<double> >         CalibJet_cluster_lead_calibEta;
   TStlSimpleProxy<vector<double> >         CalibJet_cluster_lead_calibPhi;
   TStlSimpleProxy<vector<double> >         CalibJet_cluster_lead_time;
   TStlSimpleProxy<vector<double> >         CalibJet_cluster_lead_lambda;
   TStlSimpleProxy<vector<double> >         CalibJet_cluster_lead_sec_r;
   TStlSimpleProxy<vector<double> >         CalibJet_cluster_lead_sec_lambda;
   TStlSimpleProxy<vector<double> >         CalibJet_cluster_lead_qual_tile;
   TStlSimpleProxy<vector<double> >         CalibJet_cluster_lead_qual_lar;
   TStlSimpleProxy<vector<double> >         CalibJet_cluster_lead_lateral;
   TStlSimpleProxy<vector<double> >         CalibJet_cluster_lead_longitudinal;
   TStlSimpleProxy<vector<double> >         CalibJet_cluster_lead_first_Eden;
   TStlSimpleProxy<vector<double> >         CalibJet_cluster_lead_EMF;
   TStlSimpleProxy<vector<double> >         CalibJet_cluster_lead_EEM;
   TStlSimpleProxy<vector<double> >         CalibJet_cluster_lead_Etot;
   TStlSimpleProxy<vector<double> >         CalibJet_cluster_lead_dR_tojet;
   TStlSimpleProxy<vector<double> >         CalibJet_cluster_lead_fracE_ofjet;
   TStlSimpleProxy<vector<double> >         CalibJet_cluster_lead_jetEMF_revised;
   TStlSimpleProxy<vector<double> >         Track_pT;
   TStlSimpleProxy<vector<double> >         Track_eta;
   TStlSimpleProxy<vector<double> >         Track_phi;
   TStlSimpleProxy<vector<double> >         MSTracklet_eta;
   TStlSimpleProxy<vector<double> >         MSTracklet_phi;
   TStlSimpleProxy<vector<double> >         MSeg_etaPos;
   TStlSimpleProxy<vector<double> >         MSeg_thetaPos;
   TStlSimpleProxy<vector<double> >         MSeg_phiPos;
   TStlSimpleProxy<vector<double> >         MSeg_thetaDir;
   TStlSimpleProxy<vector<double> >         MSeg_phiDir;
   TStlSimpleProxy<vector<double> >         MSeg_t0;
   TStlSimpleProxy<vector<int> >            MSeg_chIndex;
   TStlSimpleProxy<vector<int> >            MSeg_techIndex;
   TDoubleProxy                             METCST_met;
   TDoubleProxy                             METCST_phi;
   TDoubleProxy                             METCST_mpx;
   TDoubleProxy                             METCST_mpy;
   TDoubleProxy                             METCST_sumet;
   TDoubleProxy                             METTST_met;
   TDoubleProxy                             METTST_phi;
   TDoubleProxy                             METTST_mpx;
   TDoubleProxy                             METTST_mpy;
   TDoubleProxy                             METTST_sumet;
   TStlSimpleProxy<vector<double> >         CR_HLT_ET;
   TStlSimpleProxy<vector<double> >         CR_HLT_pT;
   TStlSimpleProxy<vector<double> >         CR_HLT_eta;
   TStlSimpleProxy<vector<double> >         CR_HLT_phi;
   TStlSimpleProxy<vector<double> >         CR_HLT_EMF;
   TStlSimpleProxy<vector<double> >         CR_HLT_logR;
   TStlSimpleProxy<vector<double> >         CR_HLT_minDR_HLTtracks;
   TStlSimpleProxy<vector<double> >         CR_HLT_dR;
   TStlSimpleProxy<vector<bool> >           CR_HLT_matchOff;
   TStlSimpleProxy<vector<double> >         CR_HLT_LLP_DR;
   TStlSimpleProxy<vector<double> >         CR_HLT_LLP_Lxy;
   TStlSimpleProxy<vector<double> >         CR_HLT_LLP_Lz;
   TStlSimpleProxy<vector<double> >         CR_HLT_NegativeE;
   TStlSimpleProxy<vector<double> >         CR_HLT_FracSamplingMax;
   TStlSimpleProxy<vector<double> >         CR_HLT_HECQuality;
   TStlSimpleProxy<vector<double> >         CR_HLT_LArQuality;
   TStlSimpleProxy<vector<double> >         CR_HLT_AverageLArQF;
   TStlSimpleProxy<vector<double> >         CR_HLT_HECFrac;
   TStlSimpleProxy<vector<double> >         CR_HLT_Timing;
   TStlSimpleProxy<vector<bool> >           CR_offline_HLTassoc;
   TStlSimpleProxy<vector<double> >         CR_offline_eta;
   TStlSimpleProxy<vector<double> >         CR_offline_phi;
   TStlSimpleProxy<vector<double> >         CR_offline_ET;
   TStlSimpleProxy<vector<double> >         CR_offline_pT;
   TStlSimpleProxy<vector<double> >         CR_offline_EMF;
   TStlSimpleProxy<vector<double> >         CR_offline_logR;
   TStlSimpleProxy<vector<double> >         CR_offline_nTrks;
   TStlSimpleProxy<vector<double> >         CR_offline_timing;
   TStlSimpleProxy<vector<bool> >           CR_offline_matchLLP;
   TStlSimpleProxy<vector<double> >         CR_offline_LLP_DR;
   TStlSimpleProxy<vector<double> >         CR_offline_LLP_Lxy;
   TStlSimpleProxy<vector<double> >         CR_offline_LLP_Lz;
   TStlSimpleProxy<vector<bool> >           CR_offline_passCuts;
   TStlSimpleProxy<vector<int> >            CR_offline_jetindex;
   TStlSimpleProxy<vector<bool> >           CR_offline_goodjet_stand;
   TStlSimpleProxy<vector<bool> >           CR_offline_goodjet_llp;
   TStlSimpleProxy<vector<double> >         CR_offline_BDT;
   TStlSimpleProxy<vector<double> >         CalR_emul_jetET;
   TStlSimpleProxy<vector<double> >         CalR_emul_jetPt;
   TStlSimpleProxy<vector<double> >         CalR_emul_jetEta;
   TStlSimpleProxy<vector<double> >         CalR_emul_jetPhi;
   TStlSimpleProxy<vector<double> >         CalR_emul_jetEMF;
   TStlSimpleProxy<vector<double> >         CalR_emul_jetLogR;
   TStlSimpleProxy<vector<double> >         CalR_emul_jetTime;
   TStlSimpleProxy<vector<double> >         CalR_emul_jetsumTrkpT;
   TStlSimpleProxy<vector<double> >         CalR_emul_jetnTrk;
   TStlSimpleProxy<vector<double> >         CalR_emul_minDRTrkpt2;
   TStlSimpleProxy<vector<bool> >           CalR_emul_jetIsCosmic;
   TStlSimpleProxy<vector<bool> >           CalR_emul_jetIsGoodLLP;
   TStlSimpleProxy<vector<double> >         CalR_emul_jetLxy;
   TStlSimpleProxy<vector<double> >         CalR_emul_jetLz;
   TStlSimpleProxy<vector<double> >         CalR_emul_EmulPass;
   TStlSimpleProxy<vector<double> >         CalR_emul_HLTPass;
   TStlSimpleProxy<vector<double> >         CalR_emul_L1Pass;
   TStlSimpleProxy<vector<double> >         CalR_emul_RoImaxET;
   TStlSimpleProxy<vector<bool> >           CalR_emul_EmulPassTAU30;
   TStlSimpleProxy<vector<bool> >           CalR_emul_EmulPassTAU40;
   TStlSimpleProxy<vector<bool> >           CalR_emul_EmulPassTAU60;
   TStlSimpleProxy<vector<bool> >           CalR_emul_EmulPassLLPNOMATCH;
   TStlSimpleProxy<vector<bool> >           CalR_emul_EmulPassnoiso_TAU60;
   TStlSimpleProxy<vector<bool> >           CalR_emul_EmulPassnoiso_LLPNOMATCH;
   TStlSimpleProxy<vector<bool> >           CalR_emul_HLTPassTAU30;
   TStlSimpleProxy<vector<bool> >           CalR_emul_HLTPassTAU40;
   TStlSimpleProxy<vector<bool> >           CalR_emul_HLTPassTAU60;
   TStlSimpleProxy<vector<bool> >           CalR_emul_HLTPassLLPNOMATCH;
   TStlSimpleProxy<vector<bool> >           CalR_emul_HLTPassnoiso_TAU60;
   TStlSimpleProxy<vector<bool> >           CalR_emul_HLTPassnoiso_LLPNOMATCH;
   TStlSimpleProxy<vector<bool> >           CalR_emul_L1PassTAU30;
   TStlSimpleProxy<vector<bool> >           CalR_emul_L1PassTAU40;
   TStlSimpleProxy<vector<bool> >           CalR_emul_L1PassTAU60;
   TStlSimpleProxy<vector<bool> >           CalR_emul_L1PassLLPNOMATCH;
   TStlSimpleProxy<vector<double> >         muRoIClus_Trig_eta;
   TStlSimpleProxy<vector<double> >         muRoIClus_Trig_phi;
   TStlSimpleProxy<vector<int> >            muRoIClus_Trig_nRoI;
   TStlSimpleProxy<vector<int> >            muRoIClus_Trig_nJet;
   TStlSimpleProxy<vector<int> >            muRoIClus_Trig_nTrk;
   TStlSimpleProxy<vector<bool> >           muRoIClus_Trig_offline;
   TStlSimpleProxy<vector<bool> >           muRoIClus_Trig_offline_noiso;
   TStlSimpleProxy<vector<double> >         muRoIClus_Trig_LLP_dR;
   TStlSimpleProxy<vector<int> >            muRoIClus_Trig_indexLLP;
   TStlSimpleProxy<vector<double> >         muRoIClus_Trig_jetdR;
   TStlSimpleProxy<vector<double> >         muRoIClus_Trig_jetET;
   TStlSimpleProxy<vector<double> >         muRoIClus_Trig_jetLogRatio;
   TStlSimpleProxy<vector<double> >         muRoIClus_Trig_trackdR;
   TStlSimpleProxy<vector<double> >         muRoIClus_Trig_trackpT;
   TStlSimpleProxy<vector<double> >         MSVertex_LLP_dR;
   TStlSimpleProxy<vector<int> >            MSVertex_indexLLP;
   TStlSimpleProxy<vector<double> >         MSVertex_eta;
   TStlSimpleProxy<vector<double> >         MSVertex_phi;
   TStlSimpleProxy<vector<double> >         MSVertex_R;
   TStlSimpleProxy<vector<double> >         MSVertex_z;
   TStlSimpleProxy<vector<int> >            MSVertex_nTrks;
   TStlSimpleProxy<vector<int> >            MSVertex_nMDT;
   TStlSimpleProxy<vector<int> >            MSVertex_nRPC;
   TStlSimpleProxy<vector<int> >            MSVertex_nTGC;
   TStlSimpleProxy<vector<int> >            MSVertex_nJetsInCone;
   TStlSimpleProxy<vector<int> >            MSVertex_nTracksInCone;
   TStlSimpleProxy<vector<int> >            MSVertex_sumPtTracksInCone;
   TStlSimpleProxy<vector<int> >            MSVertex_passesJetIso;
   TStlSimpleProxy<vector<int> >            MSVertex_passesTrackIso;
   TStlSimpleProxy<vector<int> >            MSVertex_passesHitThresholds;
   TStlSimpleProxy<vector<int> >            MSVertex_isGood;
   TStlSimpleProxy<vector<double> >         LLP_pdgid;
   TStlSimpleProxy<vector<double> >         LLP_E;
   TStlSimpleProxy<vector<double> >         LLP_pT;
   TStlSimpleProxy<vector<double> >         LLP_eta;
   TStlSimpleProxy<vector<double> >         LLP_phi;
   TStlSimpleProxy<vector<double> >         LLP_beta;
   TStlSimpleProxy<vector<double> >         LLP_gamma;
   TStlSimpleProxy<vector<double> >         LLP_timing;
   TStlSimpleProxy<vector<double> >         LLP_Lxy;
   TStlSimpleProxy<vector<double> >         LLP_Lz;
   TStlSimpleProxy<vector<double> >         LLP_dR_Jet;
   TStlSimpleProxy<vector<int> >            LLP_nJet_dRlt04;
   TStlSimpleProxy<vector<int> >            LLP_nMSegs_dR04;
   TStlSimpleProxy<vector<double> >         TruthParticle_pdgid;
   TStlSimpleProxy<vector<double> >         TruthParticle_E;
   TStlSimpleProxy<vector<double> >         TruthParticle_pT;
   TStlSimpleProxy<vector<double> >         TruthParticle_eta;
   TStlSimpleProxy<vector<double> >         TruthParticle_phi;
   TStlSimpleProxy<vector<double> >         TruthParticle_beta;
   TStlSimpleProxy<vector<double> >         TruthParticle_gamma;


   ntuple_recoTree(TTree *tree=0) : 
      fChain(0),
      htemp(0),
      fDirector(tree,-1),
      fClass                (TClass::GetClass("ntuple_recoTree")),
      event_count                             (&fDirector,"event_count"),
      eventNumber                             (&fDirector,"eventNumber"),
      runNumber                               (&fDirector,"runNumber"),
      backgroundFlags                         (&fDirector,"backgroundFlags"),
      lumiBlock                               (&fDirector,"lumiBlock"),
      eventWeight                             (&fDirector,"eventWeight"),
      actualIntPerCrossing                    (&fDirector,"actualIntPerCrossing"),
      averageIntPerCrossing                   (&fDirector,"averageIntPerCrossing"),
      event_HT                                (&fDirector,"event_HT"),
      event_NJets                             (&fDirector,"event_NJets"),
      event_HTMiss                            (&fDirector,"event_HTMiss"),
      event_Meff                              (&fDirector,"event_Meff"),
      hasGoodPV                               (&fDirector,"hasGoodPV"),
      isGoodLAr                               (&fDirector,"isGoodLAr"),
      isGoodTile                              (&fDirector,"isGoodTile"),
      isGoodSCT                               (&fDirector,"isGoodSCT"),
      isCompleteEvent                         (&fDirector,"isCompleteEvent"),
      jetsPassingCalREmul                     (&fDirector,"jetsPassingCalREmul"),
      dPhiLeadingCalibJets                    (&fDirector,"dPhiLeadingCalibJets"),
      dRLeadingCalibJets                      (&fDirector,"dRLeadingCalibJets"),
      event_tauRoI_maxET                      (&fDirector,"event_tauRoI_maxET"),
      event_passCalRatio_TAU30                (&fDirector,"event_passCalRatio_TAU30"),
      event_passCalRatio_LLPNOMATCH           (&fDirector,"event_passCalRatio_LLPNOMATCH"),
      event_passCalRatio_TAU60                (&fDirector,"event_passCalRatio_TAU60"),
      event_passCalRatio_cleanLLP_TAU60       (&fDirector,"event_passCalRatio_cleanLLP_TAU60"),
      event_passCalRatio_LLPNOMATCH_noiso     (&fDirector,"event_passCalRatio_LLPNOMATCH_noiso"),
      event_passCalRatio_TAU60_noiso          (&fDirector,"event_passCalRatio_TAU60_noiso"),
      event_passCalRatio_cleanLLP_TAU60_noiso (&fDirector,"event_passCalRatio_cleanLLP_TAU60_noiso"),
      event_passMuvtx                         (&fDirector,"event_passMuvtx"),
      event_passMuvtx_noiso                   (&fDirector,"event_passMuvtx_noiso"),
      event_passL1TAU30                       (&fDirector,"event_passL1TAU30"),
      event_passL1TAU40                       (&fDirector,"event_passL1TAU40"),
      event_passL1TAU60                       (&fDirector,"event_passL1TAU60"),
      event_passL1LLPNOMATCH                  (&fDirector,"event_passL1LLPNOMATCH"),
      event_passJ25                           (&fDirector,"event_passJ25"),
      event_passJ100                          (&fDirector,"event_passJ100"),
      event_passJ400                          (&fDirector,"event_passJ400"),
      CalibJet_isGoodStand                    (&fDirector,"CalibJet_isGoodStand"),
      CalibJet_isGoodLLP                      (&fDirector,"CalibJet_isGoodLLP"),
      CalibJet_E                              (&fDirector,"CalibJet_E"),
      CalibJet_ET                             (&fDirector,"CalibJet_ET"),
      CalibJet_pT                             (&fDirector,"CalibJet_pT"),
      CalibJet_eta                            (&fDirector,"CalibJet_eta"),
      CalibJet_phi                            (&fDirector,"CalibJet_phi"),
      CalibJet_width                          (&fDirector,"CalibJet_width"),
      CalibJet_logRatio                       (&fDirector,"CalibJet_logRatio"),
      CalibJet_EMF                            (&fDirector,"CalibJet_EMF"),
      CalibJet_time                           (&fDirector,"CalibJet_time"),
      CalibJet_BDT                            (&fDirector,"CalibJet_BDT"),
      CalibJet_isCRHLTJet                     (&fDirector,"CalibJet_isCRHLTJet"),
      CalibJet_indexLLP                       (&fDirector,"CalibJet_indexLLP"),
      CalibJet_nTrk                           (&fDirector,"CalibJet_nTrk"),
      CalibJet_sumTrkpT                       (&fDirector,"CalibJet_sumTrkpT"),
      CalibJet_maxTrkpT                       (&fDirector,"CalibJet_maxTrkpT"),
      CalibJet_minDRTrkpt2                    (&fDirector,"CalibJet_minDRTrkpt2"),
      CalibJet_matchPassEmul                  (&fDirector,"CalibJet_matchPassEmul"),
      CalibJet_cluster_nClusters              (&fDirector,"CalibJet_cluster_nClusters"),
      CalibJet_cluster_lead_r                 (&fDirector,"CalibJet_cluster_lead_r"),
      CalibJet_cluster_lead_x                 (&fDirector,"CalibJet_cluster_lead_x"),
      CalibJet_cluster_lead_y                 (&fDirector,"CalibJet_cluster_lead_y"),
      CalibJet_cluster_lead_z                 (&fDirector,"CalibJet_cluster_lead_z"),
      CalibJet_cluster_lead_Lxy               (&fDirector,"CalibJet_cluster_lead_Lxy"),
      CalibJet_cluster_lead_ET                (&fDirector,"CalibJet_cluster_lead_ET"),
      CalibJet_cluster_lead_pT                (&fDirector,"CalibJet_cluster_lead_pT"),
      CalibJet_cluster_lead_eta               (&fDirector,"CalibJet_cluster_lead_eta"),
      CalibJet_cluster_lead_phi               (&fDirector,"CalibJet_cluster_lead_phi"),
      CalibJet_cluster_lead_calibET           (&fDirector,"CalibJet_cluster_lead_calibET"),
      CalibJet_cluster_lead_calibEta          (&fDirector,"CalibJet_cluster_lead_calibEta"),
      CalibJet_cluster_lead_calibPhi          (&fDirector,"CalibJet_cluster_lead_calibPhi"),
      CalibJet_cluster_lead_time              (&fDirector,"CalibJet_cluster_lead_time"),
      CalibJet_cluster_lead_lambda            (&fDirector,"CalibJet_cluster_lead_lambda"),
      CalibJet_cluster_lead_sec_r             (&fDirector,"CalibJet_cluster_lead_sec_r"),
      CalibJet_cluster_lead_sec_lambda        (&fDirector,"CalibJet_cluster_lead_sec_lambda"),
      CalibJet_cluster_lead_qual_tile         (&fDirector,"CalibJet_cluster_lead_qual_tile"),
      CalibJet_cluster_lead_qual_lar          (&fDirector,"CalibJet_cluster_lead_qual_lar"),
      CalibJet_cluster_lead_lateral           (&fDirector,"CalibJet_cluster_lead_lateral"),
      CalibJet_cluster_lead_longitudinal      (&fDirector,"CalibJet_cluster_lead_longitudinal"),
      CalibJet_cluster_lead_first_Eden        (&fDirector,"CalibJet_cluster_lead_first_Eden"),
      CalibJet_cluster_lead_EMF               (&fDirector,"CalibJet_cluster_lead_EMF"),
      CalibJet_cluster_lead_EEM               (&fDirector,"CalibJet_cluster_lead_EEM"),
      CalibJet_cluster_lead_Etot              (&fDirector,"CalibJet_cluster_lead_Etot"),
      CalibJet_cluster_lead_dR_tojet          (&fDirector,"CalibJet_cluster_lead_dR_tojet"),
      CalibJet_cluster_lead_fracE_ofjet       (&fDirector,"CalibJet_cluster_lead_fracE_ofjet"),
      CalibJet_cluster_lead_jetEMF_revised    (&fDirector,"CalibJet_cluster_lead_jetEMF_revised"),
      Track_pT                                (&fDirector,"Track_pT"),
      Track_eta                               (&fDirector,"Track_eta"),
      Track_phi                               (&fDirector,"Track_phi"),
      MSTracklet_eta                          (&fDirector,"MSTracklet_eta"),
      MSTracklet_phi                          (&fDirector,"MSTracklet_phi"),
      MSeg_etaPos                             (&fDirector,"MSeg_etaPos"),
      MSeg_thetaPos                           (&fDirector,"MSeg_thetaPos"),
      MSeg_phiPos                             (&fDirector,"MSeg_phiPos"),
      MSeg_thetaDir                           (&fDirector,"MSeg_thetaDir"),
      MSeg_phiDir                             (&fDirector,"MSeg_phiDir"),
      MSeg_t0                                 (&fDirector,"MSeg_t0"),
      MSeg_chIndex                            (&fDirector,"MSeg_chIndex"),
      MSeg_techIndex                          (&fDirector,"MSeg_techIndex"),
      METCST_met                              (&fDirector,"METCST_met"),
      METCST_phi                              (&fDirector,"METCST_phi"),
      METCST_mpx                              (&fDirector,"METCST_mpx"),
      METCST_mpy                              (&fDirector,"METCST_mpy"),
      METCST_sumet                            (&fDirector,"METCST_sumet"),
      METTST_met                              (&fDirector,"METTST_met"),
      METTST_phi                              (&fDirector,"METTST_phi"),
      METTST_mpx                              (&fDirector,"METTST_mpx"),
      METTST_mpy                              (&fDirector,"METTST_mpy"),
      METTST_sumet                            (&fDirector,"METTST_sumet"),
      CR_HLT_ET                               (&fDirector,"CR_HLT_ET"),
      CR_HLT_pT                               (&fDirector,"CR_HLT_pT"),
      CR_HLT_eta                              (&fDirector,"CR_HLT_eta"),
      CR_HLT_phi                              (&fDirector,"CR_HLT_phi"),
      CR_HLT_EMF                              (&fDirector,"CR_HLT_EMF"),
      CR_HLT_logR                             (&fDirector,"CR_HLT_logR"),
      CR_HLT_minDR_HLTtracks                  (&fDirector,"CR_HLT_minDR_HLTtracks"),
      CR_HLT_dR                               (&fDirector,"CR_HLT_dR"),
      CR_HLT_matchOff                         (&fDirector,"CR_HLT_matchOff"),
      CR_HLT_LLP_DR                           (&fDirector,"CR_HLT_LLP_DR"),
      CR_HLT_LLP_Lxy                          (&fDirector,"CR_HLT_LLP_Lxy"),
      CR_HLT_LLP_Lz                           (&fDirector,"CR_HLT_LLP_Lz"),
      CR_HLT_NegativeE                        (&fDirector,"CR_HLT_NegativeE"),
      CR_HLT_FracSamplingMax                  (&fDirector,"CR_HLT_FracSamplingMax"),
      CR_HLT_HECQuality                       (&fDirector,"CR_HLT_HECQuality"),
      CR_HLT_LArQuality                       (&fDirector,"CR_HLT_LArQuality"),
      CR_HLT_AverageLArQF                     (&fDirector,"CR_HLT_AverageLArQF"),
      CR_HLT_HECFrac                          (&fDirector,"CR_HLT_HECFrac"),
      CR_HLT_Timing                           (&fDirector,"CR_HLT_Timing"),
      CR_offline_HLTassoc                     (&fDirector,"CR_offline_HLTassoc"),
      CR_offline_eta                          (&fDirector,"CR_offline_eta"),
      CR_offline_phi                          (&fDirector,"CR_offline_phi"),
      CR_offline_ET                           (&fDirector,"CR_offline_ET"),
      CR_offline_pT                           (&fDirector,"CR_offline_pT"),
      CR_offline_EMF                          (&fDirector,"CR_offline_EMF"),
      CR_offline_logR                         (&fDirector,"CR_offline_logR"),
      CR_offline_nTrks                        (&fDirector,"CR_offline_nTrks"),
      CR_offline_timing                       (&fDirector,"CR_offline_timing"),
      CR_offline_matchLLP                     (&fDirector,"CR_offline_matchLLP"),
      CR_offline_LLP_DR                       (&fDirector,"CR_offline_LLP_DR"),
      CR_offline_LLP_Lxy                      (&fDirector,"CR_offline_LLP_Lxy"),
      CR_offline_LLP_Lz                       (&fDirector,"CR_offline_LLP_Lz"),
      CR_offline_passCuts                     (&fDirector,"CR_offline_passCuts"),
      CR_offline_jetindex                     (&fDirector,"CR_offline_jetindex"),
      CR_offline_goodjet_stand                (&fDirector,"CR_offline_goodjet_stand"),
      CR_offline_goodjet_llp                  (&fDirector,"CR_offline_goodjet_llp"),
      CR_offline_BDT                          (&fDirector,"CR_offline_BDT"),
      CalR_emul_jetET                         (&fDirector,"CalR_emul_jetET"),
      CalR_emul_jetPt                         (&fDirector,"CalR_emul_jetPt"),
      CalR_emul_jetEta                        (&fDirector,"CalR_emul_jetEta"),
      CalR_emul_jetPhi                        (&fDirector,"CalR_emul_jetPhi"),
      CalR_emul_jetEMF                        (&fDirector,"CalR_emul_jetEMF"),
      CalR_emul_jetLogR                       (&fDirector,"CalR_emul_jetLogR"),
      CalR_emul_jetTime                       (&fDirector,"CalR_emul_jetTime"),
      CalR_emul_jetsumTrkpT                   (&fDirector,"CalR_emul_jetsumTrkpT"),
      CalR_emul_jetnTrk                       (&fDirector,"CalR_emul_jetnTrk"),
      CalR_emul_minDRTrkpt2                   (&fDirector,"CalR_emul_minDRTrkpt2"),
      CalR_emul_jetIsCosmic                   (&fDirector,"CalR_emul_jetIsCosmic"),
      CalR_emul_jetIsGoodLLP                  (&fDirector,"CalR_emul_jetIsGoodLLP"),
      CalR_emul_jetLxy                        (&fDirector,"CalR_emul_jetLxy"),
      CalR_emul_jetLz                         (&fDirector,"CalR_emul_jetLz"),
      CalR_emul_EmulPass                      (&fDirector,"CalR_emul_EmulPass"),
      CalR_emul_HLTPass                       (&fDirector,"CalR_emul_HLTPass"),
      CalR_emul_L1Pass                        (&fDirector,"CalR_emul_L1Pass"),
      CalR_emul_RoImaxET                      (&fDirector,"CalR_emul_RoImaxET"),
      CalR_emul_EmulPassTAU30                 (&fDirector,"CalR_emul_EmulPassTAU30"),
      CalR_emul_EmulPassTAU40                 (&fDirector,"CalR_emul_EmulPassTAU40"),
      CalR_emul_EmulPassTAU60                 (&fDirector,"CalR_emul_EmulPassTAU60"),
      CalR_emul_EmulPassLLPNOMATCH            (&fDirector,"CalR_emul_EmulPassLLPNOMATCH"),
      CalR_emul_EmulPassnoiso_TAU60           (&fDirector,"CalR_emul_EmulPassnoiso_TAU60"),
      CalR_emul_EmulPassnoiso_LLPNOMATCH      (&fDirector,"CalR_emul_EmulPassnoiso_LLPNOMATCH"),
      CalR_emul_HLTPassTAU30                  (&fDirector,"CalR_emul_HLTPassTAU30"),
      CalR_emul_HLTPassTAU40                  (&fDirector,"CalR_emul_HLTPassTAU40"),
      CalR_emul_HLTPassTAU60                  (&fDirector,"CalR_emul_HLTPassTAU60"),
      CalR_emul_HLTPassLLPNOMATCH             (&fDirector,"CalR_emul_HLTPassLLPNOMATCH"),
      CalR_emul_HLTPassnoiso_TAU60            (&fDirector,"CalR_emul_HLTPassnoiso_TAU60"),
      CalR_emul_HLTPassnoiso_LLPNOMATCH       (&fDirector,"CalR_emul_HLTPassnoiso_LLPNOMATCH"),
      CalR_emul_L1PassTAU30                   (&fDirector,"CalR_emul_L1PassTAU30"),
      CalR_emul_L1PassTAU40                   (&fDirector,"CalR_emul_L1PassTAU40"),
      CalR_emul_L1PassTAU60                   (&fDirector,"CalR_emul_L1PassTAU60"),
      CalR_emul_L1PassLLPNOMATCH              (&fDirector,"CalR_emul_L1PassLLPNOMATCH"),
      muRoIClus_Trig_eta                      (&fDirector,"muRoIClus_Trig_eta"),
      muRoIClus_Trig_phi                      (&fDirector,"muRoIClus_Trig_phi"),
      muRoIClus_Trig_nRoI                     (&fDirector,"muRoIClus_Trig_nRoI"),
      muRoIClus_Trig_nJet                     (&fDirector,"muRoIClus_Trig_nJet"),
      muRoIClus_Trig_nTrk                     (&fDirector,"muRoIClus_Trig_nTrk"),
      muRoIClus_Trig_offline                  (&fDirector,"muRoIClus_Trig_offline"),
      muRoIClus_Trig_offline_noiso            (&fDirector,"muRoIClus_Trig_offline_noiso"),
      muRoIClus_Trig_LLP_dR                   (&fDirector,"muRoIClus_Trig_LLP_dR"),
      muRoIClus_Trig_indexLLP                 (&fDirector,"muRoIClus_Trig_indexLLP"),
      muRoIClus_Trig_jetdR                    (&fDirector,"muRoIClus_Trig_jetdR"),
      muRoIClus_Trig_jetET                    (&fDirector,"muRoIClus_Trig_jetET"),
      muRoIClus_Trig_jetLogRatio              (&fDirector,"muRoIClus_Trig_jetLogRatio"),
      muRoIClus_Trig_trackdR                  (&fDirector,"muRoIClus_Trig_trackdR"),
      muRoIClus_Trig_trackpT                  (&fDirector,"muRoIClus_Trig_trackpT"),
      MSVertex_LLP_dR                         (&fDirector,"MSVertex_LLP_dR"),
      MSVertex_indexLLP                       (&fDirector,"MSVertex_indexLLP"),
      MSVertex_eta                            (&fDirector,"MSVertex_eta"),
      MSVertex_phi                            (&fDirector,"MSVertex_phi"),
      MSVertex_R                              (&fDirector,"MSVertex_R"),
      MSVertex_z                              (&fDirector,"MSVertex_z"),
      MSVertex_nTrks                          (&fDirector,"MSVertex_nTrks"),
      MSVertex_nMDT                           (&fDirector,"MSVertex_nMDT"),
      MSVertex_nRPC                           (&fDirector,"MSVertex_nRPC"),
      MSVertex_nTGC                           (&fDirector,"MSVertex_nTGC"),
      MSVertex_nJetsInCone                    (&fDirector,"MSVertex_nJetsInCone"),
      MSVertex_nTracksInCone                  (&fDirector,"MSVertex_nTracksInCone"),
      MSVertex_sumPtTracksInCone              (&fDirector,"MSVertex_sumPtTracksInCone"),
      MSVertex_passesJetIso                   (&fDirector,"MSVertex_passesJetIso"),
      MSVertex_passesTrackIso                 (&fDirector,"MSVertex_passesTrackIso"),
      MSVertex_passesHitThresholds            (&fDirector,"MSVertex_passesHitThresholds"),
      MSVertex_isGood                         (&fDirector,"MSVertex_isGood"),
      LLP_pdgid                               (&fDirector,"LLP_pdgid"),
      LLP_E                                   (&fDirector,"LLP_E"),
      LLP_pT                                  (&fDirector,"LLP_pT"),
      LLP_eta                                 (&fDirector,"LLP_eta"),
      LLP_phi                                 (&fDirector,"LLP_phi"),
      LLP_beta                                (&fDirector,"LLP_beta"),
      LLP_gamma                               (&fDirector,"LLP_gamma"),
      LLP_timing                              (&fDirector,"LLP_timing"),
      LLP_Lxy                                 (&fDirector,"LLP_Lxy"),
      LLP_Lz                                  (&fDirector,"LLP_Lz"),
      LLP_dR_Jet                              (&fDirector,"LLP_dR_Jet"),
      LLP_nJet_dRlt04                         (&fDirector,"LLP_nJet_dRlt04"),
      LLP_nMSegs_dR04                         (&fDirector,"LLP_nMSegs_dR04"),
      TruthParticle_pdgid                     (&fDirector,"TruthParticle_pdgid"),
      TruthParticle_E                         (&fDirector,"TruthParticle_E"),
      TruthParticle_pT                        (&fDirector,"TruthParticle_pT"),
      TruthParticle_eta                       (&fDirector,"TruthParticle_eta"),
      TruthParticle_phi                       (&fDirector,"TruthParticle_phi"),
      TruthParticle_beta                      (&fDirector,"TruthParticle_beta"),
      TruthParticle_gamma                     (&fDirector,"TruthParticle_gamma")
      { }
   ~ntuple_recoTree();
   Int_t   Version() const {return 1;}
   void    Begin(::TTree *tree);
   void    SlaveBegin(::TTree *tree);
   void    Init(::TTree *tree);
   Bool_t  Notify();
   Bool_t  Process(Long64_t entry);
   void    SlaveTerminate();
   void    Terminate();

   ClassDef(ntuple_recoTree,0);


//inject the user's code
#include "junk_macro_parsettree_recoTree.C"
};

#endif


#ifdef __MAKECINT__
#pragma link C++ class ntuple_recoTree;
#endif


inline ntuple_recoTree::~ntuple_recoTree() {
   // destructor. Clean up helpers.

}

inline void ntuple_recoTree::Init(TTree *tree)
{
//   Set branch addresses
   if (tree == 0) return;
   fChain = tree;
   fDirector.SetTree(fChain);
   if (htemp == 0) {
      htemp = fDirector.CreateHistogram(GetOption());
      htemp->SetTitle("junk_macro_parsettree_recoTree.C");
      fObject = htemp;
   }
}

Bool_t ntuple_recoTree::Notify()
{
   // Called when loading a new file.
   // Get branch pointers.
   fDirector.SetTree(fChain);
   junk_macro_parsettree_recoTree_Notify();
   
   return kTRUE;
}
   

inline void ntuple_recoTree::Begin(TTree *tree)
{
   // The Begin() function is called at the start of the query.
   // When running with PROOF Begin() is only called on the client.
   // The tree argument is deprecated (on PROOF 0 is passed).

   TString option = GetOption();
   junk_macro_parsettree_recoTree_Begin(tree);

}

inline void ntuple_recoTree::SlaveBegin(TTree *tree)
{
   // The SlaveBegin() function is called after the Begin() function.
   // When running with PROOF SlaveBegin() is called on each slave server.
   // The tree argument is deprecated (on PROOF 0 is passed).

   Init(tree);

   junk_macro_parsettree_recoTree_SlaveBegin(tree);

}

inline Bool_t ntuple_recoTree::Process(Long64_t entry)
{
   // The Process() function is called for each entry in the tree (or possibly
   // keyed object in the case of PROOF) to be processed. The entry argument
   // specifies which entry in the currently loaded tree is to be processed.
   // It can be passed to either TTree::GetEntry() or TBranch::GetEntry()
   // to read either all or the required parts of the data. When processing
   // keyed objects with PROOF, the object is already loaded and is available
   // via the fObject pointer.
   //
   // This function should contain the "body" of the analysis. It can contain
   // simple or elaborate selection criteria, run algorithms on the data
   // of the event and typically fill histograms.

   // WARNING when a selector is used with a TChain, you must use
   //  the pointer to the current TTree to call GetEntry(entry).
   //  The entry is always the local entry number in the current tree.
   //  Assuming that fChain is the pointer to the TChain being processed,
   //  use fChain->GetTree()->GetEntry(entry).


   fDirector.SetReadEntry(entry);
   junk_macro_parsettree_recoTree();
   junk_macro_parsettree_recoTree_Process(entry);
   return kTRUE;

}

inline void ntuple_recoTree::SlaveTerminate()
{
   // The SlaveTerminate() function is called after all entries or objects
   // have been processed. When running with PROOF SlaveTerminate() is called
   // on each slave server.
   junk_macro_parsettree_recoTree_SlaveTerminate();
}

inline void ntuple_recoTree::Terminate()
{
   // Function called at the end of the event loop.
   htemp = (TH1*)fObject;
   Int_t drawflag = (htemp && htemp->GetEntries()>0);
   
   if (gPad && !drawflag && !fOption.Contains("goff") && !fOption.Contains("same")) {
      gPad->Clear();
   } else {
      if (fOption.Contains("goff")) drawflag = false;
      if (drawflag) htemp->Draw(fOption);
   }
   junk_macro_parsettree_recoTree_Terminate();
}
