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
        var newText = new char[patchText.Length + originalText.Length];
        var length = Apply(patchText, originalText, newText);
        return newText.AsSpan(0, length);
    }

    /// <summary>
    /// Fill a text buffer with the result of a patch applied to an input text.
    /// </summary>
    /// <param name="patchText">Textual representation of the patch (unified diff format)</param>
    /// <param name="originalText">Text onto which the patch is applied.</param>
    /// <param name="newText">Buffer containing the patched text.</param>
    /// <returns>The length of the patched text.</returns>
    /// <exception cref="InvalidDiffException">Thrown if the diff text cannot be parsed or if it is inconsistent with the input text.</exception>
    /// <remarks>The patch metadata must match the input text perfectly. There is no fuzzy matching.</remarks>
    public static int Apply(ReadOnlySpan<char> patchText, ReadOnlySpan<char> originalText, Span<char> newText)
    {
        Range currentRange = default;
        var lineOperations = GetLineOperations(patchText);
        int lineNumber = 0;
        int length = 0;

        foreach (var range in originalText.Split('\n'))
        {
            lineNumber++;
            if (lineOperations.TryGetValue(lineNumber, out var operations))
            {
                if (!currentRange.Equals(default))
                {
                    length = newText.AppendLine(originalText[currentRange], start: length);
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
                        length = newText.AppendLine(operationText, start: length);
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
            length = newText.AppendLine(originalText[currentRange], start: length);
        }

        return length;
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
