﻿//-----------------------------------------------------------------------
// <copyright file="IMachine.cs">
//      Copyright (c) Microsoft Corporation. All rights reserved.
//
//      THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
//      EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//      MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
//      IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
//      CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
//      TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
//      SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.PSharp.Runtime
{
    /// <summary>
    /// Interface of a P# machine. It provides APIs for accessing internal machine
    /// functionality. This interface is only for internal consumption.
    /// </summary>
    internal interface IMachine
    {
        /// <summary>
        /// The unique machine id.
        /// </summary>
        MachineId Id { get; }

        /// <summary>
        /// Stores machine-related information, which can used
        /// for scheduling and testing.
        /// </summary>
        MachineInfo Info { get; }

        /// <summary>
        /// The unique name of this machine.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Dictionary containing all the current goto state transitions.
        /// </summary>
        Dictionary<Type, GotoStateTransition> GotoTransitions { get; }

        /// <summary>
        /// Dictionary containing all the current push state transitions.
        /// </summary>
        Dictionary<Type, PushStateTransition> PushTransitions { get; }

        /// <summary>
        /// Gets the <see cref="Type"/> of the current state.
        /// </summary>
        Type CurrentState { get; }

        /// <summary>
        /// Gets the name of the current state.
        /// </summary>
        string CurrentStateName { get; }

        /// <summary>
        /// Enqueues the specified <see cref="EventInfo"/>.
        /// </summary>
        /// <param name="eventInfo">The event metadata.</param>
        /// <returns>
        /// Task that represents the asynchronous operation. The task result
        /// is the machine status after the enqueue.
        /// </returns>
        Task<MachineStatus> EnqueueAsync(EventInfo eventInfo);

        /// <summary>
        /// Transitions to the start state, and executes the
        /// entry action, if there is any.
        /// </summary>
        /// <param name="e">Event</param>
        /// <returns>Task that represents the asynchronous operation.</returns>
        Task GotoStartStateAsync(Event e);

        /// <summary>
        /// Runs the event handler. The handler terminates if there
        /// is no next event to process or if the machine is halted.
        /// </summary>
        /// <returns>Task that represents the asynchronous operation.</returns>
        Task RunEventHandlerAsync();

        /// <summary>
        /// Returns the cached state of this machine.
        /// </summary>
        /// <returns>Hash value</returns>
        int GetCachedState();

        /// <summary>
        /// Returns the set of all states in the machine (for code coverage).
        /// </summary>
        /// <returns>Set of all states in the machine.</returns>
        HashSet<string> GetAllStates();

        /// <summary>
        /// Returns the set of all (state, registered event) pairs in the machine (for code coverage).
        /// </summary>
        /// <returns>Set of all (state, registered event) pairs in the machine.</returns>
        HashSet<(string state, string e)> GetAllStateEventPairs();

        /// <summary>
        /// Returns the current state transition. Used for code coverage.
        /// </summary>
        /// <param name="eventInfo">The metadata of the event that caused the current transition.</param>
        (string machine, string originState, string destState, string edgeLabel) GetCurrentStateTransition(EventInfo eventInfo);
    }
}