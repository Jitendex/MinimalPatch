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
using System.Text;
using MinimalPatch.Internal;

namespace MinimalPatch;

public static class Patch
{
    /// <summary>
    /// Apply a patch to an input text and return the result.
    /// </summary>
    /// <param name="patchText">Textual representation of the patch (unified diff format)</param>
    /// <param name="originalText">Text onto which the patch is applied.</param>
    /// <returns>The patched text.</returns>
    /// <exception cref="InvalidDiffException">Thrown if the diff text cannot be parsed or if it is inconsistent with the input text.</exception>
    /// <remarks>The patch metadata must match the input text perfectly. There is no fuzzy matching.</remarks>
    public static string Apply(ReadOnlySpan<char> patchText, ReadOnlySpan<char> originalText)
    {
        StringBuilder sb = new();
        Range currentRange = default;
        var lineOperations = GetLineOperations(patchText);
        int lineNumber = 0;

        foreach (var range in originalText.Split('\n'))
        {
            lineNumber++;
            if (lineOperations.TryGetValue(lineNumber, out var operations))
            {
                if (!currentRange.Equals(default))
                {
                    sb.AppendOutputLine(originalText[currentRange]);
                    currentRange = default;
                }
                foreach (var operation in operations)
                {
                    var operationText = patchText[operation.Range];
                    if (operation.IsOriginalLine())
                    {
                        ValidateOriginalLineText(operationText, originalText[range], lineNumber);
                    }
                    if (operation.IsOutputLine())
                    {
                        sb.AppendOutputLine(operationText);
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
            sb.AppendOutputLine(originalText[currentRange]);
        }

        return sb.ToString();
    }

    private static void AppendOutputLine(this StringBuilder sb, ReadOnlySpan<char> line)
    {
        if (sb.Length > 0)
        {
            sb.AppendLine();
        }
        sb.Append(line);
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

    private static void ValidateOriginalLineText(ReadOnlySpan<char> expected, ReadOnlySpan<char> actual, int lineNumber)
    {
        if (!expected.Equals(actual, StringComparison.Ordinal))
        {
            throw new InvalidDiffException($"Line #{lineNumber} of original text does not match the corresponding line in the patch");
        }
    }
}
