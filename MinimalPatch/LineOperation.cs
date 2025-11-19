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

namespace MinimalPatch;

internal sealed class LineOperation
{
    public required string Text { get; init; }
    public required Operation Operation { get; init; }

    public bool IsALine() => Operation != Operation.Insert;  // All A-Lines are either `Equal` or `Delete`
    public bool IsBLine() => Operation != Operation.Delete;  // All B-Lines are either `Equal` or `Insert`

    public override string ToString() => $"{OpToChar(Operation)}{Text}";

    private static char OpToChar(Operation op) => op switch
    {
        Operation.Equal => ' ',
        Operation.Delete => '-',
        Operation.Insert => '+',
        _ => throw new ArgumentOutOfRangeException(nameof(op))
    };
}
