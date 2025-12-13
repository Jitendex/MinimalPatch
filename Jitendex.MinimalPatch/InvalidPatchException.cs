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

namespace Jitendex.MinimalPatch;

/// <summary>
/// Exception that is thrown when a given patch text cannot be parsed or is inconsistent with the given target text.
/// </summary>
public class InvalidPatchException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidPatchException"/> class.
    /// </summary>
    public InvalidPatchException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidPatchException"/> class.
    /// </summary>
    public InvalidPatchException(string message, Exception innerException) : base(message, innerException) { }
}
