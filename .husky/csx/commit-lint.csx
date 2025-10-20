private var pattern = @"^(?=.{1,90}$)(?:build|feat|ci|chore|docs|fix|perf|refactor|revert|style|test)(?:\(.+\))*(?::).{4,}(?:#\d+)*(?<![\.\s])$";
private var msg = File.ReadAllLines(Args[0])[0];

if (System.Text.RegularExpressions.Regex.IsMatch(msg, pattern))
{
    Environment.Exit(0);
}

Console.WriteLine("❌ Your commit message is invalid! For more information visit: https://dev.azure.com/Nuuvify/_wiki/wikis/Nuuvify.wiki/1/Commit-Message-Format");
Console.WriteLine("✅ Tipos de Commits validos:");
Console.WriteLine("feat:     Um novo recurso");
Console.WriteLine("fix:      Uma correção de bug");
Console.WriteLine("docs:     Apenas alterações na documentação");
Console.WriteLine("style:    Alterações de formatação, sem mudança de código");
Console.WriteLine("refactor: Mudança de código que não corrige bug nem adiciona recurso");
Console.WriteLine("perf:     Melhorias de performance");
Console.WriteLine("test:     Adição ou correção de testes");
Console.WriteLine("chore:    Alterações em ferramentas ou processos");
Console.WriteLine("");
Console.WriteLine("📌 Exemplos de mensagens validas:");
Console.WriteLine("feat: adicionar módulo de autenticação");
Console.WriteLine("fix: resolver problema com login de usuário");
Console.WriteLine("docs: atualizar README com instruções de instalação");
Console.WriteLine("");
Console.WriteLine("✅ Formato recomendado para commits com breaking change:");
Console.WriteLine("feat!: remover suporte ao login legado");
Console.WriteLine("refactor!: reestruturação completa do módulo de autenticação");
Console.WriteLine("O ponto de exclamação (!) depois do tipo indica que há uma breaking change.");

Environment.Exit(1);
