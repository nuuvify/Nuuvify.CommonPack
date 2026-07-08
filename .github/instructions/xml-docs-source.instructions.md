---
description: "Use when creating, altering, or reviewing XML documentation for C# classes, interfaces, enums and methods in Nuuvify.CommonPack libraries. Covers required tags, consumer-oriented content, exceptions, remarks blocks, code examples, and DI registration docs."
name: "Nuuvify XML Docs Source"
applyTo: "src/**/*.cs"
---

# Diretrizes para documentação XML em C#

Todo tipo público (classe, interface, enum, record, struct) e todo membro público ou protegido
**devem** ter documentação XML completa no momento da criação ou da primeira alteração relevante.
Não adie a escrita da doc para um ciclo separado.

## Tags obrigatórias por elemento

### Tipos (`class`, `interface`, `enum`, `record`, `struct`)

```xml
/// <summary>Uma frase concisa descrevendo a responsabilidade do tipo.</summary>
/// <remarks>
/// Contexto de uso, como registrar no DI, dependências esperadas, limitações e relação
/// com outros tipos do pacote. Use <see cref="Tipo"/> para cruzar referências.
/// </remarks>
```

### Métodos e propriedades públicas

```xml
/// <summary>Frase de ação descrevendo o que o método faz.</summary>
/// <param name="param">O que o parâmetro representa e restrições de valor.</param>
/// <returns>O que é retornado e o significado de null/false/vazio.</returns>
/// <exception cref="ArgumentException">Quando ocorre e por quê.</exception>
/// <remarks>Comportamento adicional, efeitos colaterais ou exemplos quando necessário.</remarks>
```

### Enums

Cada membro do enum deve ter `<summary>` com o significado operacional do valor,
incluindo quando usar e implicações de escolha.

```xml
/// <summary>Descrição do que o valor representa e quando deve ser escolhido.</summary>
EnumMember = 1,
```

## Conteúdo orientado ao consumidor

- Escreva como se o leitor fosse um desenvolvedor que vai usar o pacote, nunca o que implementou.
- Descreva **o quê** e **por quê**, não **como** (o código já mostra o como).
- Explique pré-condições, pós-condições, efeitos colaterais e limites não evidentes.
- Para operações assíncronas, indique quando o `CancellationToken` é propagado e quando não é.
- Nunca repita a assinatura do método na `<summary>`; acrescente semântica real.

## Referências cruzadas e exemplos

- Use `<see cref="Tipo"/>` e `<see cref="Tipo.Membro"/>` para cruzar interfaces, implementações e opções relacionadas.
- Use `<see langword="true"/>`, `<see langword="false"/>`, `<see langword="null"/>` para literais.
- Inclua `<code>` dentro de `<remarks>` apenas quando o exemplo elimina dúvida real de uso,
  especialmente em extensões de DI e em tipos com comportamento surpreendente.

```xml
/// <remarks>
/// Exemplo de registro no DI:
/// <code>
/// services.AddMftMailboxCore();
/// services.AddMftMailboxSftp(sftp =>
/// {
///     sftp.Host = "sftp.parceiro.com";
///     sftp.Username = "usuario";
/// });
/// </code>
/// </remarks>
```

## Implementações de interface e herança

- Quando um membro implementa `<inheritdoc />`, use a tag simples sem repetir o conteúdo da interface.
- Adicione `<remarks>` com `<inheritdoc />` apenas para detalhar comportamento específico da implementação
  que não consta na interface (ex.: qual endpoint HTTP é chamado, qual modo SFTP é aplicado).

```xml
/// <inheritdoc />
/// <remarks>
/// Comportamento específico desta implementação que difere ou enriquece a interface.
/// </remarks>
public override Task<Result> MetodoAsync(...)
```

## Extensões de DI (`*Setup.cs`)

Classes `static` de setup devem documentar:
- **Classe**: quais serviços registra, com qual lifetime e pré-requisitos de chamada.
- **Método de extensão**: parâmetros de configuração, o que é registrado, lifetime de cada serviço
  e um exemplo de uso completo em `<remarks><code>`.

## Opções de configuração (`*Options.cs`)

Cada propriedade deve informar:
- Significado operacional (não apenas o nome).
- Valor padrão explicitado em texto (além do valor no código).
- Restrições: faixas válidas, dependências de outras propriedades, impacto de segurança.
- Quando aplicável, indicar se deve vir de variável de ambiente ou segredo gerenciado
  (não armazenar credenciais em texto claro).

## Regras de qualidade

- **Proibido**: `<summary>Gets or sets the X.</summary>` — não acrescenta informação.
- **Proibido**: repetir o tipo ou nome do parâmetro na descrição sem agregar contexto.
- **Obrigatório**: documentar `<exception>` para toda exceção lançada diretamente no corpo do método
  (não inclua exceções lançadas profundamente por dependências).
- **Obrigatório**: documentar o significado de `null` no retorno quando o método for nullable
  (`<returns><see langword="null"/> quando não encontrado.</returns>`).
- **Obrigatório**: quando `bool` retornado representa estado distinto, descrever ambos os casos.

## Padrão para pacotes `*.Abstraction`

Interfaces de abstração são a superfície pública mais crítica. Devem ter:
- `<summary>` com a responsabilidade do contrato.
- `<remarks>` explicando o papel na arquitetura, as implementações disponíveis e quando substituir.
- Cada método com `<param>`, `<returns>` e `<exception>` completos.
- Referências via `<see cref="..."/>` para implementações concretas e tipos relacionados.

## Padrão para utilitários estáticos

Classes `static` utilitárias (`Calculator`, `Builder`, `Executor`) devem documentar:
- Classe: propósito, quando usar e quem chama internamente.
- Cada método: comportamento de stream seek, thread safety, efeitos no estado do parâmetro
  e condições de erro esperadas.

## Idioma

- Documentação em **português do Brasil**, exceto nomes de tipos, membros, namespaces e termos
  técnicos sem tradução estabelecida (ex.: `CancellationToken`, `Stream`, `Bearer token`).
- Prefira termos do domínio do pacote de forma consistente entre classes relacionadas.
