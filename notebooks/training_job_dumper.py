#
# methods to dump out information for a particular training job. The plots and data
# dumped are mainly focused on telling the difference between trainings - e.g. which is better.
#

# Configure the environment
import os
import pandas as pd
import numpy as np
import matplotlib.pyplot as plt
plt.rc('font', size=14) # Font size for titles and axes, default is 10
from mpl_toolkits.mplot3d import Axes3D # if this isn't done then the 3d projection isn't known. Aweful UI!
from sklearn.metrics import roc_curve, auc
import sys

# Load mva data from csv files
def load_mva_data(data_location, sample_name):
    """Load the data written out by a MVA training job into an np array.
    Returns null if the csv file can't be found.
    
    Args:
      sample_name: The name of the sample, e.g. data15
    """
    p = "{0}\\all-{1}.csv".format(data_location, sample_name)
    if not os.path.exists(p):
        return None
    return pd.read_csv(p)

# Load data for a list of samples
def load_mva_data_from_list(location, lst):
    """Load data for a series of samples. Silently ignore those we can't find
    
    Args:
        lst: List of samples to load
    """
    return {s[0] : s[1] for s in [(sname, load_mva_data(location, sname)) for sname in lst] if not(s[1] is None)}

# Plot a particular mva sample's response for training
def plot_mva_sample(sample_name, sample_data):
    """Plot the HSS, BIB, and MJ all on one plot as a histogram
    
    Args:
      sample_data: The DataFrame that contains the data for this sample
    """
    nbins = 100
    fig = plt.figure(figsize=(15,8))
    ax = fig.add_subplot(111)
    plt.hist([sample_data['HSSWeight'], sample_data['MultijetWeight'], sample_data['BIBWeight']],
             weights=[sample_data.Weight, sample_data.Weight, sample_data.Weight],
             bins=nbins,
             histtype = 'step', normed=True,
             color=['red', 'blue', 'green'],
             label=['HSS Weight', 'Multijet Weight', 'BIB Weight'])
    plt.title("BDT Weights for Sample {0}".format(sample_name))
    plt.xlabel('MVA Weight')
    plt.legend()
    ax.set_yscale('log')

# Plot a list of samples mva response.
def plot_mva_samples(dict_of_samples):
    for k in dict_of_samples.keys():
        plot_mva_sample(k, dict_of_samples[k])
        plt.show()

# Calc all the info for a ROC curve
def roc_curve_calc(signal, background, weight='HSSWeight'):
    '''Calculate a ROC curve
    
    Args:
        signal - data frame that contains the signal
        background - data frame that contains teh background
        weight - the column in the DataFrame that we pull the weights from
        
    Returns
        tpr - true positive rate
        fpr - false postive rate
        auc - Area under the curve for the tpr/fpr curve - bigger is better!
    '''
    # build a single array marking one as signal and the other as background
    truth = np.concatenate((np.ones(len(signal.index)), np.zeros(len(background.index))))
    score = np.concatenate((signal[weight], background[weight]))
    (fpr, tpr, thresholds) = roc_curve(truth,score)
    
    a = auc(fpr, tpr)
    
    return (tpr, fpr, a)

# Plot a roc curve
def plot_roc_curve (tpr, fpr, aroc, signal_name = 'Signal', background_name = 'JZ'):
    '''Plot a ROC curve
    
    Args:
        tpr - True positive rate
        fpr = False postiive rate
        aroc - the area under the curve (in the legend)
        signal_name - for the signal axis label
        background_name - for the background axis name
    '''
    plt.figure(figsize=(10,10))
    plt.plot(fpr, tpr, color='darkorange', label='ROC Curve area {0:0.2f}'.format(aroc))
    plt.xlim([0.0,1.0])
    plt.ylim([0.0,1.05])
    plt.ylabel('{0} Positive Rate'.format(signal_name))
    plt.xlabel('{0} Postiive Rate'.format(background_name))
    plt.legend()

