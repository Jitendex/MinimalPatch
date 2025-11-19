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

namespace MinimalPatch.Test;

[TestClass]
public sealed class PatchAsyncTest
{
    [TestMethod]
    public async Task PatchApplyAsyncTest1()
    {
        await PatchApplyAsyncTest(1);
    }

    [TestMethod]
    public async Task PatchApplyAsyncTest2()
    {
        await PatchApplyAsyncTest(2);
    }

    private static async Task PatchApplyAsyncTest(int number)
    {
        var diff = File.ReadAllText(Path.Join("Data", $"hamlet_ending_{number}.patch"));
        var expected = File.ReadAllText(Path.Join("Data", "hamlet_ending_new.txt"));
        using StreamReader original = new(Path.Join("Data", "hamlet_ending_old.txt"));

        using MemoryStream actualStream = new();
        using StreamWriter actualWriter = new(actualStream);
        using StreamReader actualReader = new(actualStream);

        await Patch.ApplyAsync(diff, original, actualWriter);

        await actualWriter.FlushAsync();
        actualStream.Position = 0;
        var actual = await actualReader.ReadToEndAsync();

        Assert.AreEqual(expected, actual);
    }
}
