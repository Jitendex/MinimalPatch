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

namespace MinimalPatch.Internal;

internal sealed class Hunk
{
    public int StartA { get; }
    public int LengthA { get; }
    public int StartB { get; }
    public int LengthB { get; }
    public Dictionary<int, List<LineOperation>> LineOperations { get; }

    public Hunk(ReadOnlySpan<char> header)
    {
        bool seenHeaderStart = false;
        foreach (var range in header.Split(' '))
        {
            var text = header[range];
            if (text.StartsWith('-'))
            {
                (StartA, LengthA) = GetStartAndLength(text[1..]);
            }
            else if (text.StartsWith('+'))
            {
                (StartB, LengthB) = GetStartAndLength(text[1..]);
            }
            else if (text.StartsWith('@') && !seenHeaderStart)
            {
                seenHeaderStart = true;
            }
            else
            {
                break;
            }
        }
        LineOperations = Enumerable.Range(StartA, LengthA)
            .Select(static x => new KeyValuePair<int, List<LineOperation>>(x, []))
            .ToDictionary();
    }

    private static (int, int) GetStartAndLength(ReadOnlySpan<char> text)
    {
        int i = text.IndexOf(',');
        if (i == -1)
        {
            return (int.Parse(text), 1);
        }
        else
        {
            int start = int.Parse(text[..i]);
            int length = int.Parse(text[(i + 1)..]);
            return (start, length);
        }
    }

    public bool LengthsAreConsistent()
    {
        int aCount = 0;
        int bCount = 0;
        foreach (var opList in LineOperations.Values)
        {
            foreach (var op in opList)
            {
                if (op.IsOriginalLine()) aCount++;
                if (op.IsOutputLine()) bCount++;
            }
        }
        return LengthA == aCount && LengthB == bCount;
    }
}
