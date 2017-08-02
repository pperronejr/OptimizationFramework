
//
// Copyright 2017 Paul Perrone.  All rights reserved.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IDA.Optimization
{
    internal class SimulatedAnnealingWriter
    {
        internal SimulatedAnnealingWriter( SimulatedAnnealing simAnnealing)
        {
            simulatedAnnealing = simAnnealing;
            log = simulatedAnnealing.OptSolution.Log;
            doLogging = simulatedAnnealing.OptSolution.DoLogging;
            detailedLogging = simulatedAnnealing.OptSolution.DetailedLogging;
        }

        private SimulatedAnnealing simulatedAnnealing;

        private OptimizationLogging log;
        private bool doLogging;
        /// <summary>
        /// If this property is set to true, logging output will contain details for each trial 
        /// in all temperature phases else just a search results summary is logged.
        /// </summary>
        private bool detailedLogging;

        private int columnHeadingsFrequency = 40;

        internal void WriteTemperatureHeader()
        {
            // Adding check for optSolution.Log.DoLogging was not necessary.
            // It was added for potential performance improvement so that
            // string manipulation won't take place if logging is not required
            if (doLogging && detailedLogging)
            {
                log.WriteLineToLog
                    (
                    "*******************************************************************************************************" + System.Environment.NewLine +
                    System.Environment.NewLine +
                    String.Format("{0,3}. Temperature = {1,9:0.000000}", simulatedAnnealing.TQuantity, simulatedAnnealing.T) + System.Environment.NewLine +
                    System.Environment.NewLine +
                    "*******************************************************************************************************" +
                    System.Environment.NewLine
                    );
            }
        }
        internal void WriteTrialColumnHeadings()
        {
            if (doLogging && detailedLogging && ((simulatedAnnealing.TrialQuantity % columnHeadingsFrequency) == 0))
            {
                log.WriteLineToLog
                    (
                    System.Environment.NewLine +
                    "Trial # Improved # Degraded #   Equiv #    Result    Verdict      ObjFunc       ObjFunc_Prev   ObjFunc_Delta    Probability" + System.Environment.NewLine +
                    "------- ---------- ---------- ---------- ---------- --------- --------------- --------------- --------------- ---------------"
                    );
            }
        }
        internal void WriteTrialResults()
        {
            if (doLogging && detailedLogging)
            {
                string result;
                if (simulatedAnnealing.IsSolutionImproved)
                    result = "Improved";
                else if (simulatedAnnealing.IsSolutionEquivalent)
                    result = "Equal";
                else if (simulatedAnnealing.IsSolutionDegraded)
                    result = "Degraded";
                else
                    result = "";

                string verdict = simulatedAnnealing.KeepSolution ? "Keep" : "Revert";

                string improvedTrialIndex = 
                    simulatedAnnealing.IsSolutionImproved ? 
                    simulatedAnnealing.ImprovedTrialQuantity.ToString() : "";
                string degradedTrialIndex =
                    (simulatedAnnealing.IsSolutionDegraded && simulatedAnnealing.KeepSolution) ? 
                    simulatedAnnealing.DegradedTrialQuantity.ToString() : "";
                string equivTrialIndex =
                    (simulatedAnnealing.IsSolutionEquivalent && simulatedAnnealing.KeepSolution) ? 
                    simulatedAnnealing.EquivalentTrialQuantity.ToString() : "";

                log.WriteLineToLog
                    (
                    String.Format("{0,7}{1,11}{2,11}{3,11}{4,11}{5,10}{6,16:0.000000}{7,16:0.000000}{8,16:0.000000}{9,16:G6}",
                    new Object[10] { simulatedAnnealing.TrialQuantity, improvedTrialIndex, degradedTrialIndex, 
                        equivTrialIndex, result, verdict, simulatedAnnealing.ObjFunc, simulatedAnnealing.ObjFuncPrev, 
                        simulatedAnnealing.ObjFuncDelta, simulatedAnnealing.TrialAcceptanceProbability})
                    );
            }
        }
        internal void WriteTemperatureSummary()
        {
            if (doLogging)
            {
                int index = simulatedAnnealing.TQuantity;
                double temperature = simulatedAnnealing.T;
                double objFunc = simulatedAnnealing.ObjFunc;
                int trials = simulatedAnnealing.TrialQuantity;
                int improvedTrials = simulatedAnnealing.ImprovedTrialQuantity;
                int degradedTrials = simulatedAnnealing.DegradedTrialQuantity;
                int equivalentTrials = simulatedAnnealing.EquivalentTrialQuantity;
                int revertedTrials = trials - improvedTrials - degradedTrials - equivalentTrials;
                int totalTrials = simulatedAnnealing.TotalTrialQuantity;

                if (detailedLogging)
                {
                    log.WriteLineToLog
                        (
                        System.Environment.NewLine +
                        System.Environment.NewLine +
                        "Summary" + System.Environment.NewLine +
                        "-------" + System.Environment.NewLine +
                        System.Environment.NewLine +
                        String.Format("{0,3}. Temperature = {1,9:0.000000}", index, temperature) + System.Environment.NewLine +
                        System.Environment.NewLine +
                        String.Format("{0,24}{1,12:0.000000}", "Objective Function:", objFunc) + System.Environment.NewLine +
                        System.Environment.NewLine +
                        String.Format("{0,24}{1,12}", "Improved Solns:", improvedTrials) + System.Environment.NewLine +
                        String.Format("{0,24}{1,12}", "Degraded Solns:", degradedTrials) + System.Environment.NewLine +
                        String.Format("{0,24}{1,12}", "Equivalent Solns:", equivalentTrials) + System.Environment.NewLine +
                        String.Format("{0,24}{1,12}", "Reverted Solns:", revertedTrials) + System.Environment.NewLine +
                        String.Format("{0,24}{1,12}", "Trials:", trials) + System.Environment.NewLine +
                        System.Environment.NewLine +
                        String.Format("{0,24}{1,12}", "Total Trials:", totalTrials) + System.Environment.NewLine +
                        System.Environment.NewLine
                        );
                }

                // Collect temperature summary results for summary output once the Simulated Annealing search operation is complete.
                // Collect formatted string summarizing results for the current temperature which coincide in format to 
                // temperatureSummaryHeading.
                temperatureResults.Add
                    (
                    String.Format("{0,5}{1,16:0.000000}{2,11}{3,11}{4,11}{5,11}{6,11}{7,11}{8,16:0.000000}",
                    new Object[9] { index, objFunc, totalTrials, trials, improvedTrials, degradedTrials, 
                        equivalentTrials, revertedTrials, temperature })
                    );
            }
        }
        internal void WriteSearchResultsSummary()
        {
            if (doLogging)
            {
                if (temperatureResults.Count > 0)
                {
                    log.WriteLineToLog(temperatureSummaryHeading);
                    foreach (string tr in temperatureResults)
                    {
                        log.WriteLineToLog(tr);
                    }
                }
                else
                {
                    log.WriteLineToLog("Search was not necessary based on entry conditions");
                }
            }
        }

        private List<string> temperatureResults = new List<string>();
        private static string temperatureSummaryHeading =
            "*******************************************************************************************************" + System.Environment.NewLine +
            System.Environment.NewLine +
            System.Environment.NewLine +
            "Search Results Summary" + System.Environment.NewLine +
            "----------------------" + System.Environment.NewLine +
            System.Environment.NewLine +
            System.Environment.NewLine +
            "Index     ObjFunc       # Total   # Trials  # Improved # Degraded   # Equiv  # Reverted   Temperature  " + System.Environment.NewLine +
            "----- --------------- ---------- ---------- ---------- ---------- ---------- ---------- ---------------";

    }
}
