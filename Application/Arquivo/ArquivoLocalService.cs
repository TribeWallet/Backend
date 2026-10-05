using TribeWallet.Domain.Entities;

namespace TribeWallet.Application.Arquivo;

public class ArquivoLocalService
{
    private readonly IConfiguration _config;
    private readonly IWebHostEnvironment _env;
    private readonly IArquivoRepository _arquivoRepository;

    public ArquivoLocalService(IConfiguration config, IWebHostEnvironment env, IArquivoRepository arquivoRepository)
    {
        _config = config;
        _env = env;
        _arquivoRepository = arquivoRepository;
    }

    public async Task<Domain.Entities.Arquivo?> SalvarArquivoLocalAsync(IFormFile? file)
    {
        if (file == null || file.Length == 0)
            return null;

        //busca o diretório de uploads
        string localPath = _config["STORAGE_LOCAL_PATH"] ?? Path.Combine(_env.ContentRootPath, "Uploads");
        string uploadsFolder = Path.GetFullPath(localPath);

        //criando o nome e caminho do arquivo
        string extensao = Path.GetExtension(file.FileName).ToLowerInvariant();
        string nomeUnico = $"{Guid.NewGuid()}{extensao}";
        string caminhoFisico = Path.Combine(uploadsFolder, nomeUnico);

        using (var stream = new FileStream(caminhoFisico, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // mapeamento do tipo de conteudo
        TipoConteudo tipoEnum = MapearTipoArquivo(file.ContentType, extensao);

        // criação da entity de arquivo
        var arquivoArmazenado = new Domain.Entities.Arquivo
        {
            Nome = file.FileName,
            Path = nomeUnico,
            Tipo = tipoEnum
        };
        
        await _arquivoRepository.Create(arquivoArmazenado);
        return arquivoArmazenado;
    }

    public string ObterUrlLocalCompleta(string? caminhoArmazenamento)
    {
        if (string.IsNullOrEmpty(caminhoArmazenamento))
            return string.Empty;

        string baseUrl = _config["STORAGE_BASE_URL"] ?? "http://localhost:9000/tribewallet";
        return $"{baseUrl.TrimEnd('/')}/{caminhoArmazenamento}";
    }

    private static TipoConteudo MapearTipoArquivo(string contentType, string extensao)
    {
        return contentType.ToLower() switch
        {
            "image/jpeg" or "image/jpg" => TipoConteudo.Jpeg,
            "image/png" => TipoConteudo.Png,
            "application/pdf" => TipoConteudo.Pdf,
            _ => extensao switch
            {
                ".jpg" or ".jpeg" => TipoConteudo.Jpeg,
                ".png" => TipoConteudo.Png,
                ".pdf" => TipoConteudo.Pdf,
                _ => throw new ArgumentException("Tipo de arquivo não suportado.")
            }
        };
    }
}