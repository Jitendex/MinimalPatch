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

using System.Collections.Frozen;
using Jitendex.MinimalPatch.Internal;

namespace Jitendex.MinimalPatch;

/// <include file='docs.xml' path='docs/class[@name="Patch"]/*'/>
public static class Patch
{
    /// <include file='docs.xml' path='docs/method[@name="Apply" and @overload="0"]/*'/>
    public static string Apply(ReadOnlySpan<char> patch, ReadOnlySpan<char> original)
    {
        var unifiedDiff = Parse(patch);
        var input = new InputState
        {
            Patch = patch,
            Original = original,
            LineNumberToDiffs = unifiedDiff.GetLineNumberToDiffs(),
        };
        return string.Create
        (
            length: original.Length + unifiedDiff.TotalCharacterCountDelta,
            state: input,
            action: static (destination, input) => Apply(input, destination)
        );
    }

    /// <include file='docs.xml' path='docs/method[@name="Apply" and @overload="1"]/*'/>
    public static int Apply(ReadOnlySpan<char> patch, ReadOnlySpan<char> original, Span<char> destination)
    {
        var unifiedDiff = Parse(patch);
        var input = new InputState
        {
            Patch = patch,
            Original = original,
            LineNumberToDiffs = unifiedDiff.GetLineNumberToDiffs(),
        };
        return Apply(input, destination);
    }

    private static int Apply(InputState input, Span<char> destination)
    {
        Range currentRange = default;
        int lineNumber = 0;
        int charsWritten = 0;

        foreach (var range in input.Original.Split('\n'))
        {
            lineNumber++;
            if (input.LineNumberToDiffs.TryGetValue(lineNumber, out var diffs))
            {
                if (!currentRange.Equals(default))
                {
                    charsWritten = destination.AppendLine(input.Original[currentRange], start: charsWritten);
                    currentRange = default;
                }
                foreach (var diff in diffs)
                {
                    var operationText = input.Patch[diff.PatchRange];
                    switch (diff.Operation)
                    {
                        case Operation.Equal:
                            Validate(expected: operationText, actual: input.Original[range], lineNumber);
                            goto case Operation.Insert;
                        case Operation.Delete:
                            Validate(expected: operationText, actual: input.Original[range], lineNumber);
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
            charsWritten = destination.AppendLine(input.Original[currentRange], start: charsWritten);
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

    private static UnifiedDiff Parse(ReadOnlySpan<char> patch)
    {
        try
        {
            return new UnifiedDiff(patch);
        }
        catch (Exception ex)
        {
            throw new InvalidPatchException("Error occurred while parsing patch text", ex);
        }
    }

    private static void Validate(ReadOnlySpan<char> expected, ReadOnlySpan<char> actual, int lineNumber)
    {
        if (!expected.Equals(actual, StringComparison.Ordinal))
        {
            throw new InvalidPatchException($"Line #{lineNumber} of original text does not match the corresponding line in the patch");
        }
    }

    private readonly ref struct InputState
    {
        public readonly ReadOnlySpan<char> Patch { get; init; }
        public readonly ReadOnlySpan<char> Original { get; init; }
        public readonly FrozenDictionary<int, List<DiffLine>> LineNumberToDiffs { get; init; }
    }
}
