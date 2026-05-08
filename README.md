# ArqMover

ArqMover e uma ferramenta local para copiar arquivos de uma pasta de origem para uma pasta de destino usando o `robocopy` do Windows. A interface roda no navegador e mostra progresso, arquivo atual, mensagens de status e log da execucao.

## Requisitos

- Windows 64 bits.
- `robocopy` disponivel no sistema, normalmente em `C:\Windows\System32\Robocopy.exe`.
- Para desenvolvimento: .NET SDK 9.

## Como Executar Em Desenvolvimento

Na pasta do projeto:

```powershell
dotnet run
```

O app abre automaticamente no navegador em:

```text
http://localhost:5000
```

Para iniciar sem abrir o navegador automaticamente:

```powershell
$env:OpenBrowser='false'
dotnet run
```

## Como Usar

1. Informe a pasta de origem.
2. Informe a pasta de destino.
3. Clique em `Iniciar Backup`.
4. Acompanhe o status, arquivo atual e log na tela.
5. Para fechar, clique em `Encerrar Aplicação` ou feche a aba/janela do navegador.

Ao fechar a pagina, o navegador envia uma chamada para `/encerrar`, e o servidor local tenta finalizar a aplicacao.

## Publicar Para Outra Maquina

Gere a publicacao:

```powershell
dotnet publish -c Release
```

A pasta publicada fica em:

```text
bin\Release\net9.0\win-x64\publish
```

Copie a pasta `publish` inteira para a outra maquina. Nao copie apenas o `ArqMover.exe`, porque o app tambem precisa dos arquivos estaticos em `wwwroot`, arquivos de configuracao e metadados gerados pela publicacao.

Depois de copiar, execute:

```text
ArqMover.exe
```

## Gerar Pacote ZIP

Opcionalmente, gere um zip da publicacao:

```powershell
Compress-Archive -Path bin\Release\net9.0\win-x64\publish\* -DestinationPath ArqMover-publish-win-x64.zip -Force
```

## Observacoes Importantes

- A aplicacao usa a porta `5000`. Se outro programa estiver usando essa porta, o ArqMover nao vai iniciar.
- A publicacao atual e para `win-x64`, entao a maquina de destino precisa ser Windows 64 bits.
- O backup usa `robocopy` com parametros para copiar subpastas, ignorar arquivos mais antigos e tolerar instabilidade de rede.
- Codigos de saida do `robocopy` menores que `8` sao tratados como sucesso. Codigos `8` ou maiores sao tratados como erro.
- A tela usa o cliente SignalR carregado via CDN. Se a maquina estiver sem internet, a interface pode abrir, mas a comunicacao em tempo real pode falhar.

## Comandos Do Robocopy

O app chama o `robocopy` com os seguintes argumentos:

```text
/E /XO /FFT /Z /R:3 /W:5 /MT:16 /NP
```

Resumo:

- `/E`: copia subdiretorios, incluindo vazios.
- `/XO`: ignora arquivos de origem mais antigos.
- `/FFT`: usa tolerancia de horario de arquivo, util para alguns sistemas de arquivos.
- `/Z`: modo reiniciavel.
- `/R:3`: tenta novamente ate 3 vezes em caso de falha.
- `/W:5`: espera 5 segundos entre tentativas.
- `/MT:16`: copia em ate 16 threads.
- `/NP`: nao exibe percentual individual de cada arquivo no log do `robocopy`.

## Estrutura Principal

```text
Program.cs                 Configuracao do app, SignalR, porta e encerramento
Pages/Index.cshtml         Interface principal
Pages/Index.cshtml.cs      PageModel da tela inicial
Pages/hubs/BackupHub.cs    Execucao do robocopy e envio de status para a tela
wwwroot/                   Arquivos estaticos da interface
```

## Icone Do Executavel

O icone do executavel esta configurado no projeto:

```xml
<ApplicationIcon>wwwroot\favicon.ico</ApplicationIcon>
```

Para trocar o icone, substitua `wwwroot\favicon.ico` por outro arquivo `.ico` e publique novamente.
