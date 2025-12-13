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

namespace Jitendex.MinimalPatch.Test;

[TestClass]
public sealed class SoloDeletion
{
    private const string Original =
        """
        1
        2
        3
        """;

    [TestMethod]
    public void DeleteOne()
    {
        var diff =
            """
            --- 123
            +++ 23
            @@ -1,1 +1,0 @@
            -1
            """;

        var expected =
            """
            2
            3
            """;

        var actual = Patch.Apply(diff, Original);
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void DeleteTwo()
    {
        var diff =
            """
            --- 123
            +++ 13
            @@ -2,1 +2,0 @@
            -2
            """;

        var expected =
            """
            1
            3
            """;

        var actual = Patch.Apply(diff, Original);
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void DeleteThree()
    {
        var diff =
            """
            --- 123
            +++ 12
            @@ -3,1 +3,0 @@
            -3
            """;

        var expected =
            """
            1
            2
            """;

        var actual = Patch.Apply(diff, Original);
        Assert.AreEqual(expected, actual);
    }
}
