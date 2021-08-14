using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SoftUpdater.ClientHttpClient;
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
           "or", "and", "not"
        };
        private IClientHttpClient httpClient;

        public UpdateScriptParser(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _logger = _serviceProvider.GetRequiredService<ILogger<UpdateScriptParser>>();
            httpClient = _serviceProvider.GetRequiredService<IClientHttpClient>();
        }

        public List<Command> Parse(string[] rows)
        {
            List<Command> result = new List<Command>();
            try
            {
                int rowNum = 1;
                foreach (var row in rows)
                {                    
                    var command = new Command() { Id = Guid.NewGuid() };
                    var literals = ParseRow(row, result);
                    if (literals[0] is CustomLiteral commandName)
                    {
                        command.Name = commandName.Text;
                    }
                    else
                    {
                        throw new Exception($"Некорректное наименование команды в строке {rowNum}");
                    }
                    if (literals[1].LiteralType != LiteralTypeEnum.Delimiter)
                    {
                        throw new Exception($"Не установлен знак : в строке {rowNum} после наименования команды");
                    }
                    
                    int lastPos = 2;
                    List<ILiteral> conditionList = new List<ILiteral>();
                    bool isSetCommand = false;

                    while (lastPos < literals.Count)
                    {
                        if (literals[lastPos] is CommandLiteral commandLiteral)
                        {
                            command.CommandType = commandLiteral.Command;
                            isSetCommand = true;
                            break;
                        }
                        conditionList.Add(literals[lastPos]);
                        lastPos++;
                    }
                    if(!isSetCommand) throw new Exception($"Не найдена команда в строке {rowNum}");
                    command.Condition = BuildCommandCondition(conditionList);

                    while (lastPos < literals.Count)
                    {
                        if (literals[lastPos] is CustomLiteral argument)
                        {
                            command.Arguments.Add(argument.Text);
                            lastPos++;
                        }
                        else throw new Exception($"Некорректный аргумент в строке {rowNum}");
                    }
                      
                    result.Add(command);
                    rowNum++;
                }
            }
            catch (Exception ex)
            {
                httpClient.SendErrorMessage($"Ошибка парсинга скрипта: {ex.Message} {ex.StackTrace}");
                _logger.LogError($"Ошибка парсинга скрипта: {ex.Message} {ex.StackTrace}");
            }
            return result;
        }

        private ConditionExpression BuildCommandCondition(List<ILiteral> conditionList)
        {
            try
            {
                if (!conditionList.Any())
                {
                    return new ConditionExpression()
                    {
                        ConditionEnum = ConditionEnum.None,
                        Result = true
                    };
                }
                var first = conditionList.First();
                var last = conditionList.Last();
                if (first.LiteralType != LiteralTypeEnum.BraceOpen) throw new Exception("Выражение условия должно начинаться с символа (");
                if (last.LiteralType != LiteralTypeEnum.BraceClose) throw new Exception("Выражение условия должно заканчиваться символом )");
                if (conditionList.Count < 3) throw new Exception("Условие не содержит выражение");

                conditionList.RemoveAt(0);
                conditionList.RemoveAt(conditionList.Count - 1);

                for (int i = 0; i < conditionList.Count; i++)
                {
                    var condition = conditionList[i];
                    if (condition is CommandExpLiteral command)
                    {
                        conditionList[i] = new ExpressionLiteral(new ConditionExpression()
                        {
                            CommandId = command.CommandId,
                            ConditionEnum = ConditionEnum.Command
                        });
                    }
                    else if (condition.LiteralType == LiteralTypeEnum.Command || condition.LiteralType == LiteralTypeEnum.Custom
                        || condition.LiteralType == LiteralTypeEnum.Delimiter)
                    {
                        throw new Exception("Не удалось распарсить выражение условия");
                    }
                }

                int cursor = 0;

                List<Func<bool>> transformFunctions = new List<Func<bool>>()
            {
                // not + exp => notExp
               ()=>{
                  if(conditionList[cursor] is ServiceWordLiteral serviceWord &&
                        serviceWord.ServiceWordEnum == ServiceWordEnum.Not &&
                        conditionList.Count > cursor + 1 &&
                        conditionList[cursor + 1] is ExpressionLiteral expressionLiteral)
                  {
                       var newExpression = new ConditionExpression()
                       {
                           ConditionEnum = ConditionEnum.Not,
                           Conditions = new List<ConditionExpression>()
                           {
                              expressionLiteral.Expression
                           }
                       };
                       conditionList.RemoveAt(cursor+1);
                       conditionList.RemoveAt(cursor);
                       conditionList.Insert(cursor, new ExpressionLiteral(newExpression));
                       return true;
                  }
                  return false;
               },
               //( exp ) => exp
               ()=>{
                  if(conditionList[cursor].LiteralType == LiteralTypeEnum.BraceOpen &&
                        conditionList.Count > cursor + 2 &&
                        conditionList[cursor + 2].LiteralType == LiteralTypeEnum.BraceClose &&
                        conditionList[cursor + 1] is ExpressionLiteral expressionLiteral)
                  {
                       conditionList.RemoveAt(cursor+2);
                       conditionList.RemoveAt(cursor);
                       return true;
                  }
                  return false;
               },
               //exp or exp => exp
               ()=>{
                  if(conditionList.Count > cursor + 2
                       && conditionList[cursor] is ExpressionLiteral expressionLiteral1
                       && conditionList[cursor + 2] is ExpressionLiteral expressionLiteral2
                       && conditionList[cursor + 1] is ServiceWordLiteral wordLiteral
                       && wordLiteral.ServiceWordEnum == ServiceWordEnum.Or
                       && (cursor == 0
                            || conditionList[cursor-1].LiteralType == LiteralTypeEnum.BraceOpen
                            || (conditionList[cursor-1] is ServiceWordLiteral wordLiteral2 && wordLiteral2.ServiceWordEnum == ServiceWordEnum.Or))
                       && (conditionList.Count == cursor + 3
                            || conditionList[cursor + 3].LiteralType == LiteralTypeEnum.BraceClose
                            || (conditionList[cursor + 3] is ServiceWordLiteral wordLiteral3 && wordLiteral3.ServiceWordEnum == ServiceWordEnum.Or)))
                  {
                        var newExpression = new ConditionExpression()
                       {
                           ConditionEnum = ConditionEnum.Or,
                           Conditions = new List<ConditionExpression>()
                           {
                              expressionLiteral1.Expression, expressionLiteral2.Expression
                           }
                       };
                       conditionList.RemoveAt(cursor+2);
                       conditionList.RemoveAt(cursor+1);
                       conditionList.RemoveAt(cursor);
                       conditionList.Insert(cursor, new ExpressionLiteral(newExpression));
                       return true;
                  }
                  return false;
               },
               //exp and exp => exp
               ()=>{
                  if( conditionList.Count > cursor + 2
                       && conditionList[cursor] is ExpressionLiteral expressionLiteral1
                       && conditionList[cursor + 2] is ExpressionLiteral expressionLiteral2
                       && conditionList[cursor + 1] is ServiceWordLiteral wordLiteral
                       && wordLiteral.ServiceWordEnum == ServiceWordEnum.And )
                  {
                       var newExpression = new ConditionExpression()
                       {
                           ConditionEnum = ConditionEnum.And,
                           Conditions = new List<ConditionExpression>()
                           {
                              expressionLiteral1.Expression, expressionLiteral2.Expression
                           }
                       };
                       conditionList.RemoveAt(cursor+2);
                       conditionList.RemoveAt(cursor+1);
                       conditionList.RemoveAt(cursor);
                       conditionList.Insert(cursor, new ExpressionLiteral(newExpression));
                       return true;
                  }
                  return false;
               }
            };

                bool isChanged = true;

                while (isChanged)
                {
                    isChanged = false;
                    cursor = 0;
                    while (conditionList.Count > 1 && cursor < conditionList.Count - 1)
                    {
                        bool changedCycle = false;
                        foreach (var func in transformFunctions)
                        {
                            if (func())
                            {
                                isChanged = true;
                                changedCycle = true;
                            }
                        }
                        if (!changedCycle)
                        {
                            cursor++;
                        }
                    }
                }
                if (conditionList.Count == 1 && conditionList[0] is ExpressionLiteral expression)
                {
                    return expression.Expression;
                }
                throw new Exception("Не удалось распарсить выражение условия");
            }
            catch (Exception ex)
            {
                httpClient.SendErrorMessage($"Ошибка парсинга скрипта: {ex.Message} {ex.StackTrace}");
                throw;
            }
        }
                
        private string GetNextWord(string row, ref int cursor)
        {
            string result = "";
            bool isQuote = false;
            for (int i = cursor; i < row.Length; i++)
            {
                if (isQuote)
                {
                    if (row[i] == '\"')
                    {
                        cursor++;
                        return result;
                    }
                    cursor++;
                    result += row[i];
                }
                else
                {
                    switch (row[i])
                    {
                        case '\"':
                            cursor++;
                            isQuote = true;
                            break;
                        case ' ':
                            cursor++;
                            if (string.IsNullOrEmpty(result)) continue;
                            return result;
                        case '(':
                            if (string.IsNullOrEmpty(result))
                            {
                                cursor++;
                                return "(";
                            }
                            return result;
                        case ')':
                            if (string.IsNullOrEmpty(result))
                            {
                                cursor++;
                                return ")";
                            }
                            return result;
                        default:
                            cursor++;
                            result += row[i];
                            break;
                    }                                  
                }
            }
            return result;
        }

        private enum ServiceWordEnum
        {
            And, Or, Not
        }

        private interface ILiteral
        {
            LiteralTypeEnum LiteralType { get; }
        }

        private enum LiteralTypeEnum
        { 
           BraceOpen,
           BraceClose,
           ServiceWord,
           CommandExp,
           Command,
           Delimiter,
           Custom,
           Expression
        }

        private class Literal : ILiteral
        {
            public LiteralTypeEnum LiteralType { get; }
            public Literal(LiteralTypeEnum typeEnum)
            {
                LiteralType = typeEnum;
            }
        }

        private class CustomLiteral : Literal
        {
            public string Text { get; }
            public CustomLiteral(string text) : base(LiteralTypeEnum.Custom)
            {
                Text = text;
            }
        }

        private class ServiceWordLiteral : Literal
        {
            public ServiceWordEnum ServiceWordEnum { get; }
            public ServiceWordLiteral(ServiceWordEnum wordEnum) : base(LiteralTypeEnum.ServiceWord)
            {
                ServiceWordEnum = wordEnum;
            }
        }

        private class CommandExpLiteral : Literal
        { 
            public Guid CommandId { get; }
            public CommandExpLiteral(Guid commandId) : base(LiteralTypeEnum.CommandExp)
            {
                CommandId = commandId;
            }
        }

        private class CommandLiteral : Literal
        {
            public CommandEnum Command { get; }
            public CommandLiteral(CommandEnum command) : base(LiteralTypeEnum.Command)
            {
                Command = command;
            }
        }

        private class ExpressionLiteral : Literal
        {
            public ConditionExpression Expression { get; }
            public ExpressionLiteral(ConditionExpression expression) : base(LiteralTypeEnum.Expression)
            {
                Expression = expression;
            }
        }

        private List<ILiteral> ParseRow(string row, List<Command> commands)
        {
            int cursor = 0;
            List<ILiteral> literals = new List<ILiteral>();
            while (cursor < row.Length)
            {
                var word = GetNextWord(row, ref cursor);
                literals.Add(ParseLiteral(word, commands));
            }
            return literals;
        }

        private ILiteral ParseLiteral(string word, List<Command> commands)
        {
            if (Enum.TryParse(word, true, out CommandEnum command))
            {
                return new CommandLiteral(command);
            }
            if (word == ":") return new Literal(LiteralTypeEnum.Delimiter);
            if (word == "(") return new Literal(LiteralTypeEnum.BraceOpen);
            if (word == ")") return new Literal(LiteralTypeEnum.BraceClose);
            var commandExp = commands.FirstOrDefault(s=>s.Name.Equals(word, StringComparison.InvariantCultureIgnoreCase));
            if (commandExp != null)
            {
                return new CommandExpLiteral(commandExp.Id);
            }
            if (Enum.TryParse(word, true, out ServiceWordEnum serviceWord))
            {
                return new ServiceWordLiteral(serviceWord);
            }
            return new CustomLiteral(word);
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
       And, Or, Not, Command, None, Root
    }
        
    public class ConditionExpression
    {
        private class CheckCondition
        {
            public enum ConditionsCountTypeEnum
            { 
               None, Equal, Greater, Less, GreaterOrEqual, LessOrEqual
            }

            public ConditionsCountTypeEnum ConditionsCountType { get; set; }
            public int ConditionsCount { get; set; }
            public bool IsSetCommandId { get; set; }
            public bool IsSetResult { get; set; }
        }

        private Dictionary<ConditionEnum, CheckCondition> CheckConditions = new Dictionary<ConditionEnum, CheckCondition>()
        {
            { 
                ConditionEnum.And, 
                new CheckCondition(){  
                    ConditionsCountType = CheckCondition.ConditionsCountTypeEnum.Greater, 
                    ConditionsCount = 1, 
                    IsSetCommandId = false, 
                    IsSetResult = false 
                } 
            },
            {
                ConditionEnum.Command,
                new CheckCondition(){
                    ConditionsCountType = CheckCondition.ConditionsCountTypeEnum.Equal,
                    ConditionsCount = 0,
                    IsSetCommandId = true,
                    IsSetResult = false
                }
            },
            {
                ConditionEnum.None,
                new CheckCondition(){
                    ConditionsCountType = CheckCondition.ConditionsCountTypeEnum.Equal,
                    ConditionsCount = 0,
                    IsSetCommandId = false,
                    IsSetResult = true
                }
            },
            {
                ConditionEnum.Not,
                new CheckCondition(){
                    ConditionsCountType = CheckCondition.ConditionsCountTypeEnum.Equal,
                    ConditionsCount = 1,
                    IsSetCommandId = false,
                    IsSetResult = false
                }
            },
            {
                ConditionEnum.Or,
                new CheckCondition(){
                    ConditionsCountType = CheckCondition.ConditionsCountTypeEnum.Greater,
                    ConditionsCount = 1,
                    IsSetCommandId = false,
                    IsSetResult = false
                }
            },
            {
                ConditionEnum.Root,
                new CheckCondition(){
                    ConditionsCountType = CheckCondition.ConditionsCountTypeEnum.Equal,
                    ConditionsCount = 1,
                    IsSetCommandId = false,
                    IsSetResult = false
                }
            }
        };

        public ConditionEnum ConditionEnum { get; set; }
        public List<ConditionExpression> Conditions { get; set; }
        public Guid? CommandId { get; set; }
        public bool? Result { get; set; }

        public void CheckConditionFields()
        {
            CheckCondition condition = CheckConditions[ConditionEnum];
            switch (condition.ConditionsCountType)
            {
                case CheckCondition.ConditionsCountTypeEnum.None: break;
                case CheckCondition.ConditionsCountTypeEnum.Equal:
                    if(Conditions == null || Conditions.Count!= condition.ConditionsCount)
                        throw new ConditionExpressionException($"Некорректное условие: оператор {ConditionEnum}: должно быть {condition.ConditionsCount} входящих условий");
                    break;
                case CheckCondition.ConditionsCountTypeEnum.Greater:
                    if (Conditions == null || Conditions.Count <= condition.ConditionsCount)
                        throw new ConditionExpressionException($"Некорректное условие: оператор {ConditionEnum}: должно быть больше {condition.ConditionsCount} входящих условий");
                    break;
                case CheckCondition.ConditionsCountTypeEnum.GreaterOrEqual:
                    if (Conditions == null || Conditions.Count < condition.ConditionsCount)
                        throw new ConditionExpressionException($"Некорректное условие: оператор {ConditionEnum}: должно быть не меньше {condition.ConditionsCount} входящих условий");
                    break;
                case CheckCondition.ConditionsCountTypeEnum.Less:
                    if (Conditions == null || Conditions.Count >= condition.ConditionsCount)
                        throw new ConditionExpressionException($"Некорректное условие: оператор {ConditionEnum}: должно быть меньше {condition.ConditionsCount} входящих условий");
                    break;
                case CheckCondition.ConditionsCountTypeEnum.LessOrEqual:
                    if (Conditions == null || Conditions.Count > condition.ConditionsCount)
                        throw new ConditionExpressionException($"Некорректное условие: оператор {ConditionEnum}: должно быть не больше {condition.ConditionsCount} входящих условий");
                    break;                
            }
            if(condition.IsSetCommandId && CommandId == null) throw new ConditionExpressionException($"Некорректное условие: оператор {ConditionEnum}: команда должна быть установлена");
            if (!condition.IsSetCommandId && CommandId != null) throw new ConditionExpressionException($"Некорректное условие: оператор {ConditionEnum}: команда не должна быть установлена");
            if (condition.IsSetResult && Result == null) throw new ConditionExpressionException($"Некорректное условие: оператор {ConditionEnum}: результат должен быть установлен");
            if (!condition.IsSetResult && Result != null) throw new ConditionExpressionException($"Некорректное условие: оператор {ConditionEnum}: результат не должен быть установлен");
        }

        public bool GetResult(List<Command> commands)
        {
            CheckConditionFields();
            switch (ConditionEnum)
            {
                case ConditionEnum.And:                                     
                    foreach (var cond in Conditions)
                    {
                        if (!cond.GetResult(commands)) return false;
                    }
                    return true;
                case ConditionEnum.Or:                                     
                    foreach (var cond in Conditions)
                    {
                        if (cond.GetResult(commands)) return true;
                    }
                    return false;
                case ConditionEnum.Not:                    
                    return !Conditions[0].GetResult(commands);
                case ConditionEnum.Command:                    
                    return commands.Single(s => s.Id == CommandId).Result;
                case ConditionEnum.None:                   
                    return Result.Value;
                case ConditionEnum.Root:
                    return Conditions[0].GetResult(commands);
                default: throw new ConditionExpressionException("Некорректное условие: некорректный ConditionEnum");
            }
        }
    }

    public class Command
    { 
        public Guid Id { get; set; }
        public string Name { get; set; }
        public CommandEnum CommandType { get; set; }
        public List<string> Arguments { get; set; }
        public ConditionExpression Condition { get; set; }
        public bool Result { get; set; }
    }


}
