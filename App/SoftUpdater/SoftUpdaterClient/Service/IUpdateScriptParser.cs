using System.Collections.Generic;

namespace SoftUpdaterClient.Service
{
    public interface IUpdateScriptParser
    {
        List<Command> Parse(string[] rows);
    }
}