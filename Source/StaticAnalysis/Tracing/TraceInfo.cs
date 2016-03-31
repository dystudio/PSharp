﻿//-----------------------------------------------------------------------
// <copyright file="TraceInfo.cs">
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
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.PSharp.StaticAnalysis
{
    /// <summary>
    /// Static trace information.
    /// </summary>
    internal class TraceInfo
    {
        #region fields

        /// <summary>
        /// Error trace.
        /// </summary>
        internal List<ErrorTraceStep> ErrorTrace;

        /// <summary>
        /// Call trace.
        /// </summary>
        internal List<CallTraceStep> CallTrace;

        /// <summary>
        /// The method where the trace begins.
        /// </summary>
        internal string Method;

        /// <summary>
        /// The machine where the trace begins.
        /// </summary>
        internal string Machine;

        /// <summary>
        /// The state where the trace begins.
        /// </summary>
        internal string State;

        /// <summary>
        /// The corresponding payload of the trace.
        /// </summary>
        internal string Payload;

        #endregion

        #region methods

        /// <summary>
        /// Constructor.
        /// </summary>
        internal TraceInfo()
        {
            this.ErrorTrace = new List<ErrorTraceStep>();
            this.CallTrace = new List<CallTraceStep>();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="method">Method</param>
        /// <param name="machine">Machine</param>
        /// <param name="state">State</param>
        /// <param name="payload">Payload</param>
        internal TraceInfo(MethodDeclarationSyntax method, StateMachine machine,
            MachineState state, ExpressionSyntax payload)
        {
            this.ErrorTrace = new List<ErrorTraceStep>();
            this.CallTrace = new List<CallTraceStep>();

            if (method == null)
            {
                this.Method = null;
            }
            else
            {
                this.Method = method.Identifier.ValueText;
            }

            if (machine == null)
            {
                this.Machine = null;
            }
            else
            {
                this.Machine = machine.Name;
            }

            if (state == null)
            {
                this.State = null;
            }
            else
            {
                this.State = state.Name;
            }

            if (payload == null)
            {
                this.Payload = null;
            }
            else
            {
                this.Payload = payload.ToString();
            }
        }

        /// <summary>
        /// Adds new error trace information to the trace.
        /// </summary>
        /// <param name="expr">Expression</param>
        /// <param name="file">File</param>
        /// <param name="line">Line</param>
        internal void AddErrorTrace(string expr, string file, int line)
        {
            this.ErrorTrace.Add(new ErrorTraceStep(expr, file, line));
        }

        /// <summary>
        /// Inserts a new call to the trace.
        /// </summary>
        /// <param name="method">Method</param>
        /// <param name="call">Call</param>
        internal void InsertCall(BaseMethodDeclarationSyntax method, ExpressionSyntax call)
        {
            if (call is InvocationExpressionSyntax ||
                call is ObjectCreationExpressionSyntax)
            {
                this.CallTrace.Insert(0, new CallTraceStep(method, call));
            }
        }

        /// <summary>
        /// Merges the given trace to the current trace.
        /// </summary>
        /// <param name="log">Log</param>
        internal void Merge(TraceInfo traceInfo)
        {
            this.ErrorTrace.AddRange(traceInfo.ErrorTrace);
            this.CallTrace.AddRange(traceInfo.CallTrace);
            this.Method = traceInfo.Method;
            this.Machine = traceInfo.Machine;
            this.State = traceInfo.State;
            this.Payload = traceInfo.Payload;
        }

        #endregion
    }
}
