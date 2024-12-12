# Documentação do Projeto

Este projeto foi desenvolvido por **Marcelo Franco** e tem como objetivo automatizar o download de arquivos por estado, utilizando CAPTCHA resolvido via OCR (Tesseract).

## Requisitos

### Ferramentas Necessárias

1. **IDE**:

   - Recomendado: [Visual Studio 2022 Community Edition](https://visualstudio.microsoft.com/)
     - Workload: `.NET Desktop Development`.

2. **.NET SDK**:

   - Versão: [.NET 6 ou superior](https://dotnet.microsoft.com/download).

3. **Tesseract OCR**:

   - Instale o Tesseract OCR:
     - [Download para Windows](https://github.com/tesseract-ocr/tesseract).
     - Configure o caminho para `tessdata` no código.

4. **Git**:

   - [Download do Git](https://git-scm.com/).

5. **Gerenciador de Dependências NuGet**:

   - Certifique-se de que sua IDE possui suporte para restaurar pacotes NuGet automaticamente.

6. **Caminhos de Arquivos**:

   - Crie os diretórios a seguir no seu sistema:
     - `C:/temp/ZIP_FILES`
     - `C:/temp/IMG_CAPTCHA`

### Dependências NuGet

- [Tesseract](https://www.nuget.org/packages/Tesseract): Biblioteca para OCR.
- [System.Net.Http](https://www.nuget.org/packages/System.Net.Http): Cliente HTTP.

Execute o seguinte comando para instalar as dependências:

```bash
nuget restore
```

### Configurações Adicionais

1. **Cookie PLAY\_SESSION**:

   - O projeto automaticamente captura e atualiza o cookie necessário para as requisições.

2. **Lista de Estados**:

   - Certifique-se de que todos os estados brasileiros estão definidos no código:

```csharp
string[] estados = {
    "AC", "AL", "AP", "AM", "BA", "CE", "DF", "ES", "GO", "MA",
    "MT", "MS", "MG", "PA", "PB", "PR", "PE", "PI", "RJ", "RN",
    "RS", "RO", "RR", "SC", "SP", "SE", "TO"
};
```

## Como Executar o Projeto

1. Clone o repositório:

```bash
git clone <URL_DO_REPOSITORIO>
```

2. Abra o projeto na IDE recomendada.

3. Restaure as dependências NuGet.

4. Certifique-se de que os diretórios necessários estão configurados corretamente.

5. Compile e execute o programa.

## Contato

Caso encontre algum problema ou tenha sugestões, entre em contato pelo GitHub ou diretamente com o autor do projeto, **Marcelo Franco**.



