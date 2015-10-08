/*
Copyright 2015 Quicken Loans

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

namespace MessageRouter.Common.Reflection
{
  using Extractor = Func<AssemblyScanner,IEnumerable<Type>>;

  /// <summary>
  /// Helper for loading an Assembly and searching it for implementations of 
  /// Types related to message routing (e.g. ICommand, IHandleCommand<_>, etc.)
  /// </summary>
  public sealed class AssemblyScanner 
  {
    private Lazy<Assembly> loadFromFile (String fileName)
    {
      var rootCode = Assembly.GetExecutingAssembly();
      var rootPath = Path.GetDirectoryName (rootCode.CodeBase);
      var filePath = String.Format (@"{0}\{1}", rootPath, fileName)
                           .Replace(@"file:\" , "");

      return new Lazy<Assembly> (() => Assembly.LoadFrom(filePath)
                                ,isThreadSafe:false);
    }

    private readonly String fileName;
    private readonly Lazy<Assembly> assembly;

    /// <summary>
    /// Creates a new AssemblyScanner for the given assembly file
    /// </summary>
    /// <param name="fileName">The non-qualified assembly file name</param>
    public AssemblyScanner (String fileName)
    {
      this.fileName = fileName;
      this.assembly = loadFromFile(fileName);
    }

    /// <summary>
    /// The Assembly being scanned
    /// </summary>
    public Assembly Assembly { get { return this.assembly.Value; } }

    /// <summary>
    /// All types exported from the assembly being scanned
    /// </summary>
    public IEnumerable<Type> AllTypes { get { return this.assembly.Value.GetTypes(); } }

    /// <summary>
    /// Gets all non-interface types from the assembly being scanned 
    /// (Note: generic types are NOT considered concrete is they lack any generic arguments)
    /// </summary>
    /// <returns>A (potentially empty) sequence of matching Types</returns>
    public IEnumerable<Type> GetConcreteTypes()
    {
      return AllTypes.Where(TypeAnalyzer.IsConcrete);
    }

    /// <summary>
    /// Gets all concrete implementations of IHandleCommand<TCommand>
    /// </summary>
    /// <returns>A (potentially empty) sequence of matching Types</returns>
    public IEnumerable<Type> GetCommandHandlers()
    {
      return AllTypes.Where(TypeAnalyzer.IsCommandHandler);
    }

    /// <summary>
    /// Gets all concrete implementations of IHandleEvent<TEvent>
    /// </summary>
    /// <returns>A (potentially empty) sequence of matching Types</returns>
    public IEnumerable<Type> GetEventHandlers()
    {
      return AllTypes.Where(TypeAnalyzer.IsEventHandler);
    }

    /// <summary>
    /// Gets all concrete implementations of IHandleCommand<TCommand> or IHandleEvent<TEvent>
    /// </summary>
    /// <returns>A (potentially empty) sequence of matching Types</returns>
    public IEnumerable<Type> GetAllHandlers()
    {
      return AllTypes.Where(TypeAnalyzer.IsHandler);
    }

    /// <summary>
    /// Returns all types which implement ICommand
    /// </summary>
    /// <returns>A (potentially empty) sequence of matching Types</returns>
    public IEnumerable<Type> GetCommandMessages()
    {
      return AllTypes.Where(TypeAnalyzer.IsCommand);
    }

    /// <summary>
    /// Returns all types which implement IEvent
    /// </summary>
    /// <returns>A (potentially empty) sequence of matching Types</returns>
    public IEnumerable<Type> GetEventMessages()
    {
      return AllTypes.Where(TypeAnalyzer.IsEvent);
    }

    /// <summary>
    /// Returns all types which implement ICommand or IEvent
    /// </summary>
    /// <returns>A (potentially empty) sequence of matching Types</returns>
    public IEnumerable<Type> GetAllMessages()
    {
      return AllTypes.Where(TypeAnalyzer.IsMessage);
    }

    /// <summary>
    /// For each of a given sequence of assembly file names, loads the assembly
    /// and passes it to each of the given extractor functions for processing.
    /// Aggregates the results of all extractor functions into a single sequence.
    /// </summary>
    /// <param name="libraries">The non-qualified file names of assemblies to be processed</param>
    /// <param name="extractors">One or more functions which, given an AssemblyScanner, return types</param>
    /// <returns>A sequence of Type instances (merged from all extractor calls)</returns>
    public static IEnumerable<Type> Coalesce (IEnumerable<String> libraries
                                             ,params Extractor[] extractors)
    {
      var assemblies = libraries.Select (a => new AssemblyScanner(a));
      return extractors.SelectMany(extract => assemblies.SelectMany(extract))
                       .Distinct ();
    }
  }
}
