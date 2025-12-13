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

namespace Jitendex.MinimalPatch.Internal;

internal readonly record struct HunkHeader
{
    public readonly int StartA { get; }
    public readonly int LengthA { get; }
    public readonly int LengthB { get; }

    /// <summary>
    /// Parses hunk header in the format `@@ -StartA,LengthA +StartB,LengthB @@ [Comments]`
    /// </summary>
    public HunkHeader(ReadOnlySpan<char> text)
    {
        bool seenHeaderStart = false;
        foreach (var range in text.Split(' '))
        {
            var subtext = text[range];
            if (subtext.StartsWith('-'))
            {
                (StartA, LengthA) = Parse(subtext[1..]);
            }
            else if (subtext.StartsWith('+'))
            {
                // StartB isn't used for anything.
                (_, LengthB) = Parse(subtext[1..]);
            }
            else if (subtext.StartsWith('@') && !seenHeaderStart)
            {
                seenHeaderStart = true;
            }
            else
            {
                break;
            }
        }
        if (StartA < 1 || LengthA < 1 || LengthB < 0)
        {
            throw new InvalidPatchException($"Invalid hunk header `{text}`");
        }
    }

    private readonly ref struct HeaderElement
    {
        public readonly int Start { get; init; }
        public readonly int Length { get; init; }
        public void Deconstruct(out int start, out int length)
        {
            start = Start;
            length = Length;
        }
    }

    private static HeaderElement Parse(ReadOnlySpan<char> text)
        => text.IndexOf(',') switch
        {
            int idx and not -1 =>
                new HeaderElement
                {
                    Start = int.Parse(text[..idx]),
                    Length = int.Parse(text[(idx + 1)..]),
                },
            _ =>
                new HeaderElement
                {
                    Start = int.Parse(text),
                    Length = 1,
                }
        };
}
