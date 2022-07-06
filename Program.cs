using System.Collections.Concurrent;
using System.Text.RegularExpressions;

var warningsFile = args.Last();
// System.Console.WriteLine(warningsFile);
Regex fileAndWarning = new Regex(@"(.*)/([.\w]+.cs)\(\d+,\d+\): warning (\w+): (.*)\[.*$");
ConcurrentDictionary<string, List<Warning>> warnings = new ConcurrentDictionary<string, List<Warning>>();
foreach (var warningLine in File.ReadAllLines(warningsFile))
{
    var matches = fileAndWarning.Matches(warningLine);
    foreach (Match match in matches)
    {
        GroupCollection groups = match.Groups;
        var path = groups[1].Value;
        var filename = groups[2].Value;
        var code = groups[3].Value;
        var text = groups[4].Value;
        var warning = new Warning(path, filename, code, text);
        warnings.AddOrUpdate(code, _ => new List<Warning>() { warning }, (_, items) => { items.Add(warning); return items; });
    }
}
warnings.OrderBy(kvp => kvp.Value.Count).ThenBy(kvp => kvp.Key).ToList().ForEach(kvp => PrintWarningSummary(kvp.Key, kvp.Value));

void PrintWarningSummary(string warningCode, List<Warning> warnings)
{
    ConcurrentDictionary<string, (int count, int nonTest, string message)> messageCounts = new ConcurrentDictionary<string, (int count, int nonTest, string message)>();
    warnings.ForEach(warning => messageCounts.AddOrUpdate(
        UnifyWarningText(warning.Code, warning.Text),
        (1, warning.IsTestCode ? 0 : 1, warning.Text),
        (_, t) => (t.count + 1, t.nonTest + (warning.IsTestCode ? 0 : 1), warning.Text)));
    foreach (var messageCount in messageCounts.OrderBy(kvp => kvp.Value.nonTest))
    {
        var message = messageCount.Value.count == 1 ? messageCount.Value.message : messageCount.Key;
        if (messageCounts.Count > 1)
            System.Console.WriteLine("{0,3} {1,-12} {2,3} {3,3}P {4}", warnings.Count, warningCode, messageCount.Value.count, messageCount.Value.nonTest, message);
        else
            System.Console.WriteLine("{0,3} {1,-12}     {2,3}P {3}", warnings.Count, warningCode, messageCount.Value.nonTest, message);
    }
}

static string UnifyWarningText(string warning, string text)
{
    Regex quotedName = new Regex(@"(^|\s)'.*?'");
    Regex inQuote = new Regex(@" in '.*?' ");
    Regex listT = new Regex(@"'List<.*?>'");
    Regex typeName = new Regex(@"[tT]ype (name )?[.\w]+");
    Regex methodName = new Regex(@"[mM]ethod [.\w]+");
    Regex typeShouldOverride = new Regex(@"[.\w]+ should override");
    Regex constructor = new Regex(@"(\w+): public \1");

    var unified = warning switch
    {
        // CS1570 xml comment (why)
        // CA1711 Rename type (enum / collection etc.)
        "CA1815" => typeShouldOverride.Replace(text, "'...' should override"),
        // CA1816 Change IndexedEnumerator<T>.Dispose()
        // CA1067 Type SuperEnums.SuperEnum
        // CA1058 Change the base type of Contract.Api.UnitTests.Exceptions.TestExpectationFailureException so that it no longer extends System.ApplicationException. This base exception type does not provide any additional value for framework classes. Extend '...' or an existing unsealed exception type instead. Do not create a new exception base type unless there is specific value in enabling the creation of a catch handler for an entire class of exceptions.
        // CA1725 In member Task<IEnumerable<string>> CreateQuickBusinessHandler.Handle(CreateQuickBusinessCommand command, CancellationToken cancellationToken), change parameter name command to request in order to match the identifier as it has been declared in Task<IEnumerable<string>> IRequestHandler<CreateQuickBusinessCommand, IEnumerable<string>>.Handle(CreateQuickBusinessCommand request, CancellationToken cancellationToken)
        "CA1067" => typeName.Replace(text, "type '...'"),
        "CA1032" => constructor.Replace(text, "'...Exception': public ...Exception"),
        "CA1711" => typeName.Replace(text, "type '...'"),
        "CA2208" => quotedName.Replace(methodName.Replace(text, "method '...'"), " '...'"),
        "CA1034" => typeName.Replace(text, "type '...'"),
        "CA1304" => inQuote.Replace(text, " in '...' "),
        "CA1310" => inQuote.Replace(text, " in '...' "),
        "CA1308" => quotedName.Replace(text, " '...'", 1),
        "CA2201" => typeName.Replace(text, "type '...'"),
        "CA1002" => listT.Replace(inQuote.Replace(text, " in '...' "), "'List<...>'"),
        _ => quotedName.Replace(text, " '...'"),
    };
    return unified;
}

public record Warning(string Path, string Filename, string Code, string Text)
{
    public bool IsTestCode => Path.Contains("test", StringComparison.InvariantCultureIgnoreCase);
}
