/*
Copyright (c) 2025 Stephen Kraus

This file is part of MinimalPatch.

MinimalPatch is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

MinimalPatch is distributed in the hope that it will be useful, but
WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
General Public License for more details.

You should have received a copy of the GNU General Public License
along with MinimalPatch. If not, see <https://www.gnu.org/licenses/>.
*/

using System.Collections.Frozen;
using MinimalPatch.Internal;

namespace MinimalPatch;

public static class Patch
{
    /// <summary>
    /// Apply a patch to an input text and return the result.
    /// </summary>
    /// <param name="patchText">Textual representation of the patch (unified diff format)</param>
    /// <param name="originalText">Text onto which the patch is applied.</param>
    /// <exception cref="InvalidDiffException">Thrown if the diff text cannot be parsed or if it is inconsistent with the input text.</exception>
    /// <remarks>The patch metadata must match the input text perfectly. There is no fuzzy matching.</remarks>
    public static ReadOnlySpan<char> Apply(ReadOnlySpan<char> patchText, ReadOnlySpan<char> originalText)
    {
        var destination = new char[patchText.Length + originalText.Length];
        Apply(patchText, originalText, destination, out int charsWritten);
        return destination.AsSpan(0, charsWritten);
    }

    /// <summary>
    /// Fill a preallocated character span with the result of a patch applied to an input text.
    /// </summary>
    /// <param name="patchText">Textual representation of the patch (unified diff format)</param>
    /// <param name="originalText">Text onto which the patch is applied.</param>
    /// <param name="destination">Buffer for containing the patched text.</param>
    /// <param name="charsWritten">The number of characters written to the destination buffer.</param>
    /// <returns>The length of the patched text.</returns>
    /// <exception cref="InvalidDiffException">Thrown if the diff text cannot be parsed or if it is inconsistent with the input text.</exception>
    /// <remarks>The patch metadata must match the input text perfectly. There is no fuzzy matching.</remarks>
    public static void Apply(ReadOnlySpan<char> patchText, ReadOnlySpan<char> originalText, Span<char> destination, out int charsWritten)
    {
        Range currentRange = default;
        int lineNumber = 0;
        charsWritten = 0;

        var lineOperations = GetLineOperations(patchText);

        foreach (var range in originalText.Split('\n'))
        {
            lineNumber++;
            if (lineOperations.TryGetValue(lineNumber, out var operations))
            {
                if (!currentRange.Equals(default))
                {
                    charsWritten = destination.AppendLine(originalText[currentRange], start: charsWritten);
                    currentRange = default;
                }
                foreach (var operation in operations)
                {
                    var operationText = patchText[operation.Range];
                    if (operation.IsOriginalLine())
                    {
                        Validate(expected: operationText, actual: originalText[range], lineNumber);
                    }
                    if (operation.IsOutputLine())
                    {
                        charsWritten = destination.AppendLine(operationText, start: charsWritten);
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
            charsWritten = destination.AppendLine(originalText[currentRange], start: charsWritten);
        }
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

    private static FrozenDictionary<int, List<LineOperation>> GetLineOperations(ReadOnlySpan<char> patchText)
    {
        try
        {
            UnifiedDiff diff = new(patchText);
            return diff.GetLineOperations();
        }
        catch (Exception ex)
        {
            throw new InvalidDiffException("Error occurred while parsing patch text", ex);
        }
    }

    private static void Validate(ReadOnlySpan<char> expected, ReadOnlySpan<char> actual, int lineNumber)
    {
        if (!expected.Equals(actual, StringComparison.Ordinal))
        {
            throw new InvalidDiffException($"Line #{lineNumber} of original text does not match the corresponding line in the patch");
        }
    }
}
