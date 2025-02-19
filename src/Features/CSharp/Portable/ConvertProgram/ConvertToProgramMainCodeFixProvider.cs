﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Formatting;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Shared.Extensions;

namespace Microsoft.CodeAnalysis.CSharp.ConvertProgram;

using static ConvertProgramTransform;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = PredefinedCodeFixProviderNames.ConvertToProgramMain), Shared]
[ExtensionOrder(After = PredefinedCodeFixProviderNames.RemoveUnnecessaryImports)]
[method: ImportingConstructor]
[method: Obsolete(MefConstruction.ImportingConstructorMessage, error: true)]
internal sealed class ConvertToProgramMainCodeFixProvider() : SyntaxEditorBasedCodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds
        => [IDEDiagnosticIds.UseProgramMainId];

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var document = context.Document;
        var cancellationToken = context.CancellationToken;

        var options = await document.GetCSharpSyntaxFormattingOptionsAsync(cancellationToken).ConfigureAwait(false);
        var priority = options.PreferTopLevelStatements.Notification.Severity == ReportDiagnostic.Hidden
            ? CodeActionPriority.Low
            : CodeActionPriority.Default;

        RegisterCodeFix(context, CSharpAnalyzersResources.Convert_to_Program_Main_style_program, nameof(ConvertToProgramMainCodeFixProvider), priority);
    }

    protected override async Task FixAllAsync(
        Document document, ImmutableArray<Diagnostic> diagnostics, SyntaxEditor editor, CancellationToken cancellationToken)
    {
        var options = await document.GetSyntaxFormattingOptionsAsync(cancellationToken).ConfigureAwait(false);
        var fixedDocument = await ConvertToProgramMainAsync(document, options.AccessibilityModifiersRequired, cancellationToken).ConfigureAwait(false);
        var fixedRoot = await fixedDocument.GetRequiredSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        editor.ReplaceNode(editor.OriginalRoot, fixedRoot);
    }
}
