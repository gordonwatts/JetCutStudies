# Tools to help with using our data and scikit-learn
# To be imported by other notebooks, etc.

from glob import glob
import pandas as pd
import numpy as np
import matplotlib.pyplot as plt

from math import sqrt

from sklearn.ensemble import AdaBoostClassifier
from sklearn.metrics import accuracy_score
from sklearn.tree import DecisionTreeClassifier

def load_sample(name_pattern_root):
    '''Load in all files with prefex name.
    
    Args:
        name_pattern_root - path pattern that resovles to a set of csv or pickle files.
        
    Returns
        df - Dataframe of all files contacted together
    '''
    files = glob("{0}*.p".format(name_pattern_root)) + glob("{0}*.csv".format(name_pattern_root))
    if len(files) == 0:
        print ("No files found matching {0}".format(name_pattern_root))
        return
    
    dfs = [pd.read_pickle(f) for f in files if f.endswith(".p")] + [pd.read_csv(f) for f in files if f.endswith(".csv")]
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
    bib = load_sample("../../MVARawData/{0}/bib16".format(job))
    multijet = load_sample("../../MVARawData/{0}/multijet".format(job))
    signal = load_sample("../../MVARawData/{0}/signal".format(job))
    
    print ("BIB: {0} events".format(len(bib.index)))
    print ("Multijet: {0} events".format(len(multijet.index)))
    print ("Signal: {0} events".format(len(signal.index)))

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
        training_weight - the weights from the Weight column
        evaluation_weight - the xsection * mc weight
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

    return (all_events.loc[:,training_variable_list], all_events_class, all_events.Weight, all_events.WeightMCEvent*all_events.WeightXSection)

def default_training (events, events_weight, events_class, min_leaf_fraction = 0.01):
    '''Given samples prepared, run the default "best" training we know how to run.
    
    Args:
        events - A DF with an entry for every event, with all columns to be trained on
        events_weight - weight assigned to each event (None if no weight is to be used)
        events_class - the training class (0, 1, 2 for bib, mj, and signal)
        min_leaf_fraction - fraction of sample that can be in each leaf. Defaults to 1%
        
    Returns
        bdt - A trained boosted decision tree
    '''
    bdt = AdaBoostClassifier(
        DecisionTreeClassifier(min_samples_leaf=min_leaf_fraction),
        n_estimators=10,
        learning_rate=1)
    
    bdt.fit(events, events_class.Class, sample_weight = events_weight)
    
    # The BDT is sent back for use
    return bdt
    

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
    
    all_events, all_events_class, training_weight, evaluation_weight = prep_samples(bib, mj, sig, nEvents, training_variable_list)
    
    # Ready to train!
    bdt_discrete = AdaBoostClassifier(
        DecisionTreeClassifier(min_samples_leaf=0.01),
        n_estimators=10,
        learning_rate=1)
    
    bdt_discrete.fit(all_events, all_events_class.Class, sample_weight = training_weight)
    
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

