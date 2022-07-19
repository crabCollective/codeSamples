using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts.Core.Pooling
{
    /// <summary>
    /// Use this interface on objects that need to be pooled
    /// </summary>
    public interface IPoolable {
        /// <summary>
        /// This event is meant to be called when returning object to pool. Dont forget to set it to null when OnDestroy
        /// </summary>
        public event Action<IPoolable> onPoolReturning;
        /// <summary>
        /// This property must be overriden by particular class-related enum
        /// </summary>
        /// <value></value>
        public System.Enum Type {get; set;}
        /// <summary>
        /// Call this method to return object back to pool, typically handled by assigned factory
        /// </summary>
        void ReturnToPool();
        /// <summary>
        /// Call this method when object is out of pool
        /// </summary>
        void OnPoolGet();
        /// <summary>
        /// Call this method when object is returned back to pool
        /// </summary>
        void OnPoolRelease();
    }
}
