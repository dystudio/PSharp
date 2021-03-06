﻿// ------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Xunit;
using Xunit.Abstractions;

namespace Microsoft.PSharp.Core.Tests.LogMessages
{
    public class CustomLogWriterTest : BaseTest
    {
        public CustomLogWriterTest(ITestOutputHelper output)
            : base(output)
        {
        }

        [Fact(Timeout=5000)]
        public async Task TestCustomLogWriter()
        {
            CustomLogger logger = new CustomLogger(true);

            Configuration config = Configuration.Create().WithVerbosityEnabled();
            var runtime = PSharpRuntime.Create(config);
            runtime.SetLogger(logger);
            runtime.SetLogWriter(new CustomLogWriter());

            var tcs = new TaskCompletionSource<bool>();
            runtime.CreateMachine(typeof(M), new Configure(tcs));

            await WaitAsync(tcs.Task);
            await Task.Delay(200);

            string expected = @"<CreateLog>.
<StateLog>.
<ActionLog> Machine 'Microsoft.PSharp.Core.Tests.LogMessages.M()' in state 'Init' invoked action 'InitOnEntry'.
<CreateLog>.
<StateLog>.
<DequeueLog> Machine 'Microsoft.PSharp.Core.Tests.LogMessages.N()' in state 'Init' dequeued event 'Microsoft.PSharp.Core.Tests.LogMessages.E'.
<ActionLog> Machine 'Microsoft.PSharp.Core.Tests.LogMessages.N()' in state 'Init' invoked action 'Act'.
<DequeueLog> Machine 'Microsoft.PSharp.Core.Tests.LogMessages.M()' in state 'Init' dequeued event 'Microsoft.PSharp.Core.Tests.LogMessages.E'.
<ActionLog> Machine 'Microsoft.PSharp.Core.Tests.LogMessages.M()' in state 'Init' invoked action 'Act'.
";

            string actual = Regex.Replace(logger.ToString(), "[0-9]", string.Empty);

            HashSet<string> expectedSet = new HashSet<string>(Regex.Split(expected, "\r\n|\r|\n"));
            HashSet<string> actualSet = new HashSet<string>(Regex.Split(actual, "\r\n|\r|\n"));

            Assert.True(expectedSet.SetEquals(actualSet));

            logger.Dispose();
        }
    }
}
