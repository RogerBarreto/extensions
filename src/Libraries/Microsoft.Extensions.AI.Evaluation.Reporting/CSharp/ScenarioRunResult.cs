﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#pragma warning disable S3604
// S3604: Member initializer values should not be redundant.
// We disable this warning because it is a false positive arising from the analyzer's lack of support for C#'s primary
// constructor syntax.

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading;

namespace Microsoft.Extensions.AI.Evaluation.Reporting;

/// <summary>
/// Represents the results of a single execution of a particular iteration of a particular scenario under evaluation.
/// In other words, <see cref="ScenarioRunResult"/> represents the results of evaluating a <see cref="ScenarioRun"/>
/// and includes the <see cref="Evaluation.EvaluationResult"/> that is produced when
/// <see cref="ScenarioRun.EvaluateAsync(IEnumerable{ChatMessage}, ChatResponse, IEnumerable{Microsoft.Extensions.AI.Evaluation.EvaluationContext}?, CancellationToken)"/>
/// is invoked.
/// </summary>
/// <remarks>
/// Each execution of an evaluation run is assigned a unique <see cref="ExecutionName"/>. A single such evaluation run
/// can contain evaluations for multiple scenarios each with a unique <see cref="ScenarioName"/>. The execution of each
/// such scenario in turn can include multiple iterations each with a unique <see cref="IterationName"/>.
/// </remarks>
/// <param name="scenarioName">The <see cref="ScenarioRun.ScenarioName"/>.</param>
/// <param name="iterationName">The <see cref="ScenarioRun.IterationName"/>.</param>
/// <param name="executionName">The <see cref="ScenarioRun.ExecutionName"/>.</param>
/// <param name="creationTime">The time at which this <see cref="ScenarioRunResult"/> was created.</param>
/// <param name="messages">
/// The conversation history including the request that produced the <paramref name="modelResponse"/> being evaluated.
/// </param>
/// <param name="modelResponse">The response being evaluated.</param>
/// <param name="evaluationResult">
/// The <see cref="Evaluation.EvaluationResult"/> for the <see cref="ScenarioRun"/> corresponding to the
/// <see cref="ScenarioRunResult"/> being constructed.
/// </param>
/// <param name="chatDetails">
/// An optional <see cref="Reporting.ChatDetails"/> object that contains details related to all LLM chat conversation
/// turns involved in the execution of the the <see cref="ScenarioRun"/> corresponding to the
/// <see cref="ScenarioRunResult"/> being constructed. Can be <see langword="null"/> if none of the
/// <see cref="IEvaluator"/>s invoked during the execution of the <see cref="ScenarioRun"/> use an LLM.
/// </param>
/// <param name="tags">An optional set of text tags applicable to this <see cref="ScenarioRunResult"/>.</param>
/// <param name="formatVersion">
/// The version of the format used to persist the current <see cref="ScenarioRunResult"/>.
/// </param>
[method: JsonConstructor]
public sealed class ScenarioRunResult(
    string scenarioName,
    string iterationName,
    string executionName,
    DateTime creationTime,
    IList<ChatMessage> messages,
    ChatResponse modelResponse,
    EvaluationResult evaluationResult,
    ChatDetails? chatDetails = null,
    IList<string>? tags = null,
    int? formatVersion = null)
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ScenarioRunResult"/> class.
    /// </summary>
    /// <param name="scenarioName">The <see cref="ScenarioRun.ScenarioName"/>.</param>
    /// <param name="iterationName">The <see cref="ScenarioRun.IterationName"/>.</param>
    /// <param name="executionName">The <see cref="ScenarioRun.ExecutionName"/>.</param>
    /// <param name="creationTime">The time at which this <see cref="ScenarioRunResult"/> was created.</param>
    /// <param name="messages">
    /// The conversation history including the request that produced the <paramref name="modelResponse"/> being
    /// evaluated.
    /// </param>
    /// <param name="modelResponse">The response being evaluated.</param>
    /// <param name="evaluationResult">
    /// The <see cref="Evaluation.EvaluationResult"/> for the <see cref="ScenarioRun"/> corresponding to the
    /// <see cref="ScenarioRunResult"/> being constructed.
    /// </param>
    /// <param name="chatDetails">
    /// An optional <see cref="Reporting.ChatDetails"/> object that contains details related to all LLM chat
    /// conversation turns involved in the execution of the the <see cref="ScenarioRun"/> corresponding to the
    /// <see cref="ScenarioRunResult"/> being constructed. Can be <see langword="null"/> if none of the
    /// <see cref="IEvaluator"/>s invoked during the execution of the <see cref="ScenarioRun"/> use an LLM.
    /// </param>
    /// <param name="tags">An optional set of text tags applicable to this <see cref="ScenarioRunResult"/>.</param>
    public ScenarioRunResult(
        string scenarioName,
        string iterationName,
        string executionName,
        DateTime creationTime,
        IEnumerable<ChatMessage> messages,
        ChatResponse modelResponse,
        EvaluationResult evaluationResult,
        ChatDetails? chatDetails = null,
        IEnumerable<string>? tags = null)
            : this(
                scenarioName,
                iterationName,
                executionName,
                creationTime,
                [.. messages],
                modelResponse,
                evaluationResult,
                chatDetails,
                tags is null ? null : [.. tags])
    {
    }

    /// <summary>
    /// Gets or sets the <see cref="ScenarioRun.ScenarioName"/>.
    /// </summary>
    public string ScenarioName { get; set; } = scenarioName;

    /// <summary>
    /// Gets or sets the <see cref="ScenarioRun.IterationName"/>.
    /// </summary>
    public string IterationName { get; set; } = iterationName;

    /// <summary>
    /// Gets or sets the <see cref="ScenarioRun.ExecutionName"/>.
    /// </summary>
    public string ExecutionName { get; set; } = executionName;

    /// <summary>
    /// Gets or sets the time at which this <see cref="ScenarioRunResult"/> was created.
    /// </summary>
    public DateTime CreationTime { get; set; } = creationTime;

    /// <summary>
    /// Gets or sets the conversation history including the request that produced the <see cref="ModelResponse"/> being
    /// evaluated in this <see cref="ScenarioRunResult"/>.
    /// </summary>
