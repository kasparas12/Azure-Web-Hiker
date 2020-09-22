using System;

namespace Azure.Web.Hiker.Core.AgentRegistrar.Exceptions
{
    public class MaxAgentsRegisteredException : Exception
    {
        public MaxAgentsRegisteredException(string msg) : base(msg)
        {

        }
    }
}
