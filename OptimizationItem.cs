
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
    /// Protocol for any class to be used as OptimizationItems in an 
    /// OptimizationSolution.
    /// 
    /// </summary>
    /// 
    /// Based on some practical use, evaluate if it makes sense to have
    /// the IOptimizationItem interface not be based on IObjectiveItem so 
    /// that distincltly different items can be used as OptimizationItems and
    /// not necessarily ObjectiveItems.  
    /// If this changes, a OptimizationSolution constructor and class OptimizationItems<T> 
    /// will need to be reviewed.  Any methods or properties in IObjectiveItem that are
    /// inherited by IOptimizationItem will need to also be defined here if these interfaces 
    /// are made independent.
    abstract public class OptimizationItem : ObjectiveItem
    {
        /// <summary>
        /// Should not be called directly in a SearchOperator if mutex relationships are active.
        /// Use methods in OptimizationSolution for adjusting independent variable values in this case.
        /// </summary>
        abstract public void SetIndependentVariableRandomly();
        /// <summary>
        /// Should not be called directly in a Search Operator if mutex relationships are active.
        /// Use methods in OptimizationSolution for undoing independent variable values in this case.
        /// </summary>
        abstract public void UndoIndependentVariable();

        /// <summary>
        /// When (IsConstant = true) an OptimizationItem is no longer considered as a variable.
        /// This can be set during a search operation and it will be refelected properly in the 
        /// OptimizationSolution.OptimizationItems.  This does not have an impact on 
        /// OptimizationSolution.ObjectiveItems.
        /// 
        /// This property should be defaulted to false.
        /// 
        /// This field should preferably be public.  It was changed to protected internal
        /// so a public version can be defined in a class that inherits this so its XML attributes
        /// can be specialized.  If a new public propery with the same name is defined in an inheriting
        /// class then its get and set should reference this field (see IDA.DAPO.Annotation).
        /// When DAPO is rewritten to separate out the XML serialization then this field should be made public.
        /// </summary>
        protected internal bool IsConstant = false;

        /// <summary>
        /// MutexSetID groups OptimizationItems where only one can be included in the drawing at a time.
        /// This is done as a preprocessing step for a search operation if that search operation should
        /// consider mutual exclusion rules which are defined by class MutexOptimizationItem.
        /// 
        /// This property should be defaulted to "" or null.
        /// 
        /// This field should preferably be public.  It was changed to protected internal
        /// so a public version can be defined in a class that inherits this so its XML attributes
        /// can be specialized.  If a new public propery with the same name is defined in an inheriting
        /// class then its get and set should reference this field (see IDA.DAPO.Annotation).
        /// When DAPO is rewritten to separate out the XML serialization then this field should be made public.
        /// </summary>
        protected internal string MutexSetID;

        /// <summary>
        /// List of mutex relationship objects which is set via Mutex.SetMutexes
        /// </summary>
        internal List<Mutex> Mutexes = new List<Mutex>();

        internal bool HasMutexes
        {
            get { return (Mutexes.Count > 0); }
        }

        internal bool IsMutexed
        {
            get { return !IsActive && HasMutexes && Mutexes.Exists(mutex => mutex.IsActive); }
        }

        internal void PropagateMutexes()
        {
            Mutexes.ForEach(mutex => mutex.Propagate(this));
        }
        /// <summary>
        /// Supports one level of undo
        /// </summary>
        internal void UndoPropagateMutexes()
        {
            Mutexes.ForEach(mutex => mutex.UndoPropagate());
        }
    }
}