#pragma warning disable CA2227
    // CA2227: Collection properties should be read only.
    // We disable this warning because we want this type to be fully mutable for serialization purposes and for general
    // convenience.
    public IList<ChatMessage> Messages { get; set; } = messages;
#pragma warning restore CA2227

    /// <summary>
    /// Gets or sets the response being evaluated in this <see cref="ScenarioRunResult"/>.
    /// </summary>
    public ChatResponse ModelResponse { get; set; } = modelResponse;

    /// <summary>
    /// Gets or sets the <see cref="Evaluation.EvaluationResult"/> for the <see cref="ScenarioRun"/> corresponding to
    /// this <see cref="ScenarioRunResult"/>.
    /// </summary>
    /// <remarks>
    /// This is the same <see cref="Evaluation.EvaluationResult"/> that is returned when
    /// <see cref="ScenarioRun.EvaluateAsync(IEnumerable{ChatMessage}, ChatResponse, IEnumerable{Microsoft.Extensions.AI.Evaluation.EvaluationContext}?, CancellationToken)"/>
    /// is invoked.
    /// </remarks>
    public EvaluationResult EvaluationResult { get; set; } = evaluationResult;

    /// <summary>
    /// Gets or sets an optional <see cref="Reporting.ChatDetails"/> object that contains details related to all LLM
    /// chat conversation turns involved in the execution of the <see cref="ScenarioRun"/> corresponding to this
    /// <see cref="ScenarioRunResult"/>.
    /// </summary>
    /// <remarks>
    /// Can be <see langword="null"/> if none of the <see cref="IEvaluator"/>s invoked during the execution of the
    /// <see cref="ScenarioRun"/> use an LLM.
    /// </remarks>
    public ChatDetails? ChatDetails { get; set; } = chatDetails;

    /// <summary>
    /// Gets or sets a set of text tags applicable to this <see cref="ScenarioRunResult"/>.
    /// </summary>
#pragma warning disable CA2227
    // CA2227: Collection properties should be read only.
    // We disable this warning because we want this type to be fully mutable for serialization purposes and for general
    // convenience.
    public IList<string>? Tags { get; set; } = tags;
#pragma warning restore CA2227

    /// <summary>
    /// Gets or sets the version of the format used to persist the current <see cref="ScenarioRunResult"/>.
    /// </summary>
    public int? FormatVersion { get; set; } = formatVersion ?? Defaults.ReportingFormatVersion;
}
