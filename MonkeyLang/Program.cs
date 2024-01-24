// See https://aka.ms/new-console-template for more information

using MonkeyLang.Evalutation;
using MonkeyLang.Lexing;
using MonkeyLang.Parsing;
const string MONKEY_FACE = """"""""
                                      __,__
                             .--.  .-"     "-.  .--.
                            / .. \/  .-. .-.  \/ .. \
                           | |  '|  /   Y   \  |'  | |
                           | \   \  \ 0 | 0 /  /   / |
                           \  '- ,\.-"""""""-./, -' /
                            ''-'  /_   ^ ^   _\ '-''
                                 |  \._   _./  |
                                 \   \ '~' /   /
                                  '._ '-=-' _.'
                                     '-----'
                           """""""";

var user = System.Environment.UserName;
Console.WriteLine($"Hello {user}! This is the Monkey Programming Language!");
Console.WriteLine("Feel free to type in commands");
Start();

return;

void Start()
{

    const string prompt = ">> ";
    MonkeyLang.Environment environment = new();

    while (true)
    {
        Console.Write(prompt);
        var line = Console.ReadLine();

        if (line == null) return;

        Lexer lexer = new(line);
        Parser parser = new(lexer);
        var program = parser.ParseProgram();

        if (parser.Errors().Count != 0)
        {
            PrintParseErrors(parser.Errors());
            continue;
        }

        var evaluated = Evaluator.Eval(program, environment);


        Console.WriteLine(evaluated.Inspect());
    }
}

void PrintParseErrors(IList<string> errors)
{
    Console.Error.WriteLine(MONKEY_FACE);
    Console.Error.WriteLine("Woops! We ran into some monkey business here!");
    Console.Error.WriteLine(" parser errors:");
    foreach (var error in errors)
    {
        Console.Error.WriteLine($"\t{error}");
    }
}