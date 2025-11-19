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

using BenchmarkDotNet.Attributes;
using MinimalPatch.Test;

namespace MinimalPatch.Benchmark;

public class Benchmarks
{
    private readonly PatchTest _patchTest = new();
    private readonly GnuPatchTest _gnuPatchTest = new();

    [Benchmark]
    public void PatchApply1()
    {
        _patchTest.PatchApplyTest1();
    }

    [Benchmark]
    public void PatchApply2()
    {
        _patchTest.PatchApplyTest2();
    }

    [Benchmark]
    public void PatchApply3()
    {
        _patchTest.PatchApplyTest3();
    }

    [Benchmark]
    public void PatchApply4()
    {
        _patchTest.PatchApplyTest4();
    }

    [Benchmark]
    public void GnuPatchApply1()
    {
        _gnuPatchTest.PatchApplyTest1();
    }

    [Benchmark]
    public void GnuPatchApply2()
    {
        _gnuPatchTest.PatchApplyTest2();
    }

    [Benchmark]
    public void GnuPatchApply3()
    {
        _gnuPatchTest.PatchApplyTest3();
    }

    [Benchmark]
    public void GnuPatchApply4()
    {
        _gnuPatchTest.PatchApplyTest4();
    }
}