def plot_training_performance (bdt, training_sample, testing_sample, title_keyword):
    '''Generate a figure that shows training as a function of the number of trees and some quick info
    on the differences between test and training samples.
    
    Args
        bdt - the BDT we are testing
        training_sample - the tripple of training samples (bib, mj, hss) as treturned from test_train_samples
        testing_sample - the tripple of testing samples
        
    Returns
        fig - a figure
    '''

    # Extract test and training events suitable to feeding to the bdt
    test_events, test_events_class, test_weights, test_eval_weights = prep_samples(testing_sample[0], testing_sample[1], testing_sample[2])
    train_events, train_events_class, training_weights, training_eval_weights = prep_samples(training_sample[0], training_sample[1], training_sample[2])

    # Training performance looks at generic things - like has the tree settled down quickly or not.
    test_errors = []
    for test_predict in bdt.staged_predict(test_events):
        test_errors.append(1.0 - accuracy_score(test_predict, test_events_class.Class))

    n_trees = len(bdt)
    estimator_errors = bdt.estimator_errors_[:n_trees]

    # Compare training and testing prediction vs what we see.
    test_predictions = bdt.predict(test_events)
    train_predictions = bdt.predict(train_events)
    
    # Generate the figure to return
    fig = plt.figure(figsize=(15,15))

    ax = plt.subplot(221)
    ax.plot(range(1, n_trees+1), test_errors, c='black')
    ax.set_ylim(0.0, 1.0)
    ax.set_ylabel('Test Error')
    ax.set_xlabel('Number of trees')
    ax.set_title('Test Error for {0} training'.format(title_keyword))

    ax = plt.subplot(222)
    ax.plot(range(1,n_trees+1), estimator_errors, c='black', label="SAMME")
    ax.set_ylim(0, 1.0)
    ax.set_ylabel('Esitmator Error')
    ax.set_xlabel('Number of trees')
    ax.set_title('Estimator Error for {0} training'.format(title_keyword))

    ax = plt.subplot(223)
    ax.hist([test_events_class.Class, test_predictions], label=["Actual", "Predicted by BDT"], bins=[0,1,2,3])
    ax.set_xticks([0.5,1.5,2.5])
    ax.set_xticklabels(['BIB', 'MJ', 'HSS'])
    ax.set_title('Test Events: Prediction vs Actual for {0} training'.format(title_keyword))
    ax.legend()

    ax = plt.subplot(224)
    ax.hist([train_events_class.Class, train_predictions], label=["Actual", "Predicted by BDT"], bins=[0,1,2,3])
    ax.set_xticks([0.5,1.5,2.5])
    ax.set_xticklabels(['BIB', 'MJ', 'HSS'])
    ax.set_title('Train Events: Prediction vs Actual for {0} training'.format(title_keyword))
    ax.legend()

    return fig

def calc_performance_for_run(df):
    '''Given a data frame with the prediction and actual events, determine a set of numbers about it and return them.
    
    Args
        df - DataFrame containing columns with Weight, PredClass, and Class
    
    Return
        d - dict containing number of events of each type predicted for each time, and S/sqrt(B) for S as signal and B as mj+bib
    '''
    # Next, for each sample, calculate what we need.
    class_events = [df.Class == index for index in (0,1,2)]
    pred_events = [df.PredClass == index for index in (0,1,2)]        
    labels = {0:'BIB', 1:'MJ', 2:'HSS'}
    
    d = {"{0}in{1}".format(labels[aclass], labels[pclass]):np.sum((class_events[aclass]&pred_events[pclass])*df.Weight) for pclass in (0,1,2) for aclass in (0,1,2)}
    
    # Calculated quantities
    
    d.update({'{0}Eff'.format(labels[aclass]):np.sum((pred_events[aclass]&class_events[aclass])*df.Weight)/np.sum(class_events[aclass]*df.Weight) for aclass in (0,1,2)})
    d.update({'{0}Back'.format(labels[aclass]):np.sum((pred_events[aclass]&(~class_events[aclass]))*df.Weight) for aclass in (0,1,2)})
    d.update({'{0}SsqrtB'.format(labels[aclass]):(d['{0}in{0}'.format(labels[aclass])]/sqrt(d['{0}Back'.format(labels[aclass])])) for aclass in (0,1,2)})

    d.update({'{0}TotalWeight'.format(labels[aclass]):np.sum(class_events[aclass]*df.Weight) for aclass in (0,1,2)})
    d.update({'{0}TotalCount'.format(labels[aclass]):np.sum(class_events[aclass]) for aclass in (0,1,2)})
    
    return d

def calc_performance (bdt, testing_samples):
    '''Calculate the nubers in each class, as well as S/sqrt(B) for HSS
    
    All sums are done with event weights taken into account
    
    Args
        bdt - The BDT trained
        testing_samples - tripple (bib, mj, signal) of events that this wasn't trained on
        
    Return
        d - dict containing number of events of each type predicted for each time, and S/sqrt(B) for S as signal and B as mj+bib
    '''
    
    test_events, test_events_class, test_weights, test_eval_weights = prep_samples(testing_samples[0], testing_samples[1], testing_samples[2])
    test_predictions = bdt.predict(test_events)
    
    # Assemble a single DataFrame with all the information we are going to need
    df = pd.DataFrame(test_predictions, columns=['PredClass'])
    df.loc[:,'Class'] = test_events_class
    df.loc[:,'Weight'] = test_eval_weights
    
    return calc_performance_for_run(df)