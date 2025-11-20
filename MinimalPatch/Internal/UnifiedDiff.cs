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
                if (!line.StartsWith(HeaderPrefix(diffLineNum), StringComparison.Ordinal))
                {
                    throw new ArgumentException
                    (
                        "UnifiedDiff text does not begin with the standard header",
                        nameof(text)
                    );
                }
            }
            else if (line.StartsWith('@'))
            {
                if (hunk is not null)
                {
                    AddHunk(hunk);
                }
                hunk = new Hunk(line);
                origLineNum = hunk.Header.StartA - 1;
            }
            else if (line.Length > 0 && GetLineOperation(line[0]) is Operation operation)
            {
                if (hunk is null)
                {
                    throw new ArgumentException
                    (
                        "Line operation found before any hunks",
                        nameof(text)
                    );
                }
                if (operation.IsFileA())
                {
                    origLineNum++;
                }
                int idx = int.Max(origLineNum, hunk.Header.StartA);
                hunk.LineOperations[idx].Add(new LineOperation
                {
                    Range = new Range(range.Start.Value + 1, range.End),
                    Operation = operation,
                });
            }
            else if (range.Start.Equals(text.Length) && range.End.Equals(text.Length))
            {
                // Blank line at the end of the file.
            }
            else
            {
                throw new ArgumentException
                (
                    $"Line #{diffLineNum} in unidiff text does not begin with a standard prefix",
                    nameof(text)
                );
            }
        }

        if (hunk is not null)
        {
            AddHunk(hunk);
        }
    }

    public FrozenDictionary<int, List<LineOperation>> GetLineOperations()
        => _hunks.SelectMany(static h => h.LineOperations).ToFrozenDictionary();

    private void AddHunk(Hunk hunk)
    {
        if (hunk.LengthsAreConsistent())
        {
            _hunks.Add(hunk);
        }
        else
        {
            throw new ArgumentException
            (
                "Hunk header does not match count of line operations",
                nameof(hunk)
            );
        }
    }

    private static ReadOnlySpan<char> HeaderPrefix(int lineNumber) => lineNumber switch
    {
        1 => ['-', '-', '-'],
        2 => ['+', '+', '+'],
        _ => throw new ArgumentOutOfRangeException(nameof(lineNumber))
    };

    private static Operation? GetLineOperation(char prefix) => prefix switch
    {
        ' ' => Operation.Equal,
        '-' => Operation.Delete,
        '+' => Operation.Insert,
        _ => null
    };
}
