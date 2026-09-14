# 📘 README - TribeWallet API

## 🚀 Pré-requisitos

Antes de começar, instale:

-   **.NET SDK 8.0**
    
-   **PostgreSQL** (versão 14 ou superior recomendada)
    
-   **Git** para clonar o repositório
    
-   Um editor/IDE de sua preferência, como **Rider**, **Visual Studio Community** ou **VS Code**
    

Este projeto é **cross-platform**: pode ser buildado e executado tanto em **Windows** quanto em **Linux**.

## 📂 Clonando o projeto

bash

```
git clone https://github.com/TribeWallet/Backend.git
```

ou, caso prefira via SSH
```
git clone git@github.com:TribeWallet/Backend.git
```

## ⚙️ Configuração do ambiente

1.  Copie o arquivo de exemplo:
    
    bash
    
    ```
    mv .env.example .env
	```
    
2.  Edite o `.env` com suas credenciais locais:
    
    -   Crie um **usuário e senha no PostgreSQL** e atualize as variáveis:
        
        env
        
          ```
        POSTGRES_USER=tribewallet
        POSTGRES_PASSWORD=sua-senha-aqui
        ```
        
    -   Ajuste o **host** e **porta** se necessário.
        
    -   Troque o **secret do JWT** por uma chave segura fornecida pela equipe:
        
        env
        
        ```
        Jwt__SecretKey=CHAVE_SUPER_SECRETA_FORNECIDA
        
        ```
        

> ⚠️ O `.env` não é versionado (está no `.gitignore`). Cada desenvolvedor deve manter sua própria configuração local.

## 🛠️ Buildando a aplicação

No terminal:

bash

```
dotnet build

```

## ▶️ Executando a API

bash

```
dotnet run

```

A API estará disponível em: 👉 `http://localhost:5049`

## 🧪 Testando a API

-   Acesse o **Swagger** em: 👉 `http://localhost:5049/swagger/index.html`
    
-   Ou utilize ferramentas como **Postman** ou **Insomnia** para enviar requisições.
    

## ✅ Banco de dados

-   Para aplicar as **migrations** manualmente:
    
    bash
    
    ```
    dotnet ef database update
    
    ```


## 📌 Dicas

-   Sempre atualize as dependências:
    
    bash
    
    ```
    dotnet restore
    
    ```
    
-   Verifique a versão instalada do SDK:
    
    bash
    
    ```
    dotnet --version
    
    ```

---

## 📖 Como funciona o código

### Controllers

-   Localizados em `/Presentation/FeatureController.cs`.
    
-   Recebem requisições HTTP (parâmetros na URL e/ou body).
    
-   Endpoints protegidos usam `[Authorize]`.
    
-   Se o body traz dados, utiliza-se um **RequestDTO**.
    
-   Dependências são injetadas via construtor (Dependency Injection).
    

### Services

-   Localizados em `/Application/Feature/FeatureService.cs`.
    
-   Recebem entidades puras dos repositórios e transformam em **ResponseDTOs**.
    
-   Podem depender de outros services ou repositórios.
    
-   Registrados no `Program.cs` com `builder.Services.AddScoped<...>()`.
    

### Repositórios

-   Interfaces em `/Application/Feature/IFeatureRepository.cs`.
    
-   Implementações em `/Infrastructure/FeatureRepository.cs`.
    
-   Fazem acesso ao banco via EF Core.
    
-   Retornam **entidades** (`Domain/Entities`).
    

### Domain

-   Entidades em `/Domain/Entities/Feature.cs`.
    
-   Representam tabelas do banco.
    
-   Nunca expostas diretamente ao cliente.
    

## 🔑 Registro de services e repositories no Program.cs

Todos os services e repositórios são acessíveis nas classes que os utilizam via Dependency Injectioon, mas para isso, é necessário registrá-los no Program.cs, da seguinte forma:

csharp

```
// Program.cs
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<UsuarioService>();

builder.Services.AddScoped<IGrupoRepository, GrupoRepository>();
builder.Services.AddScoped<GrupoService>();

```


## ⚡ Programação Assíncrona

-   Todas as chamadas ao banco de dados e serviços externos são **assíncronas**.
    
-   Isso significa que os métodos devem ser declarados como `async Task<TipoDeRetorno>`.
    
-   Ao chamar esses métodos, é obrigatório usar `await`.
    
-   Esse padrão se repete em **Controllers → Services → Repositories**.
    

### Exemplo no Controller

csharp

```
[HttpGet]
public async Task<IActionResult> GetAll(bool deleted = false)
{
    var usuarios = await _service.GetAll(deleted);
    return Ok(usuarios);
}

```

### Exemplo no Service

csharp

