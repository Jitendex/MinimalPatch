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
    private int _totalLineDiffCount = 0;

    private ref struct HunkLength
    {
        public int A;
        public int B;
    }

    public int TotalCharacterCountDelta { get; private set; }

    public UnifiedDiff(ReadOnlySpan<char> patch)
    {
        Hunk? hunk = null;
        HunkLength currentLength = default;
        byte currentDiffLineNum = 1;

        foreach (var range in patch.Split('\n'))
        {
            var lineText = patch[range];

            if (currentDiffLineNum < 3)
            {
                ValidateHeaderText(lineText, currentDiffLineNum);
                currentDiffLineNum++;
            }
            else if (lineText.StartsWith('@'))
            {
                AddHunk(hunk, currentLength);
                hunk = new Hunk(lineText);
                currentLength = default;
            }
            else if (lineText.Length > 0 && GetLineOperation(lineText[0]) is Operation operation)
            {
                AddLineOperation(hunk, operation, range, ref currentLength);
            }
            else if (range.Start.Equals(patch.Length) && range.End.Equals(patch.Length))
            {
                // Blank line at the end of the file.
            }
            else
            {
                throw new InvalidPatchException($"Line does not begin with a standard prefix: `{lineText}`");
            }
        }

        AddHunk(hunk, currentLength);
    }

    private static void ValidateHeaderText(ReadOnlySpan<char> lineText, byte lineNumber)
    {
        if (!lineText.StartsWith(HeaderPrefix(lineNumber), StringComparison.Ordinal))
        {
            throw new InvalidPatchException("UnifiedDiff text does not begin with the standard header");
        }
    }

    private static ReadOnlySpan<char> HeaderPrefix(byte lineNumber) => lineNumber switch
    {
        1 => ['-', '-', '-'],
        2 => ['+', '+', '+'],
        _ => throw new ArgumentOutOfRangeException(nameof(lineNumber))
    };

    private void AddHunk(Hunk? hunk, HunkLength actualLength)
    {
        if (hunk is null)
        {
            return;
        }
        if (hunk.Header.LengthA == actualLength.A && hunk.Header.LengthB == actualLength.B)
        {
            _hunks.Add(hunk);
            _totalLineDiffCount += hunk.LineDiffs.Length;
        }
        else
        {
            throw new InvalidPatchException("Hunk header does not match count of line operations");
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

    private void AddLineOperation(Hunk? hunk, Operation operation, Range range, ref HunkLength currentLength)
    {
        if (hunk is null)
        {
            throw new InvalidPatchException("Line operation found before any hunks");
        }

        switch (operation)
        {
            case Operation.Equal:
                currentLength.A++;
                currentLength.B++;
                break;
            case Operation.Delete:
                currentLength.A++;
                TotalCharacterCountDelta -= range.End.Value - range.Start.Value;
                break;
            case Operation.Insert:
                currentLength.B++;
                TotalCharacterCountDelta += range.End.Value - range.Start.Value;
                break;
        }

        int idx = int.Max(currentLength.A - 1, 0);

        // If `idx` is too large for this array, then the header metadata was incorrect.
        hunk.LineDiffs[idx].Add(new DiffLine
        {
            Operation = operation,
            PatchRange = new Range(range.Start.Value + 1, range.End),
        });
    }

    public FrozenDictionary<int, List<DiffLine>> GetLineNumberToDiffs()
    {
        var pairs = new KeyValuePair<int, List<DiffLine>>[_totalLineDiffCount];
        int pairIdx = 0;
        foreach (var hunk in _hunks)
        {
            for (int i = 0; i < hunk.LineDiffs.Length; i++)
            {
                int lineNumber = hunk.Header.StartA + i;
                pairs[pairIdx] = new(lineNumber, hunk.LineDiffs[i]);
                pairIdx++;
            }
        }

        // https://learn.microsoft.com/en-us/dotnet/api/system.collections.frozen.frozendictionary.tofrozendictionary
        // "If the same key appears multiple times in the input, the last one in the sequence takes precedence."
        // "This differs from ToDictionary, where duplicate keys result in an exception."
        return pairs.ToFrozenDictionary();
    }
}
