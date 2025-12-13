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
public sealed class SoloInsert
{
    private const string Original =
        """
        A
        B
        """;

    [TestMethod]
    public void InsertOne()
    {
        var diff =
            """
            --- AB
            +++ 1AB
            @@ -1,1 +1,2 @@
            +1
             A
            """;

        var expected =
            """
            1
            A
            B
            """;

        var actual = Patch.Apply(diff, Original);
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void InsertTwo()
    {
        var diff =
            """
            --- AB
            +++ A2B
            @@ -1,1 +1,2 @@
             A
            +2
            """;

        var expected =
            """
            A
            2
            B
            """;

        var actual = Patch.Apply(diff, Original);
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void InsertThree()
    {
        var diff =
            """
            --- AB
            +++ AB3
            @@ -2,1 +2,2 @@
             B
            +3
            """;

        var expected =
            """
            A
            B
            3
            """;

        var actual = Patch.Apply(diff, Original);
        Assert.AreEqual(expected, actual);
    }
}
