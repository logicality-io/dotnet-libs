﻿namespace LittleForker
{
    /// <summary>
    /// Defined how a process is expected to run.
    /// </summary>
    public enum ProcessRunType
    {
        /// <summary>
        ///     Processes that are expected to terminate of their own accord.
        /// </summary>
        SelfTerminating,
        /// <summary>
        ///     Processes that are not expected to terminiate of their own
        ///     accord and that must be co-operatively shutdown or killed.
        /// </summary>
        NonTerminating
    }
}