# Calc a ROC curve for a particular bib cut
def calc_roc_with_bib_cut (sig, back, bib, bib_cut = 0.5):
    '''Calc ROC curve basics after limiting the data sample for a particular bib cut
    
    Args:
        sig - signal DataFrame
        back - background DataFrame
        bib - the bib sample
        bib_cut - only events with a bib value less than this number will be included
        
    Returns:
        tpr - true positive rate
        fpr - false positive rate
        aroc - area under the ROC curve
        sig_eff - How much signal this bib cut removed
        back_eff - how much background this bib cut removed
        bib_eff
    '''
    gsig = sig[sig['BIBWeight']<bib_cut]
    sig_eff = len(gsig.index)/len(sig.index)
    gback = back[back['BIBWeight']<bib_cut]
    back_eff = len(gback.index)/len(back.index)
    gbib = bib[bib['BIBWeight']<bib_cut]
    bib_eff = len(gbib.index)/len(bib.index)
    
    tpr, fpr, aroc = roc_curve_calc(gsig, gback)
    
    return (tpr, fpr, aroc, sig_eff, back_eff, bib_eff)

# Generate a family of ROC curves
def calc_roc_family (sig, back, bib, bib_cut_range = np.logspace(-3,0,30)):
    '''Calc ROC Curve for a family of bib cuts
    
    Args:
        sig - signal DataFrame
        back - background (jz) DataFrame
        bib - bib DataFrame
        bib_cut_range - Value of bib efficiencies to look at. Actual cuts are determined from this

    Returns:
        all - DataFrame of truth and false postive rate, area under curve, sig, back, and bib eff, and the bib cut
    '''
    sorted_bib_values = bib['BIBWeight'].sort_values()
    lst_len = len(sorted_bib_values.index)-1
    bib_cut_values = [sorted_bib_values.values[index] for index in [int(lst_len*cut_fraction) for cut_fraction in bib_cut_range]]
    all = [calc_roc_with_bib_cut (sig, back, bib, bib_cut = bc)+(bc,) for bc in bib_cut_values]
    return pd.DataFrame(all, columns=['tpr', 'fpr', 'aroc', 'sig_eff', 'back_eff', 'bib_eff', 'bib_cut'])

# Plot the efficiency for a single BIB sample
def plot_eff_for_bib(ps, sample_name):
    '''Plot the Efficiency plots for a single sample
    
    Args:
        ps - DataFrame of the sample we will look at
        sample_name - Name of sample
    '''
    fig = plt.figure(figsize=(10,10))
    ax = fig.add_subplot(111)
    plt.plot(ps.bib_eff,ps.sig_eff, label='Sample {0}'.format(sample_name))
    plt.plot(ps.bib_eff,ps.back_eff, label='Sample JZ')
    plt.plot(ps.bib_eff,ps.bib_eff, label='Sample BIB')
    plt.ylim([0.0, 1.0])
    plt.xlim([0.001, 1.0])
    plt.xlabel('BIB Fraction')
    plt.ylabel('Fraction of Events')
    ax.set_xscale('log')
    plt.grid(alpha=0.5, which='both')
    plt.title('{0}, JZ, and BIB Efficiencies as a function of BIB Fraction'.format(sample_name))
    plt.legend()

# Plot the eff of all samples on a single plot
def plot_all_eff_for_bib(samples):
    '''Plot the efficiency plots for all samples and JZ and BIB
    
    Args
        samples - The df containing all singal samples ROC calculations with eff loaded
    '''
    fig = plt.figure(figsize=(15,10))
    ax = fig.add_subplot(111)
    # Do the eff samples
    for s in samples:
        plt.plot(samples[s].bib_eff,samples[s].sig_eff, label='Sample {0}'.format(s))
    # Do the background
    abck=samples[list(samples.keys())[0]]
    plt.plot(abck.bib_eff,abck.back_eff, label='Sample JZ')
    plt.plot(abck.bib_eff,abck.bib_eff, label='Sample BIB')
    # Get the plot in shape
    plt.ylim([0.0, 1.0])
    plt.xlim([0.001, 1.0])
    plt.xlabel('BIB Fraction')
    plt.ylabel('Fraction of Events')
    plt.title('Signal, JZ, and BIB Efficiencies as a function of BIB Fraction')
    ax.set_xscale('log')
    plt.legend()

