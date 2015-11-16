/////////////////////////////////////////////////////////////////////////
//   This class has been automatically generated 
//   (at Sun Nov 15 21:20:09 2015 by ROOT version 5.34/34)
//   from TTree recoTree/recoTree
//   found on file: E:\GRIDDS\user.gwatts.301303.HSS_mH125mS15.reco.s2698_r7144_EXT2.DiVertAnalysis_v3_1381F1A5_hist\user.gwatts\user.gwatts.7001349._000001.hist-output.root
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
   TIntProxy                                lumiBlock;
   TDoubleProxy                             eventWeight;
   TIntProxy                                actualIntPerCrossing;
   TIntProxy                                averageIntPerCrossing;
   TBoolProxy                               event_passCalRatio_TAU30;
   TBoolProxy                               event_passCalRatio_LLPNOMATCH;
   TBoolProxy                               event_passCalRatio_TAU60;
   TBoolProxy                               event_passCalRatio_LLPNOMATCH_noiso;
   TBoolProxy                               event_passCalRatio_TAU60_noiso;
   TBoolProxy                               event_passMuvtx;
   TBoolProxy                               event_passMuvtx_noiso;
   TBoolProxy                               event_passL1TAU30;
   TBoolProxy                               event_passL1TAU40;
   TBoolProxy                               event_passL1TAU60;
   TBoolProxy                               event_passL1LLPNOMATCH;
   TBoolProxy                               event_passJ100;
   TBoolProxy                               event_passJ400;
   TStlSimpleProxy<vector<bool> >           Jet_isGoodStand;
   TStlSimpleProxy<vector<bool> >           Jet_isGoodLLP;
   TStlSimpleProxy<vector<double> >         Jet_E;
   TStlSimpleProxy<vector<double> >         Jet_ET;
   TStlSimpleProxy<vector<double> >         Jet_pT;
   TStlSimpleProxy<vector<double> >         Jet_eta;
   TStlSimpleProxy<vector<double> >         Jet_phi;
   TStlSimpleProxy<vector<double> >         Jet_width;
   TStlSimpleProxy<vector<double> >         Jet_logRatio;
   TStlSimpleProxy<vector<unsigned int> >   Jet_nTrk;
   TStlSimpleProxy<vector<double> >         Jet_time;
   TStlSimpleProxy<vector<bool> >           Jet_isCosmic;
   TStlSimpleProxy<vector<bool> >           Jet_isCRJet;
   TStlSimpleProxy<vector<int> >            Jet_indexLLP;
   TStlSimpleProxy<vector<double> >         Jet_px;
   TStlSimpleProxy<vector<double> >         Jet_py;
   TStlSimpleProxy<vector<double> >         Jet_pz;
   TStlSimpleProxy<vector<double> >         Jet_sumpT;
   TStlSimpleProxy<vector<double> >         Jet_sumpT_PV;
   TStlSimpleProxy<vector<double> >         Jet_FMax;
   TStlSimpleProxy<vector<double> >         Jet_jvt;
   TStlSimpleProxy<vector<double> >         Jet_EEM_Bar;
   TStlSimpleProxy<vector<double> >         Jet_EEM_End;
   TStlSimpleProxy<vector<double> >         Jet_EEM_Extra;
   TStlSimpleProxy<vector<double> >         Jet_EH_Bar;
   TStlSimpleProxy<vector<double> >         Jet_EH_End;
   TStlSimpleProxy<vector<double> >         Jet_EH_Extra;
   TStlSimpleProxy<vector<double> >         Jet_muon_pT;
   TStlSimpleProxy<vector<double> >         Jet_muon_z0;
   TStlSimpleProxy<vector<double> >         Jet_muon_d0;
   TStlSimpleProxy<vector<double> >         Jet_muon_dR;
   TStlSimpleProxy<vector<bool> >           CalibJet_isGoodStand;
   TStlSimpleProxy<vector<bool> >           CalibJet_isGoodLLP;
   TStlSimpleProxy<vector<double> >         CalibJet_E;
   TStlSimpleProxy<vector<double> >         CalibJet_ET;
   TStlSimpleProxy<vector<double> >         CalibJet_pT;
   TStlSimpleProxy<vector<double> >         CalibJet_eta;
   TStlSimpleProxy<vector<double> >         CalibJet_phi;
   TStlSimpleProxy<vector<double> >         CalibJet_width;
   TStlSimpleProxy<vector<double> >         CalibJet_logRatio;
   TStlSimpleProxy<vector<unsigned int> >   CalibJet_nTrk;
   TStlSimpleProxy<vector<double> >         CalibJet_time;
   TStlSimpleProxy<vector<bool> >           CalibJet_isCosmic;
   TStlSimpleProxy<vector<bool> >           CalibJet_isCRJet;
   TStlSimpleProxy<vector<int> >            CalibJet_indexLLP;
   TStlSimpleProxy<vector<double> >         CalibJet_px;
   TStlSimpleProxy<vector<double> >         CalibJet_py;
   TStlSimpleProxy<vector<double> >         CalibJet_pz;
   TStlSimpleProxy<vector<double> >         CalibJet_sumpT;
   TStlSimpleProxy<vector<double> >         CalibJet_sumpT_PV;
   TStlSimpleProxy<vector<double> >         CalibJet_FMax;
   TStlSimpleProxy<vector<double> >         CalibJet_jvt;
   TStlSimpleProxy<vector<double> >         CalibJet_EEM_Bar;
   TStlSimpleProxy<vector<double> >         CalibJet_EEM_End;
   TStlSimpleProxy<vector<double> >         CalibJet_EEM_Extra;
   TStlSimpleProxy<vector<double> >         CalibJet_EH_Bar;
   TStlSimpleProxy<vector<double> >         CalibJet_EH_End;
   TStlSimpleProxy<vector<double> >         CalibJet_EH_Extra;
   TStlSimpleProxy<vector<double> >         CalibJet_muon_pT;
   TStlSimpleProxy<vector<double> >         CalibJet_muon_z0;
   TStlSimpleProxy<vector<double> >         CalibJet_muon_d0;
   TStlSimpleProxy<vector<double> >         CalibJet_muon_dR;
   TStlSimpleProxy<vector<double> >         Track_pT;
   TStlSimpleProxy<vector<double> >         Track_eta;
   TStlSimpleProxy<vector<double> >         Track_phi;
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
   TStlSimpleProxy<vector<double> >         CR_Trig_dR;
   TStlSimpleProxy<vector<double> >         CR_Trig_eta;
   TStlSimpleProxy<vector<double> >         CR_Trig_phi;
   TStlSimpleProxy<vector<double> >         CR_Trig_ET;
   TStlSimpleProxy<vector<double> >         CR_Trig_pT;
   TStlSimpleProxy<vector<double> >         CR_Trig_EMF;
   TStlSimpleProxy<vector<double> >         CR_Trig_logR;
   TStlSimpleProxy<vector<double> >         CR_Trig_nTrks;
   TStlSimpleProxy<vector<double> >         CR_Trig_timing;
   TStlSimpleProxy<vector<bool> >           CR_Trig_passCuts;
   TStlSimpleProxy<vector<bool> >           CR_Trig_jetMatch;
   TStlSimpleProxy<vector<bool> >           CR_Trig_isGoodLLP;
   TStlSimpleProxy<vector<double> >         CR_Trig_trigET;
   TStlSimpleProxy<vector<double> >         CR_Trig_trigpT;
   TStlSimpleProxy<vector<double> >         CR_Trig_trigEta;
   TStlSimpleProxy<vector<double> >         CR_Trig_trigPhi;
   TStlSimpleProxy<vector<double> >         CR_Trig_trigEMF;
   TStlSimpleProxy<vector<double> >         CR_Trig_triglogR;
   TStlSimpleProxy<vector<double> >         CR_Trig_minDR_HLTjet_HLTtracks;
   TStlSimpleProxy<vector<bool> >           CR_Trig_trigMatchLLP;
   TStlSimpleProxy<vector<double> >         CR_Trig_trig_LLP_DR;
   TStlSimpleProxy<vector<double> >         CR_Trig_LLP_Lxy;
   TStlSimpleProxy<vector<double> >         CR_Trig_LLP_Lz;
   TStlSimpleProxy<vector<double> >         CR_noTrig_eta;
   TStlSimpleProxy<vector<double> >         CR_noTrig_phi;
   TStlSimpleProxy<vector<double> >         CR_noTrig_ET;
   TStlSimpleProxy<vector<double> >         CR_noTrig_pT;
   TStlSimpleProxy<vector<double> >         CR_noTrig_EMF;
   TStlSimpleProxy<vector<double> >         CR_noTrig_nTrks;
   TStlSimpleProxy<vector<double> >         CR_noTrig_timing;
   TStlSimpleProxy<vector<bool> >           CR_noTrig_passCuts;
   TStlSimpleProxy<vector<bool> >           CR_noTrig_jetMatchLLP;
   TStlSimpleProxy<vector<double> >         CR_noTrig_LLP_Lxy;
   TStlSimpleProxy<vector<double> >         CR_noTrig_LLP_Lz;
   TStlSimpleProxy<vector<double> >         muRoIClus_Trig_eta;
   TStlSimpleProxy<vector<double> >         muRoIClus_Trig_phi;
   TStlSimpleProxy<vector<int> >            muRoIClus_Trig_nRoI;
   TStlSimpleProxy<vector<int> >            muRoIClus_Trig_nJet;
   TStlSimpleProxy<vector<int> >            muRoIClus_Trig_nTrk;
   TBoolProxy                               muRoIClus_Trig_trig;
   TStlSimpleProxy<vector<bool> >           muRoIClus_Trig_offline;
   TStlSimpleProxy<vector<bool> >           muRoIClus_Trig_offline_noiso;
   TStlSimpleProxy<vector<double> >         muRoIClus_Trig_LLP_Lxy;
   TStlSimpleProxy<vector<double> >         muRoIClus_Trig_LLP_Lz;
   TStlSimpleProxy<vector<double> >         muRoIClus_Trig_LLP_dR;
   TStlSimpleProxy<vector<bool> >           muRoIClus_Trig_trigMatchLLP;
   TStlSimpleProxy<vector<double> >         muRoIClus_Trig_trackdR;
   TStlSimpleProxy<vector<double> >         muRoIClus_Trig_trackPt;
   TStlSimpleProxy<vector<double> >         muRoIClus_Trig_jetdR;
   TStlSimpleProxy<vector<double> >         muRoIClus_Trig_jetET;
   TStlSimpleProxy<vector<double> >         muRoIClus_Trig_jetLogRatio;
   TBoolProxy                               muRoIClus_Trig_trig_noiso;
   TStlSimpleProxy<vector<double> >         muRoIClus_Trig_LLP_eta;
   TStlSimpleProxy<vector<double> >         muRoIClus_Trig_LLP_phi;
   TStlSimpleProxy<vector<double> >         muRoIClus_Trig_LLP_beta;
   TStlSimpleProxy<vector<double> >         muRoIClus_Trig_LLP_pT;
   TStlSimpleProxy<vector<double> >         MSVertex_LLP_eta;
   TStlSimpleProxy<vector<double> >         MSVertex_LLP_phi;
   TStlSimpleProxy<vector<double> >         MSVertex_LLP_beta;
   TStlSimpleProxy<vector<double> >         MSVertex_LLP_pT;
   TStlSimpleProxy<vector<double> >         MSVertex_LLP_Lxy;
   TStlSimpleProxy<vector<double> >         MSVertex_LLP_Lz;
   TStlSimpleProxy<vector<double> >         MSVertex_LLP_dR;
   TStlSimpleProxy<vector<int> >            MSVertex_llpMatch;
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
   TStlSimpleProxy<vector<double> >         LLP_E;
   TStlSimpleProxy<vector<double> >         LLP_pT;
   TStlSimpleProxy<vector<double> >         LLP_eta;
   TStlSimpleProxy<vector<double> >         LLP_phi;
   TStlSimpleProxy<vector<double> >         LLP_beta;
   TStlSimpleProxy<vector<double> >         LLP_timing;
   TStlSimpleProxy<vector<double> >         LLP_Lxy;
   TStlSimpleProxy<vector<double> >         LLP_Lz;
   TStlSimpleProxy<vector<double> >         LLP_dR_Jet;
   TStlSimpleProxy<vector<int> >            LLP_nJet_dRlt04;
   TStlSimpleProxy<vector<bool> >           DiLeptonFinder_eventTriggers;
   TDoubleProxy                             DiLeptonFinder_leadingElectron_E;
   TDoubleProxy                             DiLeptonFinder_leadingElectron_pT;
   TDoubleProxy                             DiLeptonFinder_leadingElectron_eta;
   TDoubleProxy                             DiLeptonFinder_leadingElectron_phi;
   TShortProxy                              DiLeptonFinder_leadingElectron_charge;
   TFloatProxy                              DiLeptonFinder_leadingElectron_pTcone20;
   TBoolProxy                               DiLeptonFinder_leadingElectron_passCuts;
   TStlSimpleProxy<vector<bool> >           DiLeptonFinder_leadingElectron_passTriggers;
   TDoubleProxy                             DiLeptonFinder_secondLeadingElectron_E;
   TDoubleProxy                             DiLeptonFinder_secondLeadingElectron_pT;
   TDoubleProxy                             DiLeptonFinder_secondLeadingElectron_eta;
   TDoubleProxy                             DiLeptonFinder_secondLeadingElectron_phi;
   TShortProxy                              DiLeptonFinder_secondLeadingElectron_charge;
   TFloatProxy                              DiLeptonFinder_secondLeadingElectron_pTcone20;
   TBoolProxy                               DiLeptonFinder_secondLeadingElectron_passCuts;
   TStlSimpleProxy<vector<bool> >           DiLeptonFinder_secondLeadingElectron_passTriggers;
   TDoubleProxy                             DiLeptonFinder_dielectron_combinedMass;
   TIntProxy                                DiLeptonFinder_n_electron_candidates;
   TDoubleProxy                             DiLeptonFinder_leadingMuon_E;
   TDoubleProxy                             DiLeptonFinder_leadingMuon_pT;
   TDoubleProxy                             DiLeptonFinder_leadingMuon_eta;
   TDoubleProxy                             DiLeptonFinder_leadingMuon_phi;
   TShortProxy                              DiLeptonFinder_leadingMuon_charge;
   TFloatProxy                              DiLeptonFinder_leadingMuon_pTcone20;
   TBoolProxy                               DiLeptonFinder_leadingMuon_passCuts;
   TStlSimpleProxy<vector<bool> >           DiLeptonFinder_leadingMuon_passTriggers;
   TDoubleProxy                             DiLeptonFinder_secondLeadingMuon_E;
   TDoubleProxy                             DiLeptonFinder_secondLeadingMuon_pT;
   TDoubleProxy                             DiLeptonFinder_secondLeadingMuon_eta;
   TDoubleProxy                             DiLeptonFinder_secondLeadingMuon_phi;
   TShortProxy                              DiLeptonFinder_secondLeadingMuon_charge;
   TFloatProxy                              DiLeptonFinder_secondLeadingMuon_pTcone20;
   TBoolProxy                               DiLeptonFinder_secondLeadingMuon_passCuts;
   TStlSimpleProxy<vector<bool> >           DiLeptonFinder_secondLeadingMuon_passTriggers;
   TDoubleProxy                             DiLeptonFinder_dimuon_combinedMass;
   TIntProxy                                DiLeptonFinder_n_muon_candidates;
   TDoubleProxy                             DiLeptonFinder_MET_e;
   TDoubleProxy                             DiLeptonFinder_MET_px;
   TDoubleProxy                             DiLeptonFinder_MET_py;
   TDoubleProxy                             DiLeptonFinder_MET_phi;


   ntuple_recoTree(TTree *tree=0) : 
      fChain(0),
      htemp(0),
      fDirector(tree,-1),
      fClass                (TClass::GetClass("ntuple_recoTree")),
      event_count                             (&fDirector,"event_count"),
      eventNumber                             (&fDirector,"eventNumber"),
      runNumber                               (&fDirector,"runNumber"),
      lumiBlock                               (&fDirector,"lumiBlock"),
      eventWeight                             (&fDirector,"eventWeight"),
      actualIntPerCrossing                    (&fDirector,"actualIntPerCrossing"),
      averageIntPerCrossing                   (&fDirector,"averageIntPerCrossing"),
      event_passCalRatio_TAU30                (&fDirector,"event_passCalRatio_TAU30"),
      event_passCalRatio_LLPNOMATCH           (&fDirector,"event_passCalRatio_LLPNOMATCH"),
      event_passCalRatio_TAU60                (&fDirector,"event_passCalRatio_TAU60"),
      event_passCalRatio_LLPNOMATCH_noiso     (&fDirector,"event_passCalRatio_LLPNOMATCH_noiso"),
      event_passCalRatio_TAU60_noiso          (&fDirector,"event_passCalRatio_TAU60_noiso"),
      event_passMuvtx                         (&fDirector,"event_passMuvtx"),
      event_passMuvtx_noiso                   (&fDirector,"event_passMuvtx_noiso"),
      event_passL1TAU30                       (&fDirector,"event_passL1TAU30"),
      event_passL1TAU40                       (&fDirector,"event_passL1TAU40"),
      event_passL1TAU60                       (&fDirector,"event_passL1TAU60"),
      event_passL1LLPNOMATCH                  (&fDirector,"event_passL1LLPNOMATCH"),
      event_passJ100                          (&fDirector,"event_passJ100"),
      event_passJ400                          (&fDirector,"event_passJ400"),
      Jet_isGoodStand                         (&fDirector,"Jet_isGoodStand"),
      Jet_isGoodLLP                           (&fDirector,"Jet_isGoodLLP"),
      Jet_E                                   (&fDirector,"Jet_E"),
      Jet_ET                                  (&fDirector,"Jet_ET"),
      Jet_pT                                  (&fDirector,"Jet_pT"),
      Jet_eta                                 (&fDirector,"Jet_eta"),
      Jet_phi                                 (&fDirector,"Jet_phi"),
      Jet_width                               (&fDirector,"Jet_width"),
      Jet_logRatio                            (&fDirector,"Jet_logRatio"),
      Jet_nTrk                                (&fDirector,"Jet_nTrk"),
      Jet_time                                (&fDirector,"Jet_time"),
      Jet_isCosmic                            (&fDirector,"Jet_isCosmic"),
      Jet_isCRJet                             (&fDirector,"Jet_isCRJet"),
      Jet_indexLLP                            (&fDirector,"Jet_indexLLP"),
      Jet_px                                  (&fDirector,"Jet_px"),
      Jet_py                                  (&fDirector,"Jet_py"),
      Jet_pz                                  (&fDirector,"Jet_pz"),
      Jet_sumpT                               (&fDirector,"Jet_sumpT"),
      Jet_sumpT_PV                            (&fDirector,"Jet_sumpT_PV"),
      Jet_FMax                                (&fDirector,"Jet_FMax"),
      Jet_jvt                                 (&fDirector,"Jet_jvt"),
      Jet_EEM_Bar                             (&fDirector,"Jet_EEM_Bar"),
      Jet_EEM_End                             (&fDirector,"Jet_EEM_End"),
      Jet_EEM_Extra                           (&fDirector,"Jet_EEM_Extra"),
      Jet_EH_Bar                              (&fDirector,"Jet_EH_Bar"),
      Jet_EH_End                              (&fDirector,"Jet_EH_End"),
      Jet_EH_Extra                            (&fDirector,"Jet_EH_Extra"),
      Jet_muon_pT                             (&fDirector,"Jet_muon_pT"),
      Jet_muon_z0                             (&fDirector,"Jet_muon_z0"),
      Jet_muon_d0                             (&fDirector,"Jet_muon_d0"),
      Jet_muon_dR                             (&fDirector,"Jet_muon_dR"),
      CalibJet_isGoodStand                    (&fDirector,"CalibJet_isGoodStand"),
      CalibJet_isGoodLLP                      (&fDirector,"CalibJet_isGoodLLP"),
      CalibJet_E                              (&fDirector,"CalibJet_E"),
      CalibJet_ET                             (&fDirector,"CalibJet_ET"),
      CalibJet_pT                             (&fDirector,"CalibJet_pT"),
      CalibJet_eta                            (&fDirector,"CalibJet_eta"),
      CalibJet_phi                            (&fDirector,"CalibJet_phi"),
      CalibJet_width                          (&fDirector,"CalibJet_width"),
      CalibJet_logRatio                       (&fDirector,"CalibJet_logRatio"),
      CalibJet_nTrk                           (&fDirector,"CalibJet_nTrk"),
      CalibJet_time                           (&fDirector,"CalibJet_time"),
      CalibJet_isCosmic                       (&fDirector,"CalibJet_isCosmic"),
      CalibJet_isCRJet                        (&fDirector,"CalibJet_isCRJet"),
      CalibJet_indexLLP                       (&fDirector,"CalibJet_indexLLP"),
      CalibJet_px                             (&fDirector,"CalibJet_px"),
      CalibJet_py                             (&fDirector,"CalibJet_py"),
      CalibJet_pz                             (&fDirector,"CalibJet_pz"),
      CalibJet_sumpT                          (&fDirector,"CalibJet_sumpT"),
      CalibJet_sumpT_PV                       (&fDirector,"CalibJet_sumpT_PV"),
      CalibJet_FMax                           (&fDirector,"CalibJet_FMax"),
      CalibJet_jvt                            (&fDirector,"CalibJet_jvt"),
      CalibJet_EEM_Bar                        (&fDirector,"CalibJet_EEM_Bar"),
      CalibJet_EEM_End                        (&fDirector,"CalibJet_EEM_End"),
      CalibJet_EEM_Extra                      (&fDirector,"CalibJet_EEM_Extra"),
      CalibJet_EH_Bar                         (&fDirector,"CalibJet_EH_Bar"),
      CalibJet_EH_End                         (&fDirector,"CalibJet_EH_End"),
      CalibJet_EH_Extra                       (&fDirector,"CalibJet_EH_Extra"),
      CalibJet_muon_pT                        (&fDirector,"CalibJet_muon_pT"),
      CalibJet_muon_z0                        (&fDirector,"CalibJet_muon_z0"),
      CalibJet_muon_d0                        (&fDirector,"CalibJet_muon_d0"),
      CalibJet_muon_dR                        (&fDirector,"CalibJet_muon_dR"),
      Track_pT                                (&fDirector,"Track_pT"),
      Track_eta                               (&fDirector,"Track_eta"),
      Track_phi                               (&fDirector,"Track_phi"),
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
      CR_Trig_dR                              (&fDirector,"CR_Trig_dR"),
      CR_Trig_eta                             (&fDirector,"CR_Trig_eta"),
      CR_Trig_phi                             (&fDirector,"CR_Trig_phi"),
      CR_Trig_ET                              (&fDirector,"CR_Trig_ET"),
      CR_Trig_pT                              (&fDirector,"CR_Trig_pT"),
      CR_Trig_EMF                             (&fDirector,"CR_Trig_EMF"),
      CR_Trig_logR                            (&fDirector,"CR_Trig_logR"),
      CR_Trig_nTrks                           (&fDirector,"CR_Trig_nTrks"),
      CR_Trig_timing                          (&fDirector,"CR_Trig_timing"),
      CR_Trig_passCuts                        (&fDirector,"CR_Trig_passCuts"),
      CR_Trig_jetMatch                        (&fDirector,"CR_Trig_jetMatch"),
      CR_Trig_isGoodLLP                       (&fDirector,"CR_Trig_isGoodLLP"),
      CR_Trig_trigET                          (&fDirector,"CR_Trig_trigET"),
      CR_Trig_trigpT                          (&fDirector,"CR_Trig_trigpT"),
      CR_Trig_trigEta                         (&fDirector,"CR_Trig_trigEta"),
      CR_Trig_trigPhi                         (&fDirector,"CR_Trig_trigPhi"),
      CR_Trig_trigEMF                         (&fDirector,"CR_Trig_trigEMF"),
      CR_Trig_triglogR                        (&fDirector,"CR_Trig_triglogR"),
      CR_Trig_minDR_HLTjet_HLTtracks          (&fDirector,"CR_Trig_minDR_HLTjet_HLTtracks"),
      CR_Trig_trigMatchLLP                    (&fDirector,"CR_Trig_trigMatchLLP"),
      CR_Trig_trig_LLP_DR                     (&fDirector,"CR_Trig_trig_LLP_DR"),
      CR_Trig_LLP_Lxy                         (&fDirector,"CR_Trig_LLP_Lxy"),
      CR_Trig_LLP_Lz                          (&fDirector,"CR_Trig_LLP_Lz"),
      CR_noTrig_eta                           (&fDirector,"CR_noTrig_eta"),
      CR_noTrig_phi                           (&fDirector,"CR_noTrig_phi"),
      CR_noTrig_ET                            (&fDirector,"CR_noTrig_ET"),
      CR_noTrig_pT                            (&fDirector,"CR_noTrig_pT"),
      CR_noTrig_EMF                           (&fDirector,"CR_noTrig_EMF"),
      CR_noTrig_nTrks                         (&fDirector,"CR_noTrig_nTrks"),
      CR_noTrig_timing                        (&fDirector,"CR_noTrig_timing"),
      CR_noTrig_passCuts                      (&fDirector,"CR_noTrig_passCuts"),
      CR_noTrig_jetMatchLLP                   (&fDirector,"CR_noTrig_jetMatchLLP"),
      CR_noTrig_LLP_Lxy                       (&fDirector,"CR_noTrig_LLP_Lxy"),
      CR_noTrig_LLP_Lz                        (&fDirector,"CR_noTrig_LLP_Lz"),
      muRoIClus_Trig_eta                      (&fDirector,"muRoIClus_Trig_eta"),
      muRoIClus_Trig_phi                      (&fDirector,"muRoIClus_Trig_phi"),
      muRoIClus_Trig_nRoI                     (&fDirector,"muRoIClus_Trig_nRoI"),
      muRoIClus_Trig_nJet                     (&fDirector,"muRoIClus_Trig_nJet"),
      muRoIClus_Trig_nTrk                     (&fDirector,"muRoIClus_Trig_nTrk"),
      muRoIClus_Trig_trig                     (&fDirector,"muRoIClus_Trig_trig"),
      muRoIClus_Trig_offline                  (&fDirector,"muRoIClus_Trig_offline"),
      muRoIClus_Trig_offline_noiso            (&fDirector,"muRoIClus_Trig_offline_noiso"),
      muRoIClus_Trig_LLP_Lxy                  (&fDirector,"muRoIClus_Trig_LLP_Lxy"),
      muRoIClus_Trig_LLP_Lz                   (&fDirector,"muRoIClus_Trig_LLP_Lz"),
      muRoIClus_Trig_LLP_dR                   (&fDirector,"muRoIClus_Trig_LLP_dR"),
      muRoIClus_Trig_trigMatchLLP             (&fDirector,"muRoIClus_Trig_trigMatchLLP"),
      muRoIClus_Trig_trackdR                  (&fDirector,"muRoIClus_Trig_trackdR"),
      muRoIClus_Trig_trackPt                  (&fDirector,"muRoIClus_Trig_trackPt"),
      muRoIClus_Trig_jetdR                    (&fDirector,"muRoIClus_Trig_jetdR"),
      muRoIClus_Trig_jetET                    (&fDirector,"muRoIClus_Trig_jetET"),
      muRoIClus_Trig_jetLogRatio              (&fDirector,"muRoIClus_Trig_jetLogRatio"),
      muRoIClus_Trig_trig_noiso               (&fDirector,"muRoIClus_Trig_trig_noiso"),
      muRoIClus_Trig_LLP_eta                  (&fDirector,"muRoIClus_Trig_LLP_eta"),
      muRoIClus_Trig_LLP_phi                  (&fDirector,"muRoIClus_Trig_LLP_phi"),
      muRoIClus_Trig_LLP_beta                 (&fDirector,"muRoIClus_Trig_LLP_beta"),
      muRoIClus_Trig_LLP_pT                   (&fDirector,"muRoIClus_Trig_LLP_pT"),
      MSVertex_LLP_eta                        (&fDirector,"MSVertex_LLP_eta"),
      MSVertex_LLP_phi                        (&fDirector,"MSVertex_LLP_phi"),
      MSVertex_LLP_beta                       (&fDirector,"MSVertex_LLP_beta"),
      MSVertex_LLP_pT                         (&fDirector,"MSVertex_LLP_pT"),
      MSVertex_LLP_Lxy                        (&fDirector,"MSVertex_LLP_Lxy"),
      MSVertex_LLP_Lz                         (&fDirector,"MSVertex_LLP_Lz"),
      MSVertex_LLP_dR                         (&fDirector,"MSVertex_LLP_dR"),
      MSVertex_llpMatch                       (&fDirector,"MSVertex_llpMatch"),
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
      LLP_E                                   (&fDirector,"LLP_E"),
      LLP_pT                                  (&fDirector,"LLP_pT"),
      LLP_eta                                 (&fDirector,"LLP_eta"),
      LLP_phi                                 (&fDirector,"LLP_phi"),
      LLP_beta                                (&fDirector,"LLP_beta"),
      LLP_timing                              (&fDirector,"LLP_timing"),
      LLP_Lxy                                 (&fDirector,"LLP_Lxy"),
      LLP_Lz                                  (&fDirector,"LLP_Lz"),
      LLP_dR_Jet                              (&fDirector,"LLP_dR_Jet"),
      LLP_nJet_dRlt04                         (&fDirector,"LLP_nJet_dRlt04"),
      DiLeptonFinder_eventTriggers            (&fDirector,"DiLeptonFinder_eventTriggers"),
      DiLeptonFinder_leadingElectron_E        (&fDirector,"DiLeptonFinder_leadingElectron_E"),
      DiLeptonFinder_leadingElectron_pT       (&fDirector,"DiLeptonFinder_leadingElectron_pT"),
      DiLeptonFinder_leadingElectron_eta      (&fDirector,"DiLeptonFinder_leadingElectron_eta"),
      DiLeptonFinder_leadingElectron_phi      (&fDirector,"DiLeptonFinder_leadingElectron_phi"),
      DiLeptonFinder_leadingElectron_charge   (&fDirector,"DiLeptonFinder_leadingElectron_charge"),
      DiLeptonFinder_leadingElectron_pTcone20 (&fDirector,"DiLeptonFinder_leadingElectron_pTcone20"),
      DiLeptonFinder_leadingElectron_passCuts (&fDirector,"DiLeptonFinder_leadingElectron_passCuts"),
      DiLeptonFinder_leadingElectron_passTriggers(&fDirector,"DiLeptonFinder_leadingElectron_passTriggers"),
      DiLeptonFinder_secondLeadingElectron_E  (&fDirector,"DiLeptonFinder_secondLeadingElectron_E"),
      DiLeptonFinder_secondLeadingElectron_pT (&fDirector,"DiLeptonFinder_secondLeadingElectron_pT"),
      DiLeptonFinder_secondLeadingElectron_eta(&fDirector,"DiLeptonFinder_secondLeadingElectron_eta"),
      DiLeptonFinder_secondLeadingElectron_phi(&fDirector,"DiLeptonFinder_secondLeadingElectron_phi"),
      DiLeptonFinder_secondLeadingElectron_charge(&fDirector,"DiLeptonFinder_secondLeadingElectron_charge"),
      DiLeptonFinder_secondLeadingElectron_pTcone20(&fDirector,"DiLeptonFinder_secondLeadingElectron_pTcone20"),
      DiLeptonFinder_secondLeadingElectron_passCuts(&fDirector,"DiLeptonFinder_secondLeadingElectron_passCuts"),
      DiLeptonFinder_secondLeadingElectron_passTriggers(&fDirector,"DiLeptonFinder_secondLeadingElectron_passTriggers"),
      DiLeptonFinder_dielectron_combinedMass  (&fDirector,"DiLeptonFinder_dielectron_combinedMass"),
      DiLeptonFinder_n_electron_candidates    (&fDirector,"DiLeptonFinder_n_electron_candidates"),
      DiLeptonFinder_leadingMuon_E            (&fDirector,"DiLeptonFinder_leadingMuon_E"),
      DiLeptonFinder_leadingMuon_pT           (&fDirector,"DiLeptonFinder_leadingMuon_pT"),
      DiLeptonFinder_leadingMuon_eta          (&fDirector,"DiLeptonFinder_leadingMuon_eta"),
      DiLeptonFinder_leadingMuon_phi          (&fDirector,"DiLeptonFinder_leadingMuon_phi"),
      DiLeptonFinder_leadingMuon_charge       (&fDirector,"DiLeptonFinder_leadingMuon_charge"),
      DiLeptonFinder_leadingMuon_pTcone20     (&fDirector,"DiLeptonFinder_leadingMuon_pTcone20"),
      DiLeptonFinder_leadingMuon_passCuts     (&fDirector,"DiLeptonFinder_leadingMuon_passCuts"),
      DiLeptonFinder_leadingMuon_passTriggers (&fDirector,"DiLeptonFinder_leadingMuon_passTriggers"),
      DiLeptonFinder_secondLeadingMuon_E      (&fDirector,"DiLeptonFinder_secondLeadingMuon_E"),
      DiLeptonFinder_secondLeadingMuon_pT     (&fDirector,"DiLeptonFinder_secondLeadingMuon_pT"),
      DiLeptonFinder_secondLeadingMuon_eta    (&fDirector,"DiLeptonFinder_secondLeadingMuon_eta"),
      DiLeptonFinder_secondLeadingMuon_phi    (&fDirector,"DiLeptonFinder_secondLeadingMuon_phi"),
      DiLeptonFinder_secondLeadingMuon_charge (&fDirector,"DiLeptonFinder_secondLeadingMuon_charge"),
      DiLeptonFinder_secondLeadingMuon_pTcone20(&fDirector,"DiLeptonFinder_secondLeadingMuon_pTcone20"),
      DiLeptonFinder_secondLeadingMuon_passCuts(&fDirector,"DiLeptonFinder_secondLeadingMuon_passCuts"),
      DiLeptonFinder_secondLeadingMuon_passTriggers(&fDirector,"DiLeptonFinder_secondLeadingMuon_passTriggers"),
      DiLeptonFinder_dimuon_combinedMass      (&fDirector,"DiLeptonFinder_dimuon_combinedMass"),
      DiLeptonFinder_n_muon_candidates        (&fDirector,"DiLeptonFinder_n_muon_candidates"),
      DiLeptonFinder_MET_e                    (&fDirector,"DiLeptonFinder_MET_e"),
      DiLeptonFinder_MET_px                   (&fDirector,"DiLeptonFinder_MET_px"),
      DiLeptonFinder_MET_py                   (&fDirector,"DiLeptonFinder_MET_py"),
      DiLeptonFinder_MET_phi                  (&fDirector,"DiLeptonFinder_MET_phi")
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
