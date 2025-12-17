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

namespace Jitendex.MinimalPatch.Internal;

internal readonly ref struct InputState
{
    public readonly ReadOnlySpan<char> Patch { get; }
    public readonly ReadOnlySpan<char> Original { get; }
    public readonly int ExpectedOutputLength { get; }
    public readonly FrozenDictionary<int, List<DiffLine>> LineNumberToDiffs { get; }

    public InputState(ReadOnlySpan<char> patch, ReadOnlySpan<char> original)
    {
        var unifiedDiff = Parse(patch);
        Patch = patch;
        Original = original;
        ExpectedOutputLength = original.Length + unifiedDiff.TotalCharacterCountDelta;
        LineNumberToDiffs = unifiedDiff.GetLineNumberToDiffs();
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
}
