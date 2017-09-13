using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CalRatioTMVAUtilities
{
    public static class TrainingVariableUtils
    {
        /// <summary>
        /// Possible variables for training dataset
        /// </summary>
        public enum TrainingVariableSet
        {
            Default5pT,
            Default5ET,
            DefaultAllpT,
            DefaultAllET,
            Analysis2015pT,
            None
        }

        /// <summary>
        /// All possible training variables
        /// </summary>
        public enum TrainingVariables
        {
            JetPt,
            JetPhi,
            CalRatio,
            JetEta,
            NTracks,
            SumPtOfAllTracks,
            MaxTrackPt,
            JetET,
            JetWidth,
            JetTrackDR,
            EnergyDensity,
            HadronicLayer1Fraction,
            JetLat,
            JetLong,
            FirstClusterRadius,
            ShowerCenter,
            BIBDeltaTimingPlus,
            BIBDeltaTimingMinus,
            PredictedLxy,
            PredictedLz,
            InteractionsPerCrossing,
        }

        /// <summary>
        /// Return a list of the training variables that we get by looking at command line options.
        /// </summary>
        /// <returns></returns>
        public static Expression<Func<TrainingTree, double>>[] GetTrainingVariables(TrainingVariableSet varSet, TrainingVariables[] adds, TrainingVariables[] drops)
        {
            return GetListOfVariablesToUse(varSet, adds, drops)
                .Select(v => DictionaryPairForVariable(v))
                .ToArray();
        }

        /// <summary>
        /// Turn a particular type into an expression.
        /// </summary>
        /// <param name="jetPt"></param>
        /// <returns></returns>
        private static Expression<Func<TrainingTree, double>> DictionaryPairForVariable(TrainingVariables varName)
        {
            switch (varName)
            {
                case TrainingVariables.JetPt:
                    return t => t.JetPt;

                case TrainingVariables.CalRatio:
                    return t => t.CalRatio;

                case TrainingVariables.JetEta:
                    return t => t.JetEta;

                case TrainingVariables.JetPhi:
                    return t => t.JetPhi;

                case TrainingVariables.NTracks:
                    return t => t.NTracks;

                case TrainingVariables.SumPtOfAllTracks:
                    return t => t.SumPtOfAllTracks;

                case TrainingVariables.MaxTrackPt:
                    return t => t.MaxTrackPt;

                case TrainingVariables.JetET:
                    return t => t.JetET;

                case TrainingVariables.JetWidth:
                    return t => t.JetWidth;

                case TrainingVariables.JetTrackDR:
                    return t => t.JetDRTo2GeVTrack;

                case TrainingVariables.EnergyDensity:
                    return t => t.EnergyDensity;

                case TrainingVariables.HadronicLayer1Fraction:
                    return t => t.HadronicLayer1Fraction;

                case TrainingVariables.JetLat:
                    return t => t.JetLat;

                case TrainingVariables.JetLong:
                    return t => t.JetLong;

                case TrainingVariables.FirstClusterRadius:
                    return t => t.FirstClusterRadius;

                case TrainingVariables.ShowerCenter:
                    return t => t.ShowerCenter;

                case TrainingVariables.BIBDeltaTimingMinus:
                    return t => t.BIBDeltaTimingM;

                case TrainingVariables.BIBDeltaTimingPlus:
                    return t => t.BIBDeltaTimingP;

                case TrainingVariables.PredictedLxy:
                    return t => t.PredictedLxy;

                case TrainingVariables.PredictedLz:
                    return t => t.PredictedLz;

                case TrainingVariables.InteractionsPerCrossing:
                    return t => t.InteractionsPerCrossing;

                default:
                    throw new NotImplementedException($"Unknown variable requested: {varName.ToString()}");
            }
        }

        /// <summary>
        /// Return a list of all variables that we are using.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<TrainingVariables> GetListOfVariablesToUse(TrainingVariableSet opt, TrainingVariables[] adds, TrainingVariables[] drops)
        {
            var result = new HashSet<TrainingVariables>();

            // First take care of the sets
            switch (opt)
            {
                case TrainingVariableSet.Default5pT:
                    result.Add(TrainingVariables.JetPt);
                    result.Add(TrainingVariables.CalRatio);
                    result.Add(TrainingVariables.NTracks);
                    result.Add(TrainingVariables.SumPtOfAllTracks);
                    result.Add(TrainingVariables.MaxTrackPt);
                    break;

                case TrainingVariableSet.Default5ET:
                    result.Add(TrainingVariables.JetET);
                    result.Add(TrainingVariables.CalRatio);
                    result.Add(TrainingVariables.NTracks);
                    result.Add(TrainingVariables.SumPtOfAllTracks);
                    result.Add(TrainingVariables.MaxTrackPt);
                    break;

                case TrainingVariableSet.DefaultAllpT:
                    result.Add(TrainingVariables.JetPt);
                    result.Add(TrainingVariables.JetPhi);
                    result.Add(TrainingVariables.CalRatio);
                    result.Add(TrainingVariables.NTracks);
                    result.Add(TrainingVariables.SumPtOfAllTracks);
                    result.Add(TrainingVariables.MaxTrackPt);
                    result.Add(TrainingVariables.JetWidth);
                    result.Add(TrainingVariables.EnergyDensity);
                    result.Add(TrainingVariables.HadronicLayer1Fraction);
                    result.Add(TrainingVariables.JetLat);
                    result.Add(TrainingVariables.JetLong);
                    result.Add(TrainingVariables.FirstClusterRadius);
                    result.Add(TrainingVariables.ShowerCenter);
                    result.Add(TrainingVariables.BIBDeltaTimingMinus);
                    result.Add(TrainingVariables.BIBDeltaTimingPlus);
                    result.Add(TrainingVariables.PredictedLz);
                    result.Add(TrainingVariables.PredictedLxy);
                    break;

                case TrainingVariableSet.Analysis2015pT:
                    result.Add(TrainingVariables.JetPt);
                    result.Add(TrainingVariables.JetPhi);
                    result.Add(TrainingVariables.CalRatio);
                    result.Add(TrainingVariables.NTracks);
                    result.Add(TrainingVariables.SumPtOfAllTracks);
                    result.Add(TrainingVariables.MaxTrackPt);
                    result.Add(TrainingVariables.JetWidth);
                    result.Add(TrainingVariables.JetTrackDR);
                    result.Add(TrainingVariables.EnergyDensity);
                    result.Add(TrainingVariables.HadronicLayer1Fraction);
                    result.Add(TrainingVariables.JetLat);
                    result.Add(TrainingVariables.JetLong);
                    result.Add(TrainingVariables.FirstClusterRadius);
                    result.Add(TrainingVariables.ShowerCenter);
                    result.Add(TrainingVariables.BIBDeltaTimingMinus);
                    result.Add(TrainingVariables.BIBDeltaTimingPlus);
                    break;

                case TrainingVariableSet.DefaultAllET:
                    result.Add(TrainingVariables.JetET);
                    result.Add(TrainingVariables.JetPhi);
                    result.Add(TrainingVariables.CalRatio);
                    result.Add(TrainingVariables.NTracks);
                    result.Add(TrainingVariables.SumPtOfAllTracks);
                    result.Add(TrainingVariables.MaxTrackPt);
                    result.Add(TrainingVariables.JetWidth);
                    result.Add(TrainingVariables.JetTrackDR);
                    result.Add(TrainingVariables.EnergyDensity);
                    result.Add(TrainingVariables.HadronicLayer1Fraction);
                    result.Add(TrainingVariables.JetLat);
                    result.Add(TrainingVariables.JetLong);
                    result.Add(TrainingVariables.FirstClusterRadius);
                    result.Add(TrainingVariables.ShowerCenter);
                    result.Add(TrainingVariables.BIBDeltaTimingMinus);
                    result.Add(TrainingVariables.BIBDeltaTimingPlus);
                    break;

                case TrainingVariableSet.None:
                    break;

                default:
                    throw new NotImplementedException();
            }

            // Additional variables
            foreach (var v in adds)
            {
                result.Add(v);
            }

            // Remove any that we want to drop
            foreach (var v in drops)
            {
                result.Remove(v);
            }

            return result;
        }
    }
}
