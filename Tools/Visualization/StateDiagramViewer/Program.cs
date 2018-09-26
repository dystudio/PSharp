﻿//-----------------------------------------------------------------------
// <copyright file="Program.cs">
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
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Microsoft.Build.Framework;
using Microsoft.PSharp.IO;
using Microsoft.PSharp.LanguageServices;
using Microsoft.PSharp.LanguageServices.Compilation;
using Microsoft.PSharp.LanguageServices.Parsing;

namespace Microsoft.PSharp.StateDiagramViewer
{
    /// <summary>
    /// A tool to produce Dgml files reflecting the State Diagram of machines in the PSharp program
    /// </summary>
    public class StateDiagramViewerProgram
    {
        public static void ResetResolutionHelper()
        {
            ResolutionHelper.ResetToNewInstance();
        }


        /// <summary>
        /// Convenience method for Tests & other services
        /// <param name="prog">The PSharp program for which the diagram should be generated</param>
        /// <param name="config">ConfigOptions object. Defaults are used if null</param>
        /// </summary>
        public static string GetDgmlForProgram(string prog, ConfigOptions config=null)
        {
            if (config == null)
            {
                config = new ConfigOptions(); // Defaults
            }
            Version csVersion = new Version(0, 0);
            Configuration configuration = Configuration.Create();
            configuration.Verbose = 2;
            configuration.RewriteCSharpVersion = csVersion;
            var context = CompilationContext.Create(configuration);

            context.LoadSolution(prog);
            string dgml = StateDiagramViewerProgram.CreateDgml(context, out string errors, csVersion, config);
            if(dgml == null)
            {
                throw new StateDiagramViewerException(
                    "GetDGML for program failed with errors:"+Environment.NewLine + errors, null, null);
            }
            return dgml;
        }

        static void Main(string[] args)
        {
            string infile = string.Empty;
            string outfile = string.Empty;
            var projectFile = String.Empty;
            var solutionFile = String.Empty;
            var csVersion = new Version(0, 0);

            var usage = "Usage: " + Environment.NewLine +
                "   PSharpStateMachineStructureViewer.exe file.psharp [file.dgml] [/csVersion:major.minor] [options]" + Environment.NewLine +
                "OR" + Environment.NewLine +
                "   PSharpStateMachineStructureViewer.exe /s:SolutionFile.sln /p:ProjectName [outfile.dgml] [/csVersion:major.minor] [options]" + Environment.NewLine +
                Environment.NewLine +
                "Options include:" + Environment.NewLine +
                ConfigOptions.GetDescription();

            List<string> positionalArgs = new List<string>();
            ConfigOptions config = new ConfigOptions();
            if (args.Length >= 1 /*&& args.Length <= 3*/)
            {
                foreach (var arg in args)
                {
                    if (arg.StartsWith("/") || arg.StartsWith("-"))
                    {
                        var parts = arg.Substring(1).Split(new[] { ':' }, 2, StringSplitOptions.RemoveEmptyEntries);
                        switch (parts[0].ToLower())
                        {
                            case "?":
                                Output.WriteLine(usage);
                                return;
                            case "csversion":
                                bool parseVersion(string value, out Version version)
                                {
                                    if (int.TryParse(value, out int intVer)) {
                                        version = new Version(intVer, 0);
                                        return true;
                                    }
                                    return Version.TryParse(value, out version);
                                }
                                if (parts.Length != 2 || !parseVersion(parts[1], out csVersion))
                                {
                                    Output.WriteLine("Error: option csVersion requires a version major[.minor] value");
                                    return;
                                }
                                break;

                            case "p":
                                if(parts.Length != 2)
                                {
                                    Output.WriteLine("Error: option 'p' requires a value");
                                    Output.WriteLine(usage);
                                    return;
                                }
                                projectFile = parts[1];
                                break;
                            case "s":
                                if (parts.Length != 2)
                                {
                                    Output.WriteLine("Error: option 's' requires a value");
                                    Output.WriteLine(usage);
                                    return;
                                }
                                solutionFile = parts[1];
                                break;

                            default:
                                if (!config.TryParseOption(parts)) { 
                                    Output.WriteLine($"Error: unknown option {parts[0]}");
                                    return;
                                }
                                break;
                        }
                    }
                    else
                    {
                        positionalArgs.Add(arg);
                    }
                }
            }


            if( (projectFile.Length == 0 || solutionFile.Length==0) && positionalArgs.Count < 1)
            {
                Output.WriteLine(usage);
                return;
            }


            var configuration = Configuration.Create();
            configuration.Verbose = 2;
            configuration.RewriteCSharpVersion = csVersion;

            CompilationContext context = null;
            if (projectFile.Length > 0 && solutionFile.Length > 0 )
            {
                if (positionalArgs.Count > 1)
                {
                    Output.WriteLine("Too many positional arguments. Expected upto 1 ");
                    Output.WriteLine(usage);
                    return;
                }

                if (positionalArgs.Count == 1) { 
                    outfile = positionalArgs[0];
                }
                configuration.ProjectName = projectFile;
                configuration.SolutionFilePath = solutionFile;
                context = CompilationContext.Create(configuration).LoadSolution();
            }
            else if (positionalArgs.Count >= 1)
            {

                if (positionalArgs.Count > 2)
                {
                    Output.WriteLine("Too many positional arguments. Expected upto 2");
                    Output.WriteLine(usage);
                    return;
                }

                infile = positionalArgs[0];
                outfile = positionalArgs.Count >= 2 ? positionalArgs[1] : outfile;
                // Gets input file as string.
                var input_string = "";
                try
                {
                    input_string = File.ReadAllText(infile);
                }
                catch (IOException e)
                {
                    Output.WriteLine("Error: {0}", e.Message);
                    return;
                }

                context = CompilationContext.Create(configuration).LoadSolution(input_string);
            }

            // Translates and prints on console or to file.
            string errors = string.Empty;

            var output = CreateDgml(context, out errors, csVersion, config);
            if (output == null)
            {
                Output.WriteLine("Parse Error: " + errors);
            }
            else
            {
                if (!string.IsNullOrEmpty(outfile))
                {
                    try
                    {
                        File.WriteAllLines(outfile, new[] { output });
                    }
                    catch (Exception ex)
                    {
                        Output.WriteLine("Error writing to file: {0}", ex.Message);
                    }
                }
                else { 
                    Output.WriteLine("{0}", output);
                }
            }
        }