```
public async Task<IEnumerable<Usuario>> GetAll(bool deleted)
{
    return await _repository.GetAll(deleted);
}

```

### Exemplo no Repository

csharp

```
public async Task<IEnumerable<Usuario>> GetAll(bool deleted)
{
    if (!deleted)
        return await _dbContext.Usuarios
            .Where(u => u.DeletedAt == null)
            .ToListAsync();

    return await _dbContext.Usuarios.ToListAsync();
}
```


## 🧩 DTOs  de resposta aninhados

Os **ResponseDTOs** podem conter outros DTOs dentro deles. Isso garante que cada entidade seja representada de forma segura e organizada, sem expor IDs internos ou informações sensíveis.

### Exemplo de DTO aninhado

csharp

```
// Application/Grupo/DTOs/GrupoReponseDTO.cs
public class GrupoResponseDTO
{
    public string GrupoToken { get; set; }
    public string Nome { get; set; }
    public string? Descricao { get; set; }
    public ICollection<IntegranteResponseDTO> Integrantes { get; set; } = [];
    public ICollection<CompromissoFinanceiroResponseDTO> Compromissos { get; set; } = [];
}

// Application/Integrante/DTOs/IntegranteReponseDTO.cs
public class IntegranteResponseDTO
{
    public string IntegranteToken { get; set; }
    public UsuarioResponseDTO Usuario { get; set; }
    public string GrupoToken { get; set; }
    public ICollection<IntegranteCompromissoResponseDTO> Compromissos { get; set; } = [];
}

// Application/Usuario/DTOs/UsuarioReponseDTO.cs
public class UsuarioResponseDTO
{
    public string UsuarioToken { get; set; }
    public string Nome { get; set; }
    public string Sobrenome { get; set; }
    public string Email { get; set; }
    public string Username { get; set; }
}

```

> 🔎 Note que cada nível retorna apenas os dados necessários. IDs internos e senhas **nunca** devem ser expostos.


Quando o **Service** recebe uma entidade do repositório, ele trata os dados e retorna um **ResponseDTO**. Esse DTO pode ser **aninhado**, e o fluxo continua assíncrono:

csharp

```
public async Task<UsuarioResponseDTO> Update(EditUsuarioDTO editUsuarioDto, string usuarioToken)
{
    var usuario = await GetByToken(usuarioToken); // await usado aqui
    usuario.Nome = editUsuarioDto.Nome ?? usuario.Nome;

    var newUsuario = await _repository.Update(usuario); // await novamente

    return new UsuarioResponseDTO
    {
        UsuarioToken = newUsuario.Token,
        Nome = newUsuario.Nome,
        Sobrenome = newUsuario.Sobrenome,
        Email = newUsuario.Email,
        Username = newUsuario.Username
    };
}

```

> 🔑 **Regra de ouro:** se o método acessa banco, serviços externos ou depende de outro método assíncrono, ele deve ser `async Task<T>` e chamado com `await`.

## 📂 Fluxo da requisição com snippets

### Controller → Service

csharp

```
[Authorize]
[HttpPut("{usuarioToken}")]
public async Task<IActionResult> UpdateUsuario([FromBody] EditUsuarioDTO editUsuarioDto, string usuarioToken)
{
    var responseDto = await _service.Update(editUsuarioDto, usuarioToken);
    return Ok(responseDto);
}

```

### Service → Repository

csharp

```
public async Task<UsuarioResponseDTO> Update(EditUsuarioDTO editUsuarioDto, string usuarioToken)
{
    var usuario = await GetByToken(usuarioToken);
    usuario.Nome = editUsuarioDto.Nome ?? usuario.Nome;
    usuario.Sobrenome = editUsuarioDto.Sobrenome ?? usuario.Sobrenome;

    var newUsuario = await _repository.Update(usuario);

    return new UsuarioResponseDTO
    {
        UsuarioToken = newUsuario.Token,
        Nome = newUsuario.Nome,
        Sobrenome = newUsuario.Sobrenome,
        Email = newUsuario.Email,
        Username = newUsuario.Username
    };
}

```

### Repository → Banco

csharp

```
public async Task<IEnumerable<Usuario>> GetAll(bool deleted)
{
    if (!deleted)
        return await _dbContext.Usuarios.Where(u => u.DeletedAt == null).ToListAsync();

    return await _dbContext.Usuarios.ToListAsync();
}

```

## ✅ Resumindo

-   Controllers recebem requisições e retornam DTOs.
    
-   Services tratam entidades e convertem em ResponseDTOs.
    
-   Repositórios acessam o banco via EF Core.
    
-   DTOs aninhados organizam dados complexos sem expor informações sensíveis.
    
-   Tudo é registrado no `Program.cs` para funcionar com Dependency Injection.
