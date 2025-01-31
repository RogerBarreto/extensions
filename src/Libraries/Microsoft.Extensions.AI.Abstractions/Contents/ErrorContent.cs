// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;

namespace Microsoft.Extensions.AI;

/// <summary>
/// Represents an error content.
/// </summary>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class ErrorContent : AIContent
{
    public required string Message { get; set; }
    public string? Code { get; set; }
    public string? Details { get; set; }

    /// <summary>Gets a string representing this instance to display in the debugger.</summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay
    {
        get
        {
            string display = $"Message = {Message} ";

            display += Code is not null ?
                $", Code = {Code}" : string.Empty;

            display += Details is not null ?
                $", Details = {Details}" : string.Empty;

            return display;
        }
    }
}
