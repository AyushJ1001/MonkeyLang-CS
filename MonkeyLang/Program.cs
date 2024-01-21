// See https://aka.ms/new-console-template for more information

using MonkeyLang.Lexing;

var user = System.Environment.UserName;
Console.WriteLine($"Hello {user}! This is the Monkey Programming Language!");
Console.WriteLine("Feel free to type in commands");
Start();

return;

void Start()
{
    const string prompt = ">> ";

    while (true)
    {
        Console.Write(prompt);
        var line = Console.ReadLine();

        if (line == null) return;

        Lexer lexer = new(line);
        var token = lexer.NextToken();

        while (token.TokenType != TokenType.Eof)
        {
            Console.WriteLine(token);
            token = lexer.NextToken();
        }
    }
}