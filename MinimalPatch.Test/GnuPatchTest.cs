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

using System.Diagnostics;

namespace MinimalPatch.Test;

[Ignore]
[TestClass]
public sealed class GnuPatchTest
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
        var p = new Process
        {
            StartInfo =
            {
                FileName = "patch",
                WorkingDirectory = "Data",
                Arguments = $"hamlet_ending_old.txt hamlet_ending_{number}.patch -o -",
                RedirectStandardOutput = true,
            }
        };
        p.Start();
        p.WaitForExit();

        var actual = p.StandardOutput.ReadToEnd();
        var expected = File.ReadAllText(Path.Join("Data", "hamlet_ending_new.txt"));
        Assert.AreEqual(expected, actual);
    }
}
