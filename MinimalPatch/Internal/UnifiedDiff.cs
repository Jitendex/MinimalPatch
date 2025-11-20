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

    public FrozenDictionary<int, List<LineOperation>> GetLineOperations()
        => _hunks.SelectMany(static h => h.LineOperations).ToFrozenDictionary();

    public UnifiedDiff(ReadOnlySpan<char> text)
    {
        int diffLineNum = 0;
        int origLineNum = 0;
        Hunk? hunk = null;

        foreach (var range in text.Split('\n'))
        {
            diffLineNum++;
            var line = text[range];

            if (diffLineNum < 3)
            {
                ValidateHeaderLine(line, diffLineNum);
            }
            else if (line.StartsWith('@'))
            {
                AddHunk(hunk);
                hunk = new Hunk(line);
                origLineNum = hunk.Header.StartA - 1;
            }
            else if (line.Length > 0 && GetLineOperation(line[0]) is Operation operation)
            {
                AddLineOperation(hunk, operation, range, ref origLineNum);
            }
            else if (range.Start.Equals(text.Length) && range.End.Equals(text.Length))
            {
                // Blank line at the end of the file.
            }
            else
            {
                throw new InvalidDiffException($"Line #{diffLineNum} in unidiff text does not begin with a standard prefix");
            }
        }

        AddHunk(hunk);
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

    private void AddHunk(Hunk? hunk)
    {
        if (hunk is null)
        {
            return;
        }
        if (hunk.LengthsAreConsistent())
        {
            _hunks.Add(hunk);
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
        _ => null
    };

    private static void AddLineOperation(Hunk? hunk, Operation operation, Range range, ref int origLineNum)
    {
        if (hunk is null)
        {
            throw new InvalidDiffException("Line operation found before any hunks");
        }
        if (operation.IsFileA())
        {
            origLineNum++;
        }

        // origLineNum is initialized to StartA - 1.
        // If the first operations are inserts, then origLineNum
        // will be less than StartA and therefore out of range.
        int idx = int.Max(origLineNum, hunk.Header.StartA);

        hunk.LineOperations[idx].Add(new LineOperation
        {
            Operation = operation,
            Range = new Range(range.Start.Value + 1, range.End),
        });
    }
}
