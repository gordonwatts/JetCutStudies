/////////////////////////////////////////////////////////////////////////
//   This class has been automatically generated 
//   (at Fri Jun 17 00:47:16 2016 by ROOT version 5.34/36)
//   from TTree EventLoop_FileExecuted/executed files
//   found on file: E:\GRIDDS\user.gwatts.361024.Pythia8EvtGen_A14NNPDF23LO_jetjet_JZ4W.DAOD_EXOT15.r6765_r6282_p2452.DiVertAnalysis_v5_1BC42831_hist\user.gwatts\user.gwatts.8578406._000001.hist-output.root
/////////////////////////////////////////////////////////////////////////


#ifndef ntuple_EventLoop_FileExecuted_h
#define ntuple_EventLoop_FileExecuted_h

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
class TString;


// Header needed by this particular proxy
#include "C:/build/workspace/root-release-5.34/BUILDTYPE/Release/COMPILER/vc12/LABEL/win7/sources/root_v5.34.36/root/core/base/inc/TString.h"


class junk_macro_parsettree_EventLoop_FileExecuted_Interface {
   // This class defines the list of methods that are directly used by ntuple_EventLoop_FileExecuted,
   // and that can be overloaded in the user's script
public:
   void junk_macro_parsettree_EventLoop_FileExecuted_Begin(TTree*) {}
   void junk_macro_parsettree_EventLoop_FileExecuted_SlaveBegin(TTree*) {}
   Bool_t junk_macro_parsettree_EventLoop_FileExecuted_Notify() { return kTRUE; }
   Bool_t junk_macro_parsettree_EventLoop_FileExecuted_Process(Long64_t) { return kTRUE; }
   void junk_macro_parsettree_EventLoop_FileExecuted_SlaveTerminate() {}
   void junk_macro_parsettree_EventLoop_FileExecuted_Terminate() {}
};


class ntuple_EventLoop_FileExecuted : public TSelector, public junk_macro_parsettree_EventLoop_FileExecuted_Interface {
public :
   TTree          *fChain;         //!pointer to the analyzed TTree or TChain
   TH1            *htemp;          //!pointer to the histogram
   TBranchProxyDirector fDirector; //!Manages the proxys

   // Optional User methods
   TClass         *fClass;    // Pointer to this class's description

   // Proxy for each of the branches, leaves and friends of the tree


   ntuple_EventLoop_FileExecuted(TTree *tree=0) : 
      fChain(0),
      htemp(0),
      fDirector(tree,-1),
      fClass                (TClass::GetClass("ntuple_EventLoop_FileExecuted"))
      { }
   ~ntuple_EventLoop_FileExecuted();
   Int_t   Version() const {return 1;}
   void    Begin(::TTree *tree);
   void    SlaveBegin(::TTree *tree);
   void    Init(::TTree *tree);
   Bool_t  Notify();
   Bool_t  Process(Long64_t entry);
   void    SlaveTerminate();
   void    Terminate();

   ClassDef(ntuple_EventLoop_FileExecuted,0);


//inject the user's code
#include "junk_macro_parsettree_EventLoop_FileExecuted.C"
};

#endif


#ifdef __MAKECINT__
#pragma link C++ class ntuple_EventLoop_FileExecuted;
#endif


inline ntuple_EventLoop_FileExecuted::~ntuple_EventLoop_FileExecuted() {
   // destructor. Clean up helpers.

}

inline void ntuple_EventLoop_FileExecuted::Init(TTree *tree)
{
//   Set branch addresses
   if (tree == 0) return;
   fChain = tree;
   fDirector.SetTree(fChain);
   if (htemp == 0) {
      htemp = fDirector.CreateHistogram(GetOption());
      htemp->SetTitle("junk_macro_parsettree_EventLoop_FileExecuted.C");
      fObject = htemp;
   }
}

Bool_t ntuple_EventLoop_FileExecuted::Notify()
{
   // Called when loading a new file.
   // Get branch pointers.
   fDirector.SetTree(fChain);
   junk_macro_parsettree_EventLoop_FileExecuted_Notify();
   
   return kTRUE;
}
   

inline void ntuple_EventLoop_FileExecuted::Begin(TTree *tree)
{
   // The Begin() function is called at the start of the query.
   // When running with PROOF Begin() is only called on the client.
   // The tree argument is deprecated (on PROOF 0 is passed).

   TString option = GetOption();
   junk_macro_parsettree_EventLoop_FileExecuted_Begin(tree);

}

inline void ntuple_EventLoop_FileExecuted::SlaveBegin(TTree *tree)
{
   // The SlaveBegin() function is called after the Begin() function.
   // When running with PROOF SlaveBegin() is called on each slave server.
   // The tree argument is deprecated (on PROOF 0 is passed).

   Init(tree);

   junk_macro_parsettree_EventLoop_FileExecuted_SlaveBegin(tree);

}

inline Bool_t ntuple_EventLoop_FileExecuted::Process(Long64_t entry)
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
   junk_macro_parsettree_EventLoop_FileExecuted();
   junk_macro_parsettree_EventLoop_FileExecuted_Process(entry);
   return kTRUE;

}

inline void ntuple_EventLoop_FileExecuted::SlaveTerminate()
{
   // The SlaveTerminate() function is called after all entries or objects
   // have been processed. When running with PROOF SlaveTerminate() is called
   // on each slave server.
   junk_macro_parsettree_EventLoop_FileExecuted_SlaveTerminate();
}

inline void ntuple_EventLoop_FileExecuted::Terminate()
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
   junk_macro_parsettree_EventLoop_FileExecuted_Terminate();
}
