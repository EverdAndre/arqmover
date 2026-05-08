using Microsoft.AspNetCore.SignalR;
using System.Diagnostics;
using System.Text;

namespace ArqMover.Hubs;

public class BackupHub : Hub
{
    public async Task IniciarBackup(string origem, string destino)
    {
        origem = origem?.Trim() ?? string.Empty;
        destino = destino?.Trim() ?? string.Empty;

        await Clients.Caller.SendAsync("AtualizarStatus", new
        {
            arquivo = "-",
            progresso = 0,
            status = "Iniciando backup...",
            concluido = false
        });

        try
        {
            if (string.IsNullOrWhiteSpace(origem) || string.IsNullOrWhiteSpace(destino))
            {
                await EnviarFimComErro("Informe a pasta de origem e a pasta de destino.");
                return;
            }

            if (!Directory.Exists(origem))
            {
                await EnviarFimComErro("A pasta de origem não existe.");
                return;
            }

            var processo = new Process();

            processo.StartInfo.FileName = "robocopy";
            processo.StartInfo.UseShellExecute = false;
            processo.StartInfo.RedirectStandardOutput = true;
            processo.StartInfo.RedirectStandardError = true;
            processo.StartInfo.CreateNoWindow = true;
            processo.StartInfo.StandardOutputEncoding =
            Encoding.GetEncoding(850);

            processo.StartInfo.StandardErrorEncoding =
            Encoding.GetEncoding(850);

            processo.StartInfo.ArgumentList.Add(origem);
            processo.StartInfo.ArgumentList.Add(destino);
            processo.StartInfo.ArgumentList.Add("/E");
            processo.StartInfo.ArgumentList.Add("/XO");
            processo.StartInfo.ArgumentList.Add("/FFT");
            processo.StartInfo.ArgumentList.Add("/Z");
            processo.StartInfo.ArgumentList.Add("/R:3");
            processo.StartInfo.ArgumentList.Add("/W:5");
            processo.StartInfo.ArgumentList.Add("/MT:16");
            processo.StartInfo.ArgumentList.Add("/NP");
            processo.StartInfo.ArgumentList.Add("/XD");
            processo.StartInfo.ArgumentList.Add("$RECYCLE.BIN");
            processo.StartInfo.ArgumentList.Add("System Volume Information");

            processo.Start();

            int progresso = 5;

            while (!processo.StandardOutput.EndOfStream)
            {
                string linha = await processo.StandardOutput.ReadLineAsync() ?? "";

                if (!string.IsNullOrWhiteSpace(linha))
                {
                    progresso += 1;

                    if (progresso > 95)
                        progresso = 95;

                    string arquivoAtual = linha.Trim();

                    await Clients.Caller.SendAsync("AtualizarStatus", new
                    {
                        arquivo = arquivoAtual,
                        progresso = progresso,
                        status = "Copiando arquivos...",
                        concluido = false
                    });
                }
            }

            string erro = await processo.StandardError.ReadToEndAsync();

            await processo.WaitForExitAsync();

            if (processo.ExitCode >= 8)
            {
                var mensagem = string.IsNullOrWhiteSpace(erro)
                    ? $"Robocopy falhou com código {processo.ExitCode}."
                    : $"Robocopy falhou com código {processo.ExitCode}: {erro.Trim()}";

                await EnviarFimComErro(mensagem);
                return;
            }

            await Clients.Caller.SendAsync("AtualizarStatus", new
            {
                arquivo = "-",
                progresso = 100,
                status = "Concluído",
                concluido = true
            });
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("AtualizarStatus", new
            {
                arquivo = "-",
                progresso = 100,
                status = "Erro: " + ex.Message,
                concluido = true
            });
        }
    }

    private Task EnviarFimComErro(string mensagem)
    {
        return Clients.Caller.SendAsync("AtualizarStatus", new
        {
            arquivo = "-",
            progresso = 100,
            status = "Erro: " + mensagem,
            concluido = true
        });
    }
}
