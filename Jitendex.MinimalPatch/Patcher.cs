/*
Copyright (c) 2025 Stephen Kraus

This file is part of MinimalPatch.

MinimalPatch is free software: you can redistribute it and/or modify it under the
terms of the GNU General Public License as published by the Free Software Foundation,
either version 3 of the License, or (at your option) any later version.

MinimalPatch is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with MinimalPatch.
If not, see <https://www.gnu.org/licenses/>.
*/

using Jitendex.MinimalPatch.Internal;

namespace Jitendex.MinimalPatch;

/// <include file='docs.xml' path='docs/class[@name="Patcher"]/*'/>
public static class Patcher
{
    /// <include file='docs.xml' path='docs/method[@name="ApplyPatch" and @overload="0"]/*'/>
    public static string ApplyPatch(ReadOnlySpan<char> patch, ReadOnlySpan<char> original)
    {
        var state = new InputState(patch, original);
        return string.Create
        (
            length: state.ExpectedOutputLength,
            state: state,
            action: static (destination, state) => Apply(state, destination)
        );
    }

    /// <include file='docs.xml' path='docs/method[@name="ApplyPatch" and @overload="1"]/*'/>
    public static int ApplyPatch(ReadOnlySpan<char> patch, ReadOnlySpan<char> original, Span<char> destination)
    {
        var state = new InputState(patch, original);
        return Apply(state, destination);
    }

    private static int Apply(in InputState state, Span<char> destination)
    {
        Range currentRange = default;
        int lineNumber = 0;
        int charsWritten = 0;

        foreach (var range in state.Original.Split('\n'))
        {
            lineNumber++;
            if (state.LineNumberToDiffs.TryGetValue(lineNumber, out var diffs))
            {
                if (!currentRange.Equals(default))
                {
                    charsWritten = destination.AppendLine(state.Original[currentRange], start: charsWritten);
                    currentRange = default;
                }
                foreach (var diff in diffs)
                {
                    var operationText = state.Patch[diff.PatchRange];
                    switch (diff.Operation)
                    {
                        case Operation.Equal:
                            Validate(expected: operationText, actual: state.Original[range], lineNumber);
                            goto case Operation.Insert;
                        case Operation.Delete:
                            Validate(expected: operationText, actual: state.Original[range], lineNumber);
                            break;
                        case Operation.Insert:
                            charsWritten = destination.AppendLine(operationText, start: charsWritten);
                            break;
                    }
                }
            }
            else
            {
                currentRange = currentRange.Equals(default)
                    ? range
                    : new Range(currentRange.Start, range.End);
            }
        }

        if (!currentRange.Equals(default))
        {
            charsWritten = destination.AppendLine(state.Original[currentRange], start: charsWritten);
        }

        return charsWritten;
    }

    private static int AppendLine(this Span<char> buffer, ReadOnlySpan<char> line, int start)
    {
        if (start > 0)
        {
            buffer[start] = '\n';
            start++;
        }
        line.CopyTo(buffer[start..]);
        return start + line.Length;
    }

    private static void Validate(ReadOnlySpan<char> expected, ReadOnlySpan<char> actual, int lineNumber)
    {
        if (!expected.Equals(actual, StringComparison.Ordinal))
        {
            throw new InvalidPatchException($"Line #{lineNumber} of original text does not match the corresponding line in the patch");
        }
    }
}
