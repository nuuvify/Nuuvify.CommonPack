# Plano: CNPJ Alfanumérico — Validação e Máscara

## Objetivo

Atualizar a classe `Cnpj` para suportar o novo CNPJ alfanumérico brasileiro (vigente a partir de julho/2026), mantendo **100% de compatibilidade** com CNPJs numéricos existentes.

O cálculo do dígito verificador continua usando **Módulo 11**, mas com conversão ASCII para caracteres alfanuméricos (`valor ASCII - 48`).
A máscara passa a usar inserção manual de pontuação no lugar de `Convert.ToUInt64`.

---

## Contexto — Nova Regra

- **Formato:** 14 posições — `[A-Z0-9]{12}[0-9]{2}` (12 alfanuméricos + 2 dígitos verificadores numéricos)
- **Cálculo DV:** Cada caractere é convertido: ASCII decimal - 48 → dígitos 0-9 mantêm valor, letras A=17, B=18... Z=42
- **Mesmos pesos** `{5,4,3,2,9,8,7,6,5,4,3,2}` e `{6,5,4,3,2,9,8,7,6,5,4,3,2}`
- **Compatibilidade total:** CNPJs numéricos existentes geram o mesmo DV com o novo algoritmo

---

## Fase 1 — Modificar `ValueObjects/CNPJ.cs`

### Step 1: Criar método de conversão de caractere

Criar `private static int CharParaValor(char c)` → retorna `c - 48` (zero allocation, inline-friendly)

### Step 2: Atualizar `ValidarCodigo()`

- Substituir `cnpj.GetNumbers()` por novo método que remove apenas pontuação (`.` `/` `-`) mas **preserva letras** e converte para uppercase
- Validar que 12 primeiros chars são `[A-Z0-9]` e últimos 2 são `[0-9]`
- Substituir `int.Parse(tempCnpj[i].ToString())` por `CharParaValor(tempCnpj[i])` (elimina alocação de string por caractere)
- Adicionar rejeição de CNPJs com todos caracteres iguais

### Step 3: Atualizar `Mascara()`

- Substituir `Convert.ToUInt64(Codigo).ToString(@"00\.000\.000\/0000\-00")` por inserção manual de pontuação
- Formato: `XX.XXX.XXX/XXXX-XX` → `$"{Codigo[0..2]}.{Codigo[2..5]}.{Codigo[5..8]}/{Codigo[8..12]}-{Codigo[12..14]}"`

### Step 4: Atualizar XML comments e `<example>` tags

---

## Fase 2 — Atualizar testes existentes

### Step 5: Atualizar `CnpjTestes.cs`

- **Manter todos InlineData numéricos** (compatibilidade retroativa)
- Adicionar InlineData com CNPJs alfanuméricos válidos (gerados pelo simulador da Receita)
- Testes para:
  - Alfanumérico válido
  - Com máscara
  - Com letras minúsculas
  - Caracteres inválidos
  - DV errado
  - `Mascara()` alfanumérico

### Step 6: Atualizar `DocumentoPessoaTests.cs`

- Adicionar teste com CNPJ alfanumérico

---

## Fase 3 — Criar Faker e Fixture

### Step 7: Criar `CnpjFaker.cs`

- Faker com Bogus
- Método `Generate()` para gerar um CNPJ alfanumérico válido
- Método `Generate(int count)` para gerar coleção

### Step 8: Criar `CnpjFixture.cs`

- Class fixture xUnit com shared context

---

## Arquivos Envolvidos

| Arquivo                                                                      | Ação                                        |
| ---------------------------------------------------------------------------- | ------------------------------------------- |
| `src/Nuuvify.CommonPack.Domain/ValueObjects/CNPJ.cs`                         | Modificar — validação e máscara             |
| `src/Nuuvify.CommonPack.Domain/ValueObjects/CPF.cs`                          | Referência — padrão de implementação        |
| `src/Nuuvify.CommonPack.Domain/ValueObjects/DocumentoPessoa.cs`              | Verificar — consumidor da classe            |
| `src/Nuuvify.CommonPack.Extensions/Implementation/StringExtensionMethods.cs` | Referência — `GetNumbers()` (não modificar) |
| `test/Nuuvify.CommonPack.Domain.xTest/ValueObjects/CnpjTestes.cs`            | Modificar — testes existentes               |
| `test/Nuuvify.CommonPack.Domain.xTest/ValueObjects/DocumentoPessoaTests.cs`  | Modificar — adicionar teste alfanumérico    |
| `test/Nuuvify.CommonPack.Domain.xTest/ValueObjects/CnpjFaker.cs`             | Criar — faker com Bogus                     |
| `test/Nuuvify.CommonPack.Domain.xTest/ValueObjects/CnpjFixture.cs`           | Criar — fixture xUnit                       |

---

## Decisões Técnicas

| Decisão                                                                                      | Justificativa                            |
| -------------------------------------------------------------------------------------------- | ---------------------------------------- |
| **Letras minúsculas:** aceitar na entrada, converter para uppercase via `ToUpperInvariant()` | Flexibilidade de entrada                 |
| **Letras I, O, Q, F:** aceitar                                                               | Receita apenas não recomenda, não proíbe |
| **Performance:** usar `char - 48` direto, sem `int.Parse()` nem Regex                        | Zero allocation por caractere            |
| **Máscara:** inserção manual por slicing                                                     | `Convert.ToUInt64` não suporta letras    |

---

## Fora de Escopo

- Alterar `GetNumbers()` extension method
- Alterar `DocumentoPessoaEstrangeiro.cs`
- Alterar `EmpresaFilial.cs`

---

## Critérios de Verificação

1. `dotnet build` no projeto Domain sem warnings
2. **Todos os testes existentes** continuam passando (CNPJ numérico `71266534000102` gera mesmo `Codigo` e `Mascara()`)
3. Novos testes alfanuméricos passam
4. `dotnet test` em `Nuuvify.CommonPack.Domain.xTest` — 100% green
