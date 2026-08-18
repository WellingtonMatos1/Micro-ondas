# Micro-ondas Digital

Sistema web que simula o funcionamento de um micro-ondas, com aquecimento manual, programas pré-definidos e cadastro de programas customizados.

## Tecnologias utilizadas

- C#
- .NET 9
- ASP.NET Core MVC
- Razor Views
- HTML
- CSS
- JavaScript
- JSON para persistência dos programas customizados

## Funcionalidades

- Interface inspirada em um painel de micro-ondas, com teclado numérico de 0 a 9.
- Aquecimento manual com tempo e potencia informados pelo usuário.
- Inicio rápido com 30 segundos e potencia padrão 10.
- Acréscimo de 30 segundos durante aquecimento manual.
- Pausa e cancelamento em um único botão.
- Exibição da string de aquecimento conforme a potencia.
- Conversão de segundos para formato de minutos e segundos, como `1:30`.
- Validação de tempo e potencia.
- Programas de aquecimento pré-definidos: Pipoca, Leite, Carnes de boi, Frango e Feijão.
- Cadastro de programas customizados com nome, alimento, tempo, potencia e caractere de aquecimento.
- Persistência dos programas customizados em arquivo JSON.
- Remoção de programas customizados.
- Separação entre camada de interface, controllers, models e service de negocio.

## Como instalar e executar

### Pré-requisitos

- .NET SDK 9.0 ou superior instalado.

### Passos

1. Clone o repositório:

```bash
git clone <url-do-repositorio>
```

2. Acesse a pasta do projeto:

```bash
cd Micro-ondas/micro-ondas
```

3. Restaure as dependências:

```bash
dotnet restore
```

4. Execute a aplicação:

```bash
dotnet run
```

5. Abra no navegador a URL exibida no terminal, por exemplo:

```text
http://localhost:5223
```

## Como usar

- Informe o tempo em segundos e, opcionalmente, a potencia.
- Caso a potencia não seja informada, o sistema usa potencia `10`.
- Clique em `Iniciar` sem preencher tempo e potencia para usar o inicio rápido de `30` segundos.
- Durante o aquecimento manual, clique novamente em `Iniciar` para acrescentar `30` segundos.
- Use `Pausar / Cancelar` uma vez para pausar e novamente para cancelar.
- Selecione um programa pré-definido para preencher automaticamente tempo e potencia.
- Abra `Novo programa` para cadastrar um programa customizado.
- Programas customizados aparecem em itálico e podem ser removidos pelo botão `x`.


## Observação sobre persistência

Os programas customizados são salvos em:

```text
micro-ondas/App_Data/programas-customizados.json
```

Caso o arquivo ainda não exista, ele será criado automaticamente quando um programa customizado for cadastrado.

> This is a challenge by [Coodesh](https://coodesh.com/)
