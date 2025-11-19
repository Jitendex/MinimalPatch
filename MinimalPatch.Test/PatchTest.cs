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
public sealed class PatchTest
{
    [TestMethod]
    public void PatchApplyTest1()
    {
        PatchApplyTest(1);
    }

    [TestMethod]
    public void PatchApplyTest2()
    {
        PatchApplyTest(2);
    }

    private static void PatchApplyTest(int number)
    {
        var diff = File.ReadAllText(Path.Join("Data", $"hamlet_ending_{number}.patch"));
        var original = File.ReadAllText(Path.Join("Data", "hamlet_ending_old.txt"));
        var expected = File.ReadAllText(Path.Join("Data", "hamlet_ending_new.txt"));
        var actual = Patch.Apply(diff, original);
        Assert.AreEqual(expected, actual);
    }

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

        using StreamReader inStream = new(Path.Join("Data", "hamlet_ending_old.txt"));
        using MemoryStream memStream = new();
        using StreamWriter outStream = new(memStream);

        await Patch.ApplyAsync(diff, inStream, outStream);

        await outStream.FlushAsync();
        memStream.Position = 0;
        using StreamReader outStreamReader = new(memStream);

        var actual = await outStreamReader.ReadToEndAsync();

        Assert.AreEqual(expected, actual);
    }
}
