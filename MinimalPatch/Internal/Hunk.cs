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

using System.Collections.ObjectModel;

namespace MinimalPatch.Internal;

internal sealed class Hunk
{
    public HunkHeader Header { get; }
    public ReadOnlyDictionary<int, List<LineOperation>> LineOperations { get; }

    public Hunk(ReadOnlySpan<char> header)
    {
        Header = new HunkHeader(header);

        Dictionary<int, List<LineOperation>> lineOperations = new(Header.LengthA);
        for (int i = 0; i < Header.LengthA; i++)
        {
            // Adjusting the initial capacity of the lists
            // doesn't appear to affect performance much.
            lineOperations.Add(Header.StartA + i, []);
        }

        // `.AsReadOnly()` because no additional keys should be added.
        LineOperations = lineOperations.AsReadOnly();
    }

    public bool LengthsAreConsistent()
    {
        int lengthA = 0;
        int lengthB = 0;
        foreach (var operations in LineOperations.Values)
        {
            foreach (var operation in operations)
            {
                if (operation.IsOriginalLine()) lengthA++;
                if (operation.IsOutputLine()) lengthB++;
            }
        }
        return Header.LengthA == lengthA && Header.LengthB == lengthB;
    }
}
