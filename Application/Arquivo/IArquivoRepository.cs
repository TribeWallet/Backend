namespace TribeWallet.Application.Arquivo;
using TribeWallet.Domain.Entities;
public interface IArquivoRepository
{
    public Task<Arquivo?> GetByPath(string path, bool deleted = false);
    public Task<Arquivo> Create(Arquivo arquivo);
    public Task<Arquivo?> Update(Arquivo arquivo);
    
    public Task Delete(Arquivo arquivo);
}