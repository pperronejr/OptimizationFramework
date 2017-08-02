
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
    /// Orchestrates optimization of a system by applying a series of 
    /// SearchOperators to a set of OptimizationItems.
    /// </summary>
    public class OptimizationSolution
    {
        public OptimizationSolution(List<SearchOperator> searchOps)
        {
            searchOperators = searchOps;

            ObjectiveFunctionGoal = 0.0;
            DoLogging = true;
            DetailedLogging = false;
        }
        public OptimizationSolution
            (List<SearchOperator> searchOps, List<OptimizationItem> optItems, List<ObjectiveItem> objItems) :
            this(searchOps)
        {
            OptimizationItems = optItems;
            ObjectiveItems = objItems;
        }
        public OptimizationSolution
            (List<SearchOperator> searchOps, List<OptimizationItem> optItems) :
            this(searchOps, optItems, ObjectiveItems<OptimizationItem>.Convert(optItems))
        { }

        public double ObjectiveFunctionGoal { get; set; }
        public bool DoLogging
        {
            get { return Log.DoLogging; }
            set { Log.DoLogging = value; }
        }
        public bool DetailedLogging
        {
            get { return Log.DetailedLogging; }
            set { Log.DetailedLogging = value; }
        }

        /// <summary>
        /// Optimize performs the optimization specified by searchOperators which contains an 
        /// ordered list of SearchOperators to apply to OptimizationItems.  This will side effect 
        /// all of the OptimizationItems provided.
        /// </summary>
        public void Optimize()
        {
            try
            {
                if (OptimizationItems.Count > 0)
                    foreach (SearchOperator searchOp in searchOperators)
                    {
                        if (searchOp.IncludeMutexes)
                            ActivateMutexes();
                        else
                            DeactivateMutexes();
                        searchOp.OptSolution = this;
                        searchOp.Optimize();
                    }
                else
                    Log.LogWarning("Optimization not performed since there are no OptimizationItems", this);
            }
            catch (OptimizationItemNoValidValueException e)
            {
                Log.LogError("Optimization not completed." + System.Environment.NewLine + e.Message, this);
            }
        }

        /// <summary>
        /// This method is used as a conditional check to validate that the system is in a valid state.
        /// This method should be specialized if the subsect system has constraints or performance
        /// criteria that need to be met.
        /// </summary>
        /// <returns></returns>
        protected internal virtual bool IsValid()
        {
            return UniqueSolutionItems.All(solnItem => solnItem.IsValid());
        }

        public virtual double ObjectiveFunction()
        {
            double sum = 0.0;
            foreach (ObjectiveItem objItem in ObjectiveItems)
            {
                sum += objItem.GetCost();
            }
            return sum;
        }

        internal OptimizationItem RandomOptimizationItem()
        {
            int index = randomNumberGenerator.Next(0, OptimizationItems.Count);
            return OptimizationItems[index];
        }

        internal void PerturbSolutionRandomly()
        {
            PerturbItemRandomly( RandomOptimizationItem());
        }

        internal void PerturbItemRandomly(OptimizationItem optItem)
        {
            optItem.PropagateMutexes();
            optItem.SetIndependentVariableRandomly();
            PerturbedItem = optItem;
        }

        internal void UndoPerturbedItem()
        {
            PerturbedItem.UndoIndependentVariable();
            PerturbedItem.UndoPropagateMutexes();
            PerturbedItem = null;
        }

        /// <summary>
        /// List of SearchOperator objects in the order that they should 
        /// be applied to the system of optItems
        /// </summary>
        private List<SearchOperator> searchOperators;

        /// <summary>
        /// List of optimization items to be included in the optimization search.
        /// Defined as virtual in case the set of items changes during a search operation
        /// in which case this can be redefined in a specialized solution.
        /// </summary>
        public virtual List<OptimizationItem> OptimizationItems
        {
            get
            {
                return optimizationItems.FindAll(optItem => ((optItem.IsActive && !optItem.IsConstant) || optItem.IsMutexed));
            }
            set 
            { 
                optimizationItems = value;

                // Set up mutexes.

                mutexes = Mutex.GetMutexes(optimizationItems);

                // Update the Mutexes field in all optItems to include applicable Mutex objects

                // Reset Mutexes field in all optItems to an empty list
                optimizationItems.ForEach(optItem => optItem.Mutexes = new List<Mutex>());
                // Set Mutexes field in each optItem to have the appopriate list of mutexes
                mutexes.ForEach(mutex =>
                    mutex.OptimizationItems.ForEach(optItem =>
                        optItem.Mutexes.Add(mutex)));
            }
        }
        private List<OptimizationItem> optimizationItems;

        /// <summary>
        /// List of objective items to be included in the Objective Function 
        /// calculation.
        /// Defined as virtual in case the set of items changes during a search operation
        /// in which case this can be redefined in a specialized solution.
        /// </summary>
        public virtual List<ObjectiveItem> ObjectiveItems 
        {
            get
            {
                return objectiveItems.FindAll(objItem => objItem.IsActive);
            }
            set { objectiveItems = value; } 
        }
        private List<ObjectiveItem> objectiveItems;

        /// <summary>
        /// A combined list of all the unique items from both OptimizationItems and ObjectiveItems.
        /// This is created so operations that need to work on both lists only consider each unique
        /// object once rather than duplicating operations such for the IsValid check.
        /// The list of items are recalculated everytime to accomodate the scenarion when the
        /// sets of optimization or objective items may change during a search.
        /// </summary>
        internal List<ObjectiveItem> UniqueSolutionItems
        {
            get 
            {
                List<ObjectiveItem> uniqueSolutionItems = new List<ObjectiveItem>();
                uniqueSolutionItems.AddRange(ObjectiveItems<OptimizationItem>.Convert(OptimizationItems));
                ObjectiveItems.ForEach(
                    objItem =>
                    {
                        if (!uniqueSolutionItems.Contains(objItem))
                            uniqueSolutionItems.Add(objItem);
                    });
                return uniqueSolutionItems;
            }
        }

        private List<Mutex> mutexes;

        internal void ActivateMutexes()
        {
            mutexes.ForEach(mutex => mutex.IsActive = true);
        }
        internal void DeactivateMutexes()
        {
            mutexes.ForEach(mutex => mutex.IsActive = false);
        }

        internal OptimizationItem PerturbedItem;

        internal OptimizationLogging Log = new OptimizationLogging();
        internal Random randomNumberGenerator = new Random();
    }
}
