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
    /// <param name="diffText">Textual representation of the patch (unified diff format)</param>
    /// <param name="input">Text onto which the patch is applied.</param>
    /// <returns>The patched text.</returns>
    /// <exception cref="InvalidDiffException">Thrown if the diff text cannot be parsed or if it is inconsistent with the input text.</exception>
    /// <remarks>The patch metadata must match the input text perfectly. There is no fuzzy matching.</remarks>
    public static string Apply(ReadOnlySpan<char> diffText, ReadOnlySpan<char> input)
    {
        StringBuilder sb = new();
        Range currentRange = default;
        var lineNumToOps = GetLineNumberToOperationsDictionary(diffText);
        int lineNumber = 0;

        foreach (var range in input.Split('\n'))
        {
            lineNumber++;
            if (lineNumToOps.TryGetValue(lineNumber, out var ops))
            {
                if (!currentRange.Equals(default))
                {
                    if (sb.Length > 0) sb.AppendLine();
                    sb.Append(input[currentRange]);
                    currentRange = default;
                }
                foreach (var op in ops)
                {
                    if (op.IsALine())
                    {
                        ValidateALineText(input[range], op.Text, lineNumber);
                    }
                    if (op.IsBLine())
                    {
                        if (sb.Length > 0) sb.AppendLine();
                        sb.Append(op.Text);
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
            if (sb.Length > 0) sb.AppendLine();
            sb.Append(input[currentRange]);
        }

        return sb.ToString();
    }

    private static FrozenDictionary<int, List<LineOperation>> GetLineNumberToOperationsDictionary(ReadOnlySpan<char> diffText)
    {
        try
        {
            UnifiedDiff diff = new(diffText);
            return diff.GetLineNumberToOperationsDictionary();
        }
        catch (Exception ex)
        {
            throw new InvalidDiffException("Error occurred while parsing diff text", ex);
        }
    }

    private static void ValidateALineText(ReadOnlySpan<char> sourceText, ReadOnlySpan<char> aLineText, int lineNumber)
    {
        if (!sourceText.Equals(aLineText, StringComparison.Ordinal))
        {
            throw new InvalidDiffException($"Line #{lineNumber} of input text does not match diff");
        }
    }
}
