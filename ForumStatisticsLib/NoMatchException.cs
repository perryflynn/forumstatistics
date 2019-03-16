using System;
using System.Collections.Generic;
using System.Text;

namespace PerryFlynn.ForumStatistics.Parser
{
    /// <summary>
    /// Exception thrown then some regex delivers no match
    /// </summary>
    public class NoMatchException : Exception
    {
        public NoMatchException(string property, string regex) :
            base($"Cannot match for property '{property}' with '{regex}'")
        { }
    }
}
