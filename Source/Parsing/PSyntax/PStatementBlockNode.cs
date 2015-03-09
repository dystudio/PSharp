﻿//-----------------------------------------------------------------------
// <copyright file="PStatementBlockNode.cs">
//      Copyright (c) 2015 Pantazis Deligiannis (p.deligiannis@imperial.ac.uk)
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
using System.Linq;

namespace Microsoft.PSharp.Parsing.PSyntax
{
    /// <summary>
    /// Statement block node.
    /// </summary>
    public sealed class PStatementBlockNode : PSyntaxNode
    {
        #region fields

        /// <summary>
        /// The machine parent node.
        /// </summary>
        public readonly PMachineDeclarationNode Machine;

        /// <summary>
        /// The state parent node.
        /// </summary>
        public readonly PStateDeclarationNode State;

        /// <summary>
        /// The left curly bracket token.
        /// </summary>
        public Token LeftCurlyBracketToken;

        /// <summary>
        /// List of statement nodes.
        /// </summary>
        public List<PStatementNode> Statements;

        /// <summary>
        /// The right curly bracket token.
        /// </summary>
        public Token RightCurlyBracketToken;

        #endregion

        #region public API

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="machineNode">PMachineDeclarationNode</param>
        /// <param name="stateNode">PStateDeclarationNode</param>
        public PStatementBlockNode(PMachineDeclarationNode machineNode, PStateDeclarationNode stateNode)
        {
            this.Machine = machineNode;
            this.State = stateNode;
            this.Statements = new List<PStatementNode>();
        }

        /// <summary>
        /// Returns the full text.
        /// </summary>
        /// <returns>string</returns>
        public override string GetFullText()
        {
            return base.TextUnit.Text;
        }

        /// <summary>
        /// Returns the rewritten text.
        /// </summary>
        /// <returns>string</returns>
        public override string GetRewrittenText()
        {
            return base.RewrittenTextUnit.Text;
        }

        #endregion

        #region internal API

        /// <summary>
        /// Rewrites the syntax node declaration to the intermediate C#
        /// representation.
        /// </summary>
        /// <param name="position">Position</param>
        internal override void Rewrite(ref int position)
        {
            var start = position;

            foreach (var stmt in this.Statements)
            {
                stmt.Rewrite(ref position);
            }

            var text = "\n" + this.LeftCurlyBracketToken.TextUnit.Text + "\n";

            foreach (var stmt in this.Statements)
            {
                text += stmt.GetRewrittenText();
            }

            text += this.RightCurlyBracketToken.TextUnit.Text + "\n";

            base.RewrittenTextUnit = new TextUnit(text, this.LeftCurlyBracketToken.TextUnit.Line, start);
            position = base.RewrittenTextUnit.End + 1;
        }

        /// <summary>
        /// Generates a new text unit.
        /// </summary>
        internal override void GenerateTextUnit()
        {
            foreach (var stmt in this.Statements)
            {
                stmt.GenerateTextUnit();
            }

            var text = "\n" + this.LeftCurlyBracketToken.TextUnit.Text + "\n";

            foreach (var stmt in this.Statements)
            {
                text += stmt.GetFullText();
            }

            text += this.RightCurlyBracketToken.TextUnit.Text + "\n";

            base.TextUnit = new TextUnit(text, this.LeftCurlyBracketToken.TextUnit.Line,
                this.LeftCurlyBracketToken.TextUnit.Start);
        }

        #endregion
    }
}