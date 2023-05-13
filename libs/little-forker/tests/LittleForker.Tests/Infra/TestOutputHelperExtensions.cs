﻿using Xunit.Abstractions;

namespace Logicality.LittleForker.Infra;

public static class TestOutputHelperExtensions
{
    public static void WriteLine2(this ITestOutputHelper outputHelper, object o)
    {
        if (o != null)
        {
            outputHelper.WriteLine(o.ToString());
        }
    }
}