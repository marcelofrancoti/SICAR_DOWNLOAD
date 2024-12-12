using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tesseract;

namespace DownloadSICAR.Util
{
    public static class Configuracao
    {
        public static string cookieSessaoPlay = "";
        public static int MaximoTentativas = 5; // Número máximo de tentativas
        public static long TamanhoMinimoArquivo = 1024 * 10; // Tamanho mínimo do arquivo válido em bytes (10 KB)
        public static string diretorioArquivosZip = "C:/temp/ZIP_FILES";
        public static void ConfigurarClienteHttp(HttpClient clienteHttp)
        {
            clienteHttp.DefaultRequestHeaders.Clear();
            clienteHttp.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36");
            clienteHttp.DefaultRequestHeaders.Add("Connection", "keep-alive");
        }

        public static async Task AtualizarCookieSessaoPlay(HttpClient clienteHttp)
        {
            string url = "https://consultapublica.car.gov.br/publico/municipios/ReCaptcha";

            var resposta = await clienteHttp.GetAsync(url);
            if (resposta.Headers.TryGetValues("Set-Cookie", out var cookies))
            {
                foreach (var cookie in cookies)
                {
                    if (cookie.Contains("PLAY_SESSION"))
                    {
                        cookieSessaoPlay = cookie.Split(';')[0];
                        Console.WriteLine($"Novo PLAY_SESSION capturado: {cookieSessaoPlay}");
                        clienteHttp.DefaultRequestHeaders.Remove("Cookie");
                        clienteHttp.DefaultRequestHeaders.Add("Cookie", cookieSessaoPlay);
                        break;
                    }
                }
            }

            if (string.IsNullOrEmpty(cookieSessaoPlay))
            {
                throw new Exception("Não foi possível atualizar o cookie PLAY_SESSION.");
            }
        }

        public static string ResolverCaptcha(string caminhoArquivo)
        {
            try
            {
                using var motor = new TesseractEngine(@"C:/Program Files/Tesseract-OCR/tessdata", "eng", EngineMode.Default);
                using var imagem = Pix.LoadFromFile(caminhoArquivo);
                using var pagina = motor.Process(imagem);
                return pagina.GetText().Trim();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao resolver CAPTCHA: {ex.Message}");
                throw;
            }
        }

        public static async Task BaixarArquivoEstadoAsync(string urlDownload, string caminhoArquivo, HttpClient clienteHttp)
        {
            try
            {
                var resposta = await clienteHttp.GetAsync(urlDownload);
                resposta.EnsureSuccessStatusCode();

                var bytesArquivo = await resposta.Content.ReadAsByteArrayAsync();

                string caminhoCompleto = Path.Combine(diretorioArquivosZip, caminhoArquivo);
                Directory.CreateDirectory(diretorioArquivosZip);

                if (File.Exists(caminhoCompleto))
                {
                    var infoArquivoExistente = new FileInfo(caminhoCompleto);

                    // Comparar somente a data sem horas
                    var dataArquivoBaixado = DateTime.Now.Date;
                    var dataArquivoExistente = infoArquivoExistente.LastWriteTime.Date;

                    if (dataArquivoExistente >= dataArquivoBaixado && infoArquivoExistente.Length >= bytesArquivo.Length)
                    {
                        Console.WriteLine($"O arquivo existente ({caminhoArquivo}) é mais recente ou maior. Download descartado.");
                        return;
                    }
                    else
                    {
                        Console.WriteLine($"Substituindo o arquivo {caminhoArquivo} porque o novo é mais recente ou maior.");
                    }
                }

                await File.WriteAllBytesAsync(caminhoCompleto, bytesArquivo);

                Console.WriteLine($"Arquivo baixado com sucesso: {caminhoCompleto}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao baixar o arquivo: {ex.Message}");
            }
        }

        public static async Task FluxoPrincipal(string urlCaptcha, string modeloUrlDownload, string[] estados)
        {
            while (true)
            {
                var estadosPendentes = estados.Where(estado => !File.Exists($"{Configuracao.diretorioArquivosZip}/{estado}_AREA_IMOVEL.zip"));

                if (!estadosPendentes.Any())
                {
                    Console.WriteLine("Todos os estados foram processados com sucesso.");
                    break;
                }

                foreach (var estado in estadosPendentes)
                {
                    Console.WriteLine($"Processando estado: {estado}");

                    bool sucesso = false;
                    int tentativa = 0;

                    while (!sucesso && tentativa < Configuracao.MaximoTentativas)
                    {
                        tentativa++;
                        Console.WriteLine($"Tentativa {tentativa} para o estado {estado}");

                        using var clienteHttp = new HttpClient();
                        Configuracao.ConfigurarClienteHttp(clienteHttp);

                        try
                        {
                            // Atualizar o cookie PLAY_SESSION
                            await Configuracao.AtualizarCookieSessaoPlay(clienteHttp);

                            // Baixar e resolver o CAPTCHA
                            Console.WriteLine("Baixando CAPTCHA...");
                            var imagemCaptcha = await clienteHttp.GetByteArrayAsync(urlCaptcha);

                            string caminhoArquivoCaptcha = $"C:/temp/IMG_CAPTCHA/{estado}_captcha.png";
                            Directory.CreateDirectory(Path.GetDirectoryName(caminhoArquivoCaptcha));
                            await File.WriteAllBytesAsync(caminhoArquivoCaptcha, imagemCaptcha);

                            Console.WriteLine("Resolvendo CAPTCHA...");
                            string textoCaptcha = Configuracao.ResolverCaptcha(caminhoArquivoCaptcha);

                            if (string.IsNullOrEmpty(textoCaptcha) || textoCaptcha.Length < 3)
                            {
                                throw new Exception("CAPTCHA resolvido é inválido ou muito curto.");
                            }

                            Console.WriteLine($"CAPTCHA resolvido: {textoCaptcha}");

                            // Montar a URL de download
                            string urlDownload = string.Format(modeloUrlDownload, estado, textoCaptcha);

                            // Tentar baixar o arquivo
                            string caminhoSaida = $"{Configuracao.diretorioArquivosZip}/{estado}_AREA_IMOVEL.zip";
                            Directory.CreateDirectory(Path.GetDirectoryName(caminhoSaida));

                            await Configuracao.BaixarArquivoEstadoAsync(urlDownload, caminhoSaida, clienteHttp);

                            // Verificar o arquivo baixado
                            FileInfo arquivoBaixado = new FileInfo(caminhoSaida);
                            if (arquivoBaixado.Length < Configuracao.TamanhoMinimoArquivo)
                            {
                                Console.WriteLine($"Arquivo corrompido ou muito pequeno. Excluindo: {caminhoSaida}");
                                arquivoBaixado.Delete();
                                throw new Exception("Arquivo baixado está vazio ou corrompido.");
                            }

                            Console.WriteLine($"Arquivo baixado com sucesso para o estado {estado}: {caminhoSaida}");
                            sucesso = true; // Marcar como sucesso
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Erro na tentativa {tentativa} para o estado {estado}: {ex.Message}");

                            if (tentativa == Configuracao.MaximoTentativas)
                            {
                                Console.WriteLine($"Falha ao processar o estado {estado} após {Configuracao.MaximoTentativas} tentativas.");
                            }
                        }
                    }
                }
            }
        }
    }
}