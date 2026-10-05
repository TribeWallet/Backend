namespace TribeWallet.Application.Arquivo;
using TribeWallet.Domain.Entities;
public interface IArquivoRepository
{
    public Task<Arquivo?> GetByToken(string token, bool deleted = false);
    public Task<Arquivo> Create(Arquivo arquivo);
}