# Plot a ROC family of plots
def plot_roc_family_sample(sample_info_bib, sample_name, background_name = 'JZ', nsamples=10):
    '''Plot a family of scaled ROC curves
    
    Args:
        sample_info_bib - Each row has a DF containing info on each bib cut point
        sample_name - Name of the sample (for plot title)
        background_name - Name of the background samples (JZ usually)
        nsamples - sample the list of bib cuts (if there are too many)
        
    Returns
        None
        But matplotlib will be sitting at a plot
    '''
    resample = 1
    if nsamples <= len(sample_info_bib.index):
        resample = int(len(sample_info_bib.index)/nsamples)
    f = plt.figure(figsize=(15,10))
    ax = f.add_subplot(111)
    for index,r in sample_info_bib.iloc[::-resample].iterrows():
        sig_scale = r.sig_eff
        back_scale = r.back_eff
        ax.plot(r.fpr*back_scale, r.tpr*sig_scale, label='BIB Cut {0:0.4f} eff={1:0.3f} (AROC={2:0.3f})'.format(r.bib_cut, r.bib_eff, r.aroc))
    plt.xlim([0.0,1.0])
    plt.ylim([0.0,1.05])
    plt.ylabel('{0} Positive Rate'.format(sample_name))
    plt.xlabel('{0} Postiive Rate'.format(background_name))
    plt.title('ROC curves for {0} as a function of BIB cut'.format(sample_name))
    plt.legend()

# Slice up a sample by a partiuclar weight
def split_data_in_slices (sample, weight, divisions = 20):
    '''Split a sample up along a given axis. In each bin calculate the average
    
    Args:
        sample  -  The sample DataFrame we are going to split
        weight - Name of weight axis to split by
        divisions - How many equal sized bins to split this into
    
    Returns:
        DataFrame with rows labeled by the lower edge of the sliced bin, and average
        weight for HSS, MultiJet, and BIB, and a column with the # of events in each bin.
    '''
    s = sample.drop('Weight', axis=1)
    pdiv = 1.0/divisions
    slice_edges = [(b*pdiv, pdiv*(b+1)) for b in range(divisions)]

    mean_evolution = {se[0]:s[(s[weight] >= se[0]) & (s[weight] < se[1])].mean() for se in slice_edges}
    count = {se[0]:len(s[(s[weight] >= se[0]) & (s[weight] < se[1])].index) for se in slice_edges}

    me = pd.DataFrame(mean_evolution).T
    me['SliceCount']=pd.Series(list(count.values()), index=me.index)
    return me

# Split all the slices
def split_data_in_slices_for_all (samples, weight, divisions = 20):
    '''Split all the samples in the dict samples
    
    Args
        samples - dict of all samples
        weight - Name of weight axis to split along
        divisions - how many equal bins to split things into
        
    Return
        Dict of results
    '''
    return {name:split_data_in_slices(samples[name],weight,divisions) for name in samples}

# Plot the # of events in each slice
def plot_average_weights (sample, sample_name, slice_weight_name):
    '''Plot the slice data for a particular sample
    
    Args
        sample - slice data for the sample
        sample_name - the name of the sample we will put on plot
        weight_name - the weight name that was used to slice everything
        
    Returns
        Plot drawn on the current figure.
    '''
    fig = plt.figure(figsize=(10,10))
    ax = fig.add_subplot(111)
    ax.plot(sample.index, sample['HSSWeight'], label='HSS Weight')
    ax.plot(sample.index, sample['MultijetWeight'], label='JZ Weight')
    ax.plot(sample.index, sample['BIBWeight'], label="BIB Weight")
    ax.set_xlabel('Bin in {0}'.format(slice_weight_name))
    ax.set_ylabel('Average Weight Value')
    ax.set_title('Average Weights in {0} bins for {1}'.format(slice_weight_name, sample_name))
    plt.legend()

