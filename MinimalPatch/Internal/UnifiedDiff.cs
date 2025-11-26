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

namespace MinimalPatch.Internal;

internal sealed class UnifiedDiff
{
    private readonly List<Hunk> _hunks = [];
    private readonly int _sumLengthA = 0;

    private ref struct CurrentHunkLength
    {
        public int A;
        public int B;
    }

    public UnifiedDiff(ReadOnlySpan<char> text)
    {
        Hunk? hunk = null;
        CurrentHunkLength currentLength = new();
        int currentDiffLineNum = 0;

        foreach (var range in text.Split('\n'))
        {
            currentDiffLineNum++;
            var line = text[range];

            if (currentDiffLineNum < 3)
            {
                ValidateHeaderLine(line, currentDiffLineNum);
            }
            else if (line.StartsWith('@'))
            {
                AddHunk(hunk, ref currentLength);
                hunk = new Hunk(line);
                _sumLengthA += hunk.Header.LengthA;
            }
            else if (line.Length > 0 && GetLineOperation(line[0]) is Operation operation)
            {
                AddLineOperation(hunk, operation, range, ref currentLength);
            }
            else if (range.Start.Equals(text.Length) && range.End.Equals(text.Length))
            {
                // Blank line at the end of the file.
            }
            else
            {
                throw new InvalidDiffException($"Line #{currentDiffLineNum} in unidiff text does not begin with a standard prefix");
            }
        }

        AddHunk(hunk, ref currentLength);
    }

    private static void ValidateHeaderLine(ReadOnlySpan<char> line, int diffLineNum)
    {
        if (!line.StartsWith(HeaderPrefix(diffLineNum), StringComparison.Ordinal))
        {
            throw new InvalidDiffException("UnifiedDiff text does not begin with the standard header");
        }
    }

    private static ReadOnlySpan<char> HeaderPrefix(int lineNumber) => lineNumber switch
    {
        1 => ['-', '-', '-'],
        2 => ['+', '+', '+'],
        _ => throw new ArgumentOutOfRangeException(nameof(lineNumber))
    };

    private void AddHunk(Hunk? hunk, ref CurrentHunkLength currentLength)
    {
        if (hunk is null)
        {
            return;
        }
        if (currentLength.A == hunk.Header.LengthA && currentLength.B == hunk.Header.LengthB)
        {
            _hunks.Add(hunk);
            currentLength.A = 0;
            currentLength.B = 0;
        }
        else
        {
            throw new InvalidDiffException("Hunk header does not match count of line operations");
        }
    }

    private static Operation? GetLineOperation(char prefix) => prefix switch
    {
        ' ' => Operation.Equal,
        '-' => Operation.Delete,
        '+' => Operation.Insert,
        '\\' => throw new NotSupportedException("'No newline at end of file' operations have not been implemented"),
        _ => null
    };

    private static void AddLineOperation(Hunk? hunk, Operation operation, Range range, ref CurrentHunkLength currentLength)
    {
        if (hunk is null)
        {
            throw new InvalidDiffException("Line operation found before any hunks");
        }
        if (operation.IsFileA())
        {
            currentLength.A++;
        }
        if (operation.IsFileB())
        {
            currentLength.B++;
        }

        int idx = int.Max(currentLength.A - 1, 0);

        hunk.LineOperations[idx].Add(new LineOperation
        {
            Operation = operation,
            Range = new Range(range.Start.Value + 1, range.End),
        });
    }

    public FrozenDictionary<int, List<LineOperation>> GetLineOperations()
    {
        var pairs = new KeyValuePair<int, List<LineOperation>>[_sumLengthA];
        int pairIdx = 0;
        foreach (var hunk in _hunks)
        {
            for (int i = 0; i < hunk.Header.LengthA; i++)
            {
                int lineNumber = hunk.Header.StartA + i;
                pairs[pairIdx] = new(lineNumber, hunk.LineOperations[i]);
                pairIdx++;
            }
        }
        return pairs.ToFrozenDictionary();
    }
}
