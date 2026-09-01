namespace TribeWallet.Application.Grupo;
using TribeWallet.Domain.Entities;
using TribeWallet.Application.Grupo.DTOs;

public class GrupoService
{
    private readonly IGrupoRepository _repository;

    public GrupoService(IGrupoRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<GrupoReturnDTO>> GetByUsuarioToken(string token)
    {
        var grupos = await _repository.GetByUsuarioToken(token);
        var returnDtoList = grupos.Select(g => new GrupoReturnDTO
        {
            Nome = g.Nome,
            Descricao =  g.Descricao,
        }).ToList();
        
        return returnDtoList;
    }
    
    public async Task<GrupoReturnDTO>
}