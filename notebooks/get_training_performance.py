from bdt_training_scikit_tools import load_default_samples, default_training_variable_list, \
    test_train_samples, prep_samples, default_training, calc_performance
    
def do_training (vlist):
    all_events, training_list = vlist
    return get_training_performance (all_events, training_list)
    
def get_training_performance (all_events, training_list):
    '''Run a training with the set of varaibles given. Return a performance table.'''
    
    # Split into testing and training samples
    train, test = test_train_samples(all_events)
        
    # Prep samples for training
    all_events, all_events_class, training_weight, evaluation_weight = prep_samples(train[0], train[1], train[2], training_variable_list=training_list)
    
    # Run training
    bdt = default_training(all_events, training_weight, all_events_class, estimators=200)
    
    # Create a thing of all the results
    return {tuple(training_list): calc_performance(bdt, test, training_variables = training_list)}