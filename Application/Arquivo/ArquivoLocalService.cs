
namespace TribeWallet.Application.Arquivo;
using TribeWallet.Domain.Entities;

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

    public async Task<Arquivo?> GetArquivoByPath(string? caminhoArquivo)
    {
        var arquivo = await _arquivoRepository.GetByPath(caminhoArquivo);
        return arquivo;
    }
    public async Task<Arquivo?> SalvarArquivoLocalAsync(IFormFile? file)
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
        var arquivoArmazenado = new Arquivo
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

        string baseUrl = _config["STORAGE_BASE_URL"] ?? "http://localhost:5049/tribewallet";
        return $"{baseUrl.TrimEnd('/')}/{caminhoArmazenamento.TrimStart('/')}";
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
    
    public async Task DeletarArquivo(string? caminhoArmazenamento)
    {
        if (string.IsNullOrWhiteSpace(caminhoArmazenamento))
            return;

        string localPath = _config["STORAGE_LOCAL_PATH"] ?? Path.Combine(_env.ContentRootPath, "Uploads");
        string uploadsFolder = Path.GetFullPath(localPath);

        string filename = Path.GetFileName(caminhoArmazenamento);
        string caminhoFisico = Path.Combine(uploadsFolder, filename);

        if (File.Exists(caminhoFisico))
        {
            var arquivoMetadata = await GetArquivoByPath(caminhoArmazenamento);
            if (arquivoMetadata != null)
                await _arquivoRepository.Delete(arquivoMetadata);
            File.Delete(caminhoFisico);
        }
    }
}