        /// <summary>
        /// Produces the Dgml file for the given program
        /// </summary>
        /// <param name="text">Text</param>
        /// <returns>Text</returns>
        public static string CreateDgml(CompilationContext context, out string errors, Version csVersion, ConfigOptions config)
        {
            try
            {
                errors = null;
                ParsingEngine.Create(context).Run();
                ResolutionHelper resolutionHelper = ResolutionHelper.Instance();
                resolutionHelper.PopulateMachines(context.GetProjects()[0].PSharpPrograms);

                // Populate events in namespaces
                resolutionHelper.PopulateGlobalEvents(context.GetProjects()[0].PSharpPrograms);
                // Populate events in machines
                foreach (MachineInfo machineInfo in resolutionHelper.GetAllMachines())
                {
                    machineInfo.ResolveBaseMachine();
                    resolutionHelper.PopulateStates(machineInfo);
                    resolutionHelper.PopulateEvents(machineInfo);
                }

                foreach (MachineInfo machineInfo in resolutionHelper.GetAllMachines())
                {
                    foreach (string stateName in machineInfo.GetStates(false))
                    {
                        ResolutionHelper.Instance().GetState(stateName).ResolveBaseState();
                    }
                }
                MemoryStream memStream = new MemoryStream();
                using (var writer = new XmlTextWriter(memStream, Encoding.UTF8))
                {
                    EmitStateMachineStructure(ResolutionHelper.Instance().GetAllMachines(), writer, config);
                    //context.GetProjects()[0].PSharpPrograms[0].EmitStateMachineStructure(writer);
                }
                return Encoding.UTF8.GetString(memStream.ToArray());
            }
            catch (ParsingException ex)
            {
                errors = ex.Message;
                return null;
            }
            catch(StateDiagramViewerBaseException ex)
            {
                errors = ex.Message;
                return null;
            }
        }

        /// <summary>
        /// Emits dgml representation of the state machine structure
        /// </summary>
        /// <param name="machines">List of machines to include in the diagram</param>
        /// <param name="writer">XmlTestWriter</param>
        /// <param name="config">ConfigOptions object</param>
        internal static void EmitStateMachineStructure(List<MachineInfo> machines, XmlTextWriter writer, ConfigOptions config)
        {
            
            // Starts document.
            writer.WriteStartDocument(true);
            writer.Formatting = Formatting.Indented;
            writer.Indentation = 2;
            DgmlWriter dgmlWriter = new DgmlWriter(writer, config);
            dgmlWriter.WriteAll(machines);

            // Ends document.
            writer.WriteEndDocument();

        }


    }

}
