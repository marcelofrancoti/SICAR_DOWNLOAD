using System;
using System.IO;
using System.Net.Http;
using System.Linq;
using System.Threading.Tasks;
using Tesseract;
using DownloadSICAR.Util;

class Programa
{
 
    static async Task Main(string[] args)
    {
        string urlCaptcha = "https://consultapublica.car.gov.br/publico/municipios/ReCaptcha";
        string modeloUrlDownload = "https://consultapublica.car.gov.br/publico/estados/downloadBase?idEstado={0}&tipoBase=AREA_IMOVEL&ReCaptcha={1}";
        string[] estados = {
                    "AC", "AL", "AP", "AM", "BA", "CE", "DF", "ES", "GO", "MA",
                    "MT", "MS", "MG", "PA", "PB", "PR", "PE", "PI", "RJ", "RN",
                    "RS", "RO", "RR", "SC", "SP", "SE", "TO"
            };

        try
        {
            await Configuracao.FluxoPrincipal(urlCaptcha, modeloUrlDownload, estados);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro inesperado: {ex.Message}");
        }
    }

 







}