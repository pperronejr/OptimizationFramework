
//
// Copyright 2017 Paul Perrone.  All rights reserved.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace IDA.Optimization
{
    public class SimulatedAnnealing : SearchOperator
    {
        public SimulatedAnnealing()
        {
            IncludeMutexes = true; 
        }

        // --- Public fields which can be set to control the search operation ---

        public double TInitial = 0.0;
        public double TReductionFactor = 0.9;
        public int TQuantityMax = 50;
        public int TrialQuantityMaxFactor = 20;
        public int ImprovedTrialQuantityMaxFactor = 5;
        // Max search time in seconds tested at the outer temperature loop of the search.  If 0, then the search time is unlimited.
        public int SearchTimeMax = 0;

        // ----------------------------------------------------------------------

        private int trialQuantityMax;
        private int improvedTrialQuantityMax;   

        private Random randomNumberGenerator;

        protected override void InitOptSolutionDependents()
        {
            base.InitOptSolutionDependents();

            trialQuantityMax = TrialQuantityMaxFactor * OptSolution.OptimizationItems.Count;
            improvedTrialQuantityMax = ImprovedTrialQuantityMaxFactor * OptSolution.OptimizationItems.Count;
            randomNumberGenerator = OptSolution.randomNumberGenerator;
        }

        // Initialize Optimize() fields outside of the method so they are visible to 
        // the SimulatedAnnealingWriter which has access to this object

        internal double T;
        internal int TQuantity;
        internal int TrialQuantity;
        internal int ImprovedTrialQuantity;
        internal int EquivalentTrialQuantity;
        internal int DegradedTrialQuantity;
        internal int TotalTrialQuantity;

        internal double ObjFuncPrev;
        internal double ObjFunc;
        internal double ObjFuncDelta;
        internal double TrialAcceptanceProbability;

        internal bool IsSolutionImproved;
        internal bool IsSolutionEquivalent;
        internal bool IsSolutionDegraded;
        internal bool KeepSolution;

        public override void Optimize()
        {
            T = TInitial/TReductionFactor;
            TQuantity = 0;
            TrialQuantity = 0;
            ImprovedTrialQuantity = 0;
            EquivalentTrialQuantity = 0;
            DegradedTrialQuantity = 0;
            TotalTrialQuantity = 0;


            // Properly initialize all Independent Variables randomly.  Initial Independent Variable
            // values from construction of OptimizationItem objects are ignored.
            OptSolution.OptimizationItems.ForEach(optItem => OptSolution.PerturbItemRandomly( optItem));

            ObjFunc = OptSolution.ObjectiveFunction();

            SimulatedAnnealingWriter logWriter = new SimulatedAnnealingWriter(this);

            Stopwatch stopWatch = Stopwatch.StartNew();

            // Instead of checking for degradedTrialQuantity, might want to save a copy of
            // the best trial and use that when the search converges if it ends up being better
            // than the final result.
            while ((TQuantity < TQuantityMax) &&
                ((ImprovedTrialQuantity > 0) || (DegradedTrialQuantity > 0) || (TQuantity == 0)) &&
                (ObjFunc > OptSolution.ObjectiveFunctionGoal) &&
                ((SearchTimeMax == 0) || (stopWatch.Elapsed.TotalSeconds < SearchTimeMax)))
            {
                T *= TReductionFactor;
                TQuantity++;
                TrialQuantity = 0;
                ImprovedTrialQuantity = 0;
                EquivalentTrialQuantity = 0;
                DegradedTrialQuantity = 0;

                logWriter.WriteTemperatureHeader();

                do
                {
                    logWriter.WriteTrialColumnHeadings();

                    TrialQuantity++;
                    TotalTrialQuantity++;

                    OptSolution.PerturbSolutionRandomly();

                    ObjFuncPrev = ObjFunc;
                    ObjFunc = OptSolution.ObjectiveFunction();
                    ObjFuncDelta = ObjFunc - ObjFuncPrev;

                    IsSolutionImproved = (ObjFuncDelta < 0.0);
                    IsSolutionEquivalent = (ObjFuncDelta == 0.0);
                    IsSolutionDegraded = (ObjFuncDelta > 0.0);

                    TrialAcceptanceProbability =
                        (ObjFuncDelta <= 0.0) ? 1.0 : System.Math.Exp(-ObjFuncDelta / T);

                    KeepSolution =
                        (IsSolutionImproved) ||
                        (IsSolutionEquivalent && (randomNumberGenerator.NextDouble() <= 0.5)) ||
                        (IsSolutionDegraded && (randomNumberGenerator.NextDouble() < TrialAcceptanceProbability));

                    if (IsSolutionImproved) ImprovedTrialQuantity++;
                    if (IsSolutionEquivalent && KeepSolution) EquivalentTrialQuantity++;          
                    if (IsSolutionDegraded && KeepSolution) DegradedTrialQuantity++;

                    logWriter.WriteTrialResults();

                    if (!KeepSolution)
                    {
                        OptSolution.UndoPerturbedItem();
                        ObjFunc = OptSolution.ObjectiveFunction();
                    }
                }
                while ((TrialQuantity < trialQuantityMax) &&
                    (ImprovedTrialQuantity < improvedTrialQuantityMax));

                logWriter.WriteTemperatureSummary();
            }
            stopWatch.Stop();

            logWriter.WriteSearchResultsSummary();
        }
    }
}
