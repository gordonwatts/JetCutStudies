# Tools to help with using our data and scikit-learn
# To be imported by other notebooks, etc.

from glob import glob
import pandas as pd
import numpy as np

from sklearn.ensemble import AdaBoostClassifier
from sklearn.metrics import accuracy_score
from sklearn.tree import DecisionTreeClassifier

def load_sample(name_pattern):
    '''Load in all files with prefex name.
    
    Args:
        name_pattern - path pattern that resolves to a set of files
        
    Returns
        df - Dataframe of all files contacted together
    '''
    files = glob(name_pattern)
    if len(files) == 0:
        print ("No files found matching {0}".format(name_pattern))
        return
    
    dfs = [pd.read_pickle(f) for f in files]
    df = dfs[0]
    for adf in dfs[1:]:
        df = df.append(adf)
    return df

def load_default_samples(job):
    '''Return the bib, mj, and signal samples for this job from the
    default location.
    
    Args
        job - name of job subdirectory
        
    Returns
        bib - the bib dataframe
        multijet - the mj dataframe
        signal - the signal dataframe
    '''
    job = "8"
    bib = load_sample("../../MVARawData/{0}/bib16*.p".format(job))
    multijet = load_sample("../../MVARawData/{0}/multijet*.p".format(job))
    signal = load_sample("../../MVARawData/{0}/signal*.p".format(job))
    
    print ("BIB length: {0}".format(len(bib.index)))
    print ("Multijet length: {0}".format(len(multijet.index)))
    print ("Signal length: {0}".format(len(signal.index)))

    return (bib, multijet, signal)

# The default variable list for training
default_training_variable_list = ['JetPt', 'CalRatio',
       'NTracks', 'SumPtOfAllTracks', 'MaxTrackPt',
       'JetWidth', 'EnergyDensity',
       'HadronicLayer1Fraction', 'JetLat', 'JetLong', 'FirstClusterRadius',
       'ShowerCenter', 'BIBDeltaTimingM', 'BIBDeltaTimingP', 'PredictedLz',
       'PredictedLxy']

# Prep the samples for training - limit number of events, etc.
def prep_samples (bib, mj, sig, nEvents = 0, training_variable_list = default_training_variable_list):
    '''Convert the input data frames into samples that are ready to feed to the
    scikitlearn infrastructure.
    
    Args
        bib - the bib dataframe
        mj - the mj dataframe
        sig - the signal dataframe
        nEvents - how many events to prepare. 0 means use everything.
        
    Returns
        events - all the events appended in a dataframe
        event_classes - class index (0 is bib, 1 for mj, and 2 for sig)
        weights - the weights from the Weight column
    '''
    # Append the three inputs, as they are what we will be fitting against.
    # At the same time (to keep things straight) build the class sigle array.
    s_bib = bib if nEvents == 0 else bib[:nEvents]
    s_bib_class = pd.DataFrame(np.zeros(len(s_bib.index)), columns=['Class'], dtype = 'int64')
    s_mj = mj if nEvents == 0 else mj[:nEvents]
    s_mj_class = pd.DataFrame(np.ones(len(s_mj.index)), columns=['Class'], dtype = 'int64')
    s_sig = sig if nEvents == 0 else sig[:nEvents]
    s_sig_class = pd.DataFrame(np.ones(len(s_sig.index))*2, columns=['Class'], dtype = 'int64')
    
    all_events = s_bib.append(s_mj, ignore_index=True)
    all_events = all_events.append(s_sig, ignore_index=True)
    
    all_events_class = s_bib_class.append(s_mj_class, ignore_index=True)
    all_events_class = all_events_class.append(s_sig_class, ignore_index=True)

    return (all_events.loc[:,training_variable_list], all_events_class, all_events.Weight)

def train_me (bib, mj, sig, nEvents = 10000, training_variable_list = default_training_variable_list):
    '''Return training on nEvents
    
    Classes are 0 for bib, 1 for mj, and 2 for sig
    
    All events will be used in the fit (none for testing!)
    
    Args:
        bib - BIB background
        mj - MJ background
        sig - signal
        nEvents - how many events of each to use
    '''
    
    all_events, all_events_class = prep_samples(bib, mj, sig, nEvents, training_variable_list)
    
    # Ready to train!
    bdt_discrete = AdaBoostClassifier(
        DecisionTreeClassifier(min_samples_leaf=0.01),
        n_estimators=10,
        learning_rate=1)
    
    bdt_discrete.fit(all_events, all_events_class.Class)
    
    # The BDT is sent back for use
    return bdt_discrete

def test_train_samples(sample_list, event_mod = 3):
    '''Split the samples into those that are for testing and training. Use
    the modulus of event number to do the split.
    
    Args
        sample_list - iterable list of samples that we are to split
        event_mod - the modulus to apply. Those that are != 0 will be put in teh training sample.
        
    Returns
        training - list of same length as sample_list with just training event
        testing - list of same length as sample_list with just testing events
    '''
    # Seperate the samples we will look at.
    training = [s[s.EventNumber % event_mod != 0] for s in sample_list]
    testing = [s[s.EventNumber % event_mod == 0] for s in sample_list]
    
    return (training, testing)
