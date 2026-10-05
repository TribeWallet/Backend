using Microsoft.EntityFrameworkCore;
using TribeWallet.Application.Arquivo;
using TribeWallet.Data;
using TribeWallet.Domain.Entities;

namespace TribeWallet.Infrastructure;

public class ArquivoRepository : IArquivoRepository
{
    private readonly AppDbContext _dbContext;

    public ArquivoRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Arquivo?> GetByPath(string path, bool deleted = false)
    {
        var arquivo = _dbContext.Arquivos
            .Where(a => a.Path == path);

        if (!deleted)
            arquivo
                .Where(a => a.DeletedAt == null);

        return await arquivo.FirstOrDefaultAsync();
    }

    public async Task<Arquivo> Create(Arquivo arquivo)
    {
        _dbContext.Arquivos.Add(arquivo);
        
        await _dbContext.SaveChangesAsync();
        return arquivo;
    }

    public async Task<Arquivo?> Update(Arquivo arquivo)
    {
        _dbContext.Arquivos.Update(arquivo);
        
        await _dbContext.SaveChangesAsync();
        return arquivo;
    }

    public async Task Delete(Arquivo arquivo)
    {
        if (arquivo.DeletedAt == null)
        {
            arquivo.DeletedAt = DateTime.UtcNow;
            _dbContext.Arquivos.Update(arquivo);
            
            await  _dbContext.SaveChangesAsync();
        }
    }
}