using TribeWallet.Application.Integrante;

namespace TribeWallet.Application.Grupo;
using TribeWallet.Domain.Entities;
using TribeWallet.Application.Grupo.DTOs;

public class GrupoService
{
    private readonly IGrupoRepository _grupoRepository;
    private readonly IIntegranteRepository _integranteRepository;

    public GrupoService(IGrupoRepository grupoRepository, IIntegranteRepository integranteRepository)
    {
        _grupoRepository = grupoRepository;
        _integranteRepository = integranteRepository;
    }

    public async Task<List<GrupoReturnDTO>> GetByUsuarioToken(string token)
    {
        var grupos = await _grupoRepository.GetByUsuarioToken(token);
        var returnDtoList = new List<GrupoReturnDTO>();
        foreach (var grupo in grupos)
        {
            foreach (var integrante in grupo.Integrantes)
            {
                Console.WriteLine(integrante.Usuario.Nome);
            }
            var integrantes = await _integranteRepository.GetAllByGrupoToken(grupo.Token);
            var integrantesDto = ConvertIntegrantes(integrantes, grupo.Token);

            var grupoDto = new GrupoReturnDTO
            {
                GrupoToken =  grupo.Token,
                Nome = grupo.Nome,
                Descricao = grupo.Descricao,
                Integrantes = integrantesDto
            };
            returnDtoList.Add(grupoDto);
        }
        
        return returnDtoList;
    }

    public async Task<GrupoReturnDTO?> Create(GrupoCreateDTO grupoCreateDto)
    {
        var grupo = new Grupo
        {
            Nome = grupoCreateDto.Nome,
            Descricao = grupoCreateDto.Descricao
        };
        
        grupo = await _grupoRepository.Create(grupo);
        var integrantes = await _integranteRepository.GetAllByGrupoToken(grupo.Token);
        var integrantesDto = ConvertIntegrantes(integrantes, grupo.Token);
        var returnDto = new GrupoReturnDTO
        {
            GrupoToken = grupo.Token,
            Nome = grupo.Nome,
            Descricao = grupo.Descricao,
            Integrantes =  integrantesDto
            //TODO adicionar compromissos
        };
        return returnDto;
    }

    private ICollection<IntegranteReturnDTO> ConvertIntegrantes(ICollection<Integrante> integrantes, string grupoToken)
    {
        var returnDtos = new List<IntegranteReturnDTO>();

        foreach (var integrante  in integrantes)
        {
            var usuarioDto = new UsuarioReturnDTO
            {
                UsuarioToken = integrante.Usuario.Token,
                Nome = integrante.Usuario.Nome,
                Sobrenome = integrante.Usuario.Sobrenome,
                Email = integrante.Usuario.Email,
                Username = integrante.Usuario.Username
            };
            var integranteDto = new IntegranteReturnDTO
            {
                IntegranteToken = integrante.Token,
                Usuario = usuarioDto,
                GrupoToken = grupoToken
                //TODO adicionar compromissos
            };
            
            returnDtos.Add(integranteDto);
        }

        return returnDtos;
    }
}