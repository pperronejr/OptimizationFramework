
//
// Copyright 2017 Paul Perrone.  All rights reserved.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IDA.Optimization
{
    /// <summary>
    /// Search Operator protocol used in OptimizationSolution. 
    /// A Search Operator contains an algorithm that searches a system's
    /// solution space in order to define an improved system.
    /// </summary>
    abstract public class SearchOperator
    {
        abstract public void Optimize();
        /// <summary>
        /// Define this method in a derived class if there are any fields in the derived class
        /// that depend on OptSolution.  These dependent fields should be initialized in this 
        /// method. OptSolution is set by the OptimizationSolution class in which a SearchOperator
        /// object is used.  This method is called after the OptSolution is set.
        /// </summary>
        protected virtual void InitOptSolutionDependents()
        { }

        /// <summary>
        /// When set to True, Mutexes are activated by the OptimizationSolution just before it fires
        /// Optmize in this search operator.  If false then the mutexes are deactivated.
        /// </summary>
        public bool IncludeMutexes = false;

        /// <summary>
        /// If any fields depend on OptimizationSolution then they should be initialized
        /// in the method InitOptSolutionDependents.
        /// </summary>
        private OptimizationSolution optSolution;
        public OptimizationSolution OptSolution
        {
            get { return optSolution; }
            set
            {
                optSolution = value;
                InitOptSolutionDependents();
            }
        }
    }
}
