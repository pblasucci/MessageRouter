using System;

namespace MessageRouter.Interfaces
{
    /// <summary>
    /// Provides a minimal complete set of operations for reflectively loading CLR types
    /// </summary>
    public interface IResolver
    {
        /// <summary>
        /// Determines if the resolver knows how to instantiate the given type
        /// </summary>
        /// <param name="type">The type to be instantiated</param>
        /// <returns>True if the type may be instantiated, false otherwise</returns>
        bool CanResolve(Type type);

        /// <summary>
        /// Gets a type from the resolver, using compile-time type information
        /// </summary>
        /// <typeparam name="T">The type to be retrieved</typeparam>
        /// <returns>An instance of the desired type</returns>
        T Get<T>();
      
        /// <summary>
        /// Gets a type from the resolver, using run-time type information
        /// </summary>
        /// <param name="type">The type to be retrieved</param>
        /// <returns>An instance of the desired type</returns>
        object Get(Type type);
    }
}
