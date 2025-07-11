using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Code.Commands
{
    public struct CommandResult 
    {
        public static CommandResult Ok()
        {
            return new CommandResult();
        }

        public static CommandResult Fail()
        {
            return new CommandResult();
        }
    }
}
