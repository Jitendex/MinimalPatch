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
    public HunkHeader Header { get; }
    public List<LineOperation>[] LineOperations { get; }

    public Hunk(ReadOnlySpan<char> header)
    {
        try
        {
            Header = new HunkHeader(header);
        }
        catch (Exception ex)
        {
            throw new InvalidPatchException($"Cannot parse patch hunk header `{header}`", ex);
        }

        // Note that Header.LengthA could be zero or negative here.
        int operationsCount = int.Max(1, Header.LengthA);

        LineOperations = new List<LineOperation>[operationsCount];
        for (int i = 0; i < operationsCount; i++)
        {
            // Adjusting the initial capacity of the lists
            // doesn't appear to affect performance much.
            LineOperations[i] = [];
        }
    }
}
