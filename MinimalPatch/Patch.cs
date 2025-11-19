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

using System.Text;

namespace MinimalPatch;

public static class Patch
{
    public static async Task ApplyAsync(string diffText, StreamReader inStream, StreamWriter outStream)
    {
        var readTask = inStream.ReadLineAsync();
        var writeTask = Task.CompletedTask;

        UnifiedDiff diff = new(diffText);
        var lineNumToOps = diff.GetLineNumberToOperationsDictionary();

        int lineNumber = 1;
        while (await readTask is string text)
        {
            readTask = inStream.ReadLineAsync();
            if (lineNumToOps.TryGetValue(lineNumber, out var ops))
            {
                foreach (var op in ops)
                {
                    if (op.IsALine())
                    {
                        if (!string.Equals(text, op.Text, StringComparison.Ordinal))
                        {
                            throw new ArgumentException(
                                $"Line #{lineNumber} of text file does not match diff");
                        }
                    }
                    if (op.IsBLine())
                    {
                        await writeTask;
                        writeTask = outStream.WriteLineAsync(op.Text);
                    }
                }
            }
            else
            {
                await writeTask;
                writeTask = outStream.WriteLineAsync(text);
            }
            lineNumber++;
        }

        await writeTask;
    }

    public static string Apply(ReadOnlySpan<char> diffText, ReadOnlySpan<char> text)
    {
        StringBuilder sb = new();
        Range currentRange = default;

        UnifiedDiff diff = new(diffText);
        var lineNumToOps = diff.GetLineNumberToOperationsDictionary();

        int lineNumber = 0;
        foreach (var range in text.Split('\n'))
        {
            lineNumber++;
            if (lineNumToOps.TryGetValue(lineNumber, out var ops))
            {
                if (!currentRange.Equals(default))
                {
                    if (sb.Length > 0) sb.AppendLine();
                    sb.Append(text[currentRange]);
                    currentRange = default;
                }

                foreach (var op in ops)
                {
                    if (op.IsALine())
                    {
                        if (!text[range].Equals(op.Text, StringComparison.Ordinal))
                        {
                            throw new ArgumentException(
                                $"Line #{lineNumber} of text file does not match diff");
                        }
                    }
                    if (op.IsBLine())
                    {
                        if (sb.Length > 0) sb.AppendLine();
                        sb.Append(op.Text);
                    }
                }
            }
            else
            {
                currentRange = currentRange.Equals(default)
                    ? range
                    : new Range(currentRange.Start, range.End);
            }
        }

        if (!currentRange.Equals(default))
        {
            if (sb.Length > 0) sb.AppendLine();
            sb.Append(text[currentRange]);
        }

        return sb.ToString();
    }
}
