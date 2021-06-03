using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SoftUpdaterClient.Service
{
    public class UpdateScriptParser : IUpdateScriptParser
    {
        private IServiceProvider _serviceProvider;
        private ILogger _logger;
        private readonly string[] _functionWords = new string[] {
           "if", "or", "and", "not"
        };


        public UpdateScriptParser(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _logger = _serviceProvider.GetRequiredService<ILogger<UpdateScriptParser>>();
        }

        public List<Command> Parse(string[] rows)
        {
            List<Command> result = new List<Command>();
            try
            {

                foreach (var row in rows)
                {
                    int cursor = 0;
                    var command = new Command() { Id = Guid.NewGuid() };
                    command.Name = ParseCommandName(row, ref cursor);
                    command.Condition = ParseCommandCondition(row, ref cursor, result);
                    command.CommandType = ParseCommandType(row, ref cursor);
                    command.Arguments = ParseCommandArguments(row, cursor);
                    result.Add(command);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка парсинга скрипта: {ex.Message} {ex.StackTrace}");
            }
            return result;
        }

        private string ParseCommandName(string row, ref int cursor)
        {
            var pos = row.IndexOf(':');
            var result = row.Substring(0, pos).Trim();
            cursor = pos + 1;
            return result;
        }

        private ConditionExpression ParseCommandCondition(string row, ref int cursor, List<Command> result)
        {
            throw new NotImplementedException();
        }

        private CommandEnum ParseCommandType(string row, ref int cursor)
        {
            throw new NotImplementedException();
        }

        private Dictionary<string, string> ParseCommandArguments(string row, int cursor)
        {
            throw new NotImplementedException();
        }
    }

    public enum CommandEnum
    {         
       Backup = 1,
       Install = 2,
       Rollback = 3,
       CMD = 4,
        Stop = 5,
        Start = 6
    }

    public enum ConditionEnum
    { 
       And, Or, Not
    }

    public interface ICondition
    {
        bool GetResult();
    }

    public class Condition : ICondition
    {
        public bool GetResult()
        {
            throw new NotImplementedException();
        }
    }

    public class ConditionExpression: ICondition
    { 
        public ConditionEnum ConditionEnum { get; set; }
        public List<ICondition> Conditions { get; set; }

        public bool GetResult()
        {            
            switch (ConditionEnum)
            {
                case ConditionEnum.And:
                    if (Conditions.Count < 2) 
                        throw new ConditionExpressionException("Некорректное условие: оператор AND: должно быть не менее двух входящих условий");                    
                    foreach (var cond in Conditions)
                    {
                        if (!cond.GetResult()) return false;
                    }
                    return true;
                case ConditionEnum.Or:
                    if (Conditions.Count < 2)
                        throw new ConditionExpressionException("Некорректное условие: оператор OR: должно быть не менее двух входящих условий");                    
                    foreach (var cond in Conditions)
                    {
                        if (cond.GetResult()) return true;
                    }
                    return false;
                case ConditionEnum.Not:
                    if (Conditions.Count !=1)
                        throw new ConditionExpressionException("Некорректное условие: оператор NOT: должно быть одно входящее условие");
                    return !Conditions[0].GetResult();
                default: throw new ConditionExpressionException("Некорректное условие: некорректный ConditionEnum");
            }
        }
    }

    public class Command
    { 
        public Guid Id { get; set; }
        public string Name { get; set; }
        public CommandEnum CommandType { get; set; }
        public Dictionary<string, string> Arguments { get; set; }
        public ConditionExpression Condition { get; set; }
        public bool Result { get; set; }
    }


}
