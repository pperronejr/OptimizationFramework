
//
// Copyright 2017 Paul Perrone.  All rights reserved.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IDA.Optimization
{
    public class Mutex
    {
        public Mutex(List<OptimizationItem> optItems)
        {
            OptimizationItems = optItems;
        }

        /// <summary>
        /// If true then enforce this Mutex else it is ignored.
        /// </summary>
        internal bool IsActive = false;

        public List<OptimizationItem> OptimizationItems = new List<OptimizationItem>();
        internal string MutexSetID
        {
            get { return (OptimizationItems.Count > 0) ? OptimizationItems[0].MutexSetID : "" ; }
        }

        private OptimizationItem activeOptItem;
        private OptimizationItem previousActiveOptItem;

        internal void Propagate(OptimizationItem selectedOptItem)
        {
            if (IsActive && (selectedOptItem != null))
            {
                if (OptimizationItems.Contains(selectedOptItem))
                {
                    previousActiveOptItem = activeOptItem;
                    activeOptItem = selectedOptItem;
                    OptimizationItems.ForEach(item => item.IsActive = false);
                    activeOptItem.IsActive = true;
                }
                else
                    // This exception may not be worth throwing since nothing will break if ignored.
                    throw new System.Exception("Trying to apply a Mutex relation but the selected item is not part of the Mutex.");
            }
        }
        /// <summary>
        /// Only support 1 level of undo
        /// </summary>
        internal void UndoPropagate()
        {
            if (IsActive)
            {
                Propagate(previousActiveOptItem);
                // Allow only one level of undo and prevent a cycle of undo and redo.
                previousActiveOptItem = activeOptItem;
            }
        }

        internal Random randomNumberGenerator = new Random();

        /// <summary>
        /// Returns the complete list of optItems which participate in Active Mutexes.
        /// </summary>
        /// <param name="optItems"></param>
        static internal List<OptimizationItem> GetMutexedOptimizationItems(List<Mutex> mutexes)
        {
            // Return a list of all optimization items which participate in the mutex relations.
            List<OptimizationItem> mutexedOptItems = new List<OptimizationItem>();
            mutexes.ForEach(mutex => { if (mutex.IsActive) mutexedOptItems.AddRange(mutex.OptimizationItems); });

            // Return the list without duplicates
            return mutexedOptItems.Distinct().ToList();
        }

        /// <summary>
        /// Creates and returns a list of Mutex objects representing all the mutex sets of optItems where
        /// the number of optItems participating in a mutex is greater than 1.  OptimizationItem.Mutexes
        /// field is also set accordingly in all optItems.
        /// 
        /// This is a simplistic implementation of Mutexes. An OptItem should only appear in a single 
        /// mutex to insure that the resulting behavior is correct.
        /// </summary>
        /// <param name="optItems"></param>
        /// <returns></returns>
        static public List<Mutex> GetMutexes(List<OptimizationItem> optItems)
        {
            List<OptimizationItem> mutexedOptItems = new List<OptimizationItem>();
            List<Mutex> mutexes = new List<Mutex>();

            // Collect all optItems with a MutexSetID that is not null or an empty string ("").

            optItems.ForEach(optItem =>
            {
                if ((optItem.MutexSetID != null) && (optItem.MutexSetID != ""))
                    mutexedOptItems.Add(optItem);
            }
            );

            // Group annots by MutexSetID

            var mutexGroups = mutexedOptItems.GroupBy(optItem => optItem.MutexSetID);

            // Collect groups as MutexAnnotation objects if they have more than one element.

            foreach (var mutexGroup in mutexGroups)
            {
                mutexedOptItems = mutexGroup.ToList();
                if (mutexedOptItems.Count > 1)
                    mutexes.Add( new Mutex(mutexedOptItems));
            }

            return mutexes;
        }
    }
}