# Plot the # of events in each slice.
def plot_slice_sizes (sample, sample_name, slice_weight_name):
    '''Plot the number of events in each slice
    
    Args
        sample - Slice data for a sample
        sample_name - name of the sample for plot title
        slice_weight_name - name of weight that that is used to slice
        
    Returns
        Default plot of the number of events
    '''
    fig = plt.figure(figsize=(15,10))
    ax = fig.add_subplot(111)
    ax.plot(sample.index, sample.SliceCount)
    ax.set_xlabel('Bin in {0}'.format(slice_weight_name))
    ax.set_ylabel('Number of Events in Bin')
    ax.set_title('Events in each slice of {0} in {1}'.format(slice_weight_name, sample_name))

# Call this to dump out plotting information in the location of jobdir.
def training_job (jobdir, outputdir, jobindex):
    '''Dump all plots for a particular job.

    Args:
        jobdir - location in the filesystem where the job csv files have been unpacked
        outputdir - location where the plots and python outputs should be written
        jobindex - the job number - used as part of the job filenames.

    '''
    signal_sample_names=["125pi25lt5m", "200pi25lt5m", "400pi50lt5m", "600pi150lt5m", "1000pi400lt5m"]
    bib_sample_names=["data15", "data16"]
    mj_sample_names=["jz"]

    signal_samples = load_mva_data_from_list(jobdir, signal_sample_names)
    bib_samples = load_mva_data_from_list(jobdir, bib_sample_names)
    mj_samples = load_mva_data_from_list(jobdir, mj_sample_names)

    # Plot the raw MVA values for the samples
    all_samples = {**signal_samples, **bib_samples, **mj_samples}
    for k in all_samples:
        plot_mva_sample(k, all_samples[k])
        plt.savefig("{0}/{1}-mva-{2}.png".format(outputdir, jobindex, k))
        plt.close()

    # See how the MVA's evolve as a function of the Jet and signal numbers
    all_slices = {sname:split_data_in_slices_for_all(all_samples, sname) for sname in ['MultijetWeight', 'HSSWeight']}

    for wt in all_slices:
        for s in all_slices[wt]:
            plot_average_weights(all_slices[wt][s], s, wt)
            plt.savefig("{0}/{1}-slice-{2}-{3}.png".format(outputdir, jobindex, s, wt))
            plt.close()
            plot_slice_sizes(all_slices[wt][s], s, wt)
            plt.savefig("{0}/{1}-slicesize-{2}-{3}.png".format(outputdir, jobindex, s, wt))
            plt.close()

    # Get ROC info for all samples, and plot them
    p_samples = {sname:calc_roc_family(signal_samples[sname], mj_samples["jz"], bib_samples['data15']) for sname in signal_samples.keys()}
    for sname in p_samples.keys():
        plot_roc_family_sample(p_samples[sname], sname)
        plt.savefig("{0}/{1}-roc-{2}.png".format(outputdir, jobindex, sname))
        plt.close()

    # Plot the efficiencies for all these samples on a single plot
    plot_all_eff_for_bib(p_samples)
    plt.savefig("{0}/{1}-eff.png".format(outputdir, jobindex))
    plt.close()


# If invoked from main
def main(args):
    jobdir = args[1]
    outputdir = args[2]
    jobindex = args[3]

    if not os.path.exists(jobdir):
        raise Exception("Input directory {0} does not exist.".format(jobdir))
    if not os.path.exists(outputdir):
        raise Exception("Output directory {0} does not exist!".format(outputdir))

    training_job(jobdir, outputdir, jobindex)

if __name__ == '__main__':
    main(sys.argv)