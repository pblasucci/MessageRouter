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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MessageRouter.Common
{
  /// <summary>
  /// Adds useful helpers to System.Type instances
  /// </summary>
  public static class TypeAnalyzer
  {
    /// <summary>
    /// Tests if a type implements a given target interface, taking generic
    /// arguments and generic parameters into account
    /// </summary>
    /// <param name="test">Implementing Type</param>
    /// <param name="target">Type which defines an interface</param>
    /// <returns>true if test implments target, false otherwise</returns>
    public static Boolean HasStrictInterface (this Type test, Type target)
    {
      var face = test.GetInterface(target.Name,true);
      if (face != null)
      {
        return target.GetGenericArguments()
                     .SequenceEqual(face.GetGenericArguments());
      }
      return false;
    }

    /// <summary>
    /// Tests if a type implements a given target interface, taking generic
    /// arguments and generic parameters into account
    /// </summary>
    /// <typeparam name="TInterface">Target Interface</typeparam>
    /// <param name="test">Implementing Type</param>
    /// <returns>true if test implments target, false otherwise</returns>
    public static Boolean HasStrictInterface<TInterface> (this Type test)
    {
      return HasStrictInterface(test, typeof(TInterface));
    }

    /// <summary>
    /// Checks if a type  has generic arguments for all of it generic parameters
    /// (Note: types with no generic parameters return false)
    /// </summary>
    /// <param name="test">Type to analyze</param>
    /// <returns>true if check passes, false otherwise</returns>
    public static Boolean IsClosedGeneric (this Type test)
    {
      if (test.IsGenericType && !test.IsGenericTypeDefinition)
      {
        var genArgs = test.GetGenericArguments();
        return (genArgs != null && genArgs.Length > 0);
      }

      return false;
    }

    /// <summary>
    /// Checks that a Type is not an interface. Additionally if the type is
    /// generic, checks that all generic parameters have arguemnts.
    /// </summary>
    /// <param name="test">the Type being analyzed</param>
    /// <returns>true if the Type matches, false otherwise</returns>
    public static Boolean IsConcrete(this Type test)
    {
      return (!test.IsInterface) 
          && (test.IsGenericType ? test.IsClosedGeneric() : true);
    }
    
    // Type should implement a message interface exactly once (or not at all)
    private static Boolean messageOf (this Type test, params Type[] targets)
    {
      var query = from targ in targets
                  from subj in test.GetInterfaces()
                  where   !subj.IsGenericType
                  select  subj == targ;
      return query.SingleOrDefault (result => result);
    }

    // Type should implement a handler interface exactly once (or not at all)
    private static Boolean handlerFor (this Type test, params Type[] targets)
    {
      if (test.IsConcrete())
      {
        var query = from targ in targets
                    from subj in test.GetInterfaces()
                    where   subj.IsGenericType
                    select  subj.GetGenericTypeDefinition() == targ;
        return query.SingleOrDefault (result => result);
      }
      return false;
    }

    /// <summary>
    /// Checks if a Type implements or inherits from ICommand
    /// </summary>
    /// <param name="test">the Type being analyzed</param>
    /// <returns>true if the Type matches, false otherwise</returns>
    public static Boolean IsCommand (this Type test) 
    {
      return test.messageOf(typeof(ICommand));
    }

    /// <summary>
    /// Checks if a Type implements or inherits from IEvent
    /// </summary>
    /// <param name="test">the Type being analyzed</param>
    /// <returns>true if the Type matches, false otherwise</returns>
    public static Boolean IsEvent (this Type test) 
    {
      return test.messageOf(typeof(IEvent));
    }

    /// <summary>
    /// Checks if a Type implements or inherits from ICommand or IEvent
    /// </summary>
    /// <param name="test">the Type being analyzed</param>
    /// <returns>true if the Type matches, false otherwise</returns>
    public static Boolean IsMessage (this Type test) 
    {
      return test.messageOf(typeof(ICommand), typeof(IEvent));
    }

    /// <summary>
    /// Checks if a concrete Type implements IHandleCommand<TCommand>
    /// </summary>
    /// <param name="test">the Type being analyzed</param>
    /// <returns>true if the Type matches, false otherwise</returns>
    public static Boolean IsCommandHandler (this Type test)
    {
      return test.handlerFor(typeof(IHandleCommand<>));
    }

    /// <summary>
    /// Checks if a concrete Type implements IHandleEvent<TEvent>
    /// </summary>
    /// <param name="test">the Type being analyzed</param>
    /// <returns>true if the Type matches, false otherwise</returns>
    public static Boolean IsEventHandler (this Type test)
    {
      return test.handlerFor(typeof(IHandleEvent<>));
    }

    /// <summary>
    /// Checks if a concrete Type implements IHandleCommand<TCommand> or IHandleEvent<TEvent>
    /// </summary>
    /// <param name="test">the Type being analyzed</param>
    /// <returns>true if the Type matches, false otherwise</returns>
    public static Boolean IsHandler (this Type test)
    {
      return test.handlerFor(typeof(IHandleCommand<>), typeof(IHandleEvent<>));
    }
  }
}
