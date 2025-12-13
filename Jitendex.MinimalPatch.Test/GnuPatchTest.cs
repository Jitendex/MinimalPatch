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

using System.Diagnostics;

namespace Jitendex.MinimalPatch.Test;

[Ignore]
[TestClass]
public sealed class GnuPatchTest
{
    [TestMethod]
    public void PatchApplyTest1()
    {
        PatchApplyTest("ending", 1);
    }

    [TestMethod]
    public void PatchApplyTest2()
    {
        PatchApplyTest("ending", 2);
    }

    [TestMethod]
    public void PatchApplyTest3()
    {
        PatchApplyTest("act1", 1);
    }

    [TestMethod]
    public void PatchApplyTest4()
    {
        PatchApplyTest("full", 1);
    }

    private static void PatchApplyTest(string size, int number)
    {
        var tmpfile = $"/tmp/hamlet-{Guid.NewGuid()}.txt";
        using var process = new Process
        {
            StartInfo =
            {
                FileName = "patch",
                WorkingDirectory = "Data",
                Arguments = $"--quiet hamlet_{size}_old.txt hamlet_{size}_{number}.patch -o {tmpfile}",
                RedirectStandardOutput = true,
            }
        };
        process.Start();
        process.WaitForExit();

        var actual = File.ReadAllText(tmpfile);
        var expected = File.ReadAllText(Path.Join("Data", $"hamlet_{size}_new.txt"));

        if (File.Exists(tmpfile))
        {
            File.Delete(tmpfile);
        }

        Assert.AreEqual(expected, actual);
    }
}
