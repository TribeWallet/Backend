using TribeWallet.Application;
using TribeWallet.Application.CompromissoFinanceiro;
using TribeWallet.Application.Grupo;
using TribeWallet.Application.Grupo.DTOs;
using TribeWallet.Application.Integrante;
using TribeWallet.Application.Usuario;
using TribeWallet.Domain.Entities;
using TribeWallet.Infrastructure;

namespace TribeWallet.Services;

public class GrupoIntegranteCommonService
{
    private readonly IGrupoRepository _grupoRepository;
    private readonly IIntegranteRepository _integranteRepository;
    private readonly IUsuarioRepository _usuarioRepository;

    public GrupoIntegranteCommonService(IGrupoRepository grupoRepository, IIntegranteRepository integranteRepository, IUsuarioRepository usuarioRepository)
    {
        _grupoRepository = grupoRepository;
        _integranteRepository = integranteRepository;
        _usuarioRepository = usuarioRepository;
    }

    public async Task<GrupoResponseDTO> ConvertGrupoToResponseDto(Grupo grupo, bool deleted)
    {
        var integrantes = await _integranteRepository.GetAllByGrupoToken(grupo.Token, deleted);
        var integrantesDto  = new List<IntegranteResponseDTO>();

        foreach (var integrante in integrantes)
        {
            
            var integranteDto = ConvertIntegranteToResponseDto(integrante);
            integrantesDto.Add(integranteDto);
        }
        
        var grupoResponseDto = new GrupoResponseDTO
        {
            GrupoToken = grupo.Token,
            Nome = grupo.Nome,
            Descricao = grupo.Descricao,
            Integrantes = integrantesDto
        };
        
        return grupoResponseDto;
    }
    
    public IntegranteResponseDTO ConvertIntegranteToResponseDto(Integrante integrante)
    {
        var usuarioDto = ConvertUsuarioToResponseDto(integrante.Usuario);
        var integranteDto = new IntegranteResponseDTO
        {
            IntegranteToken = integrante.Token,
            Usuario = usuarioDto,
            GrupoToken = integrante.Grupo.Token,
            DeletedAt =  integrante.DeletedAt,
            //TODO adicionar compromissos
        };
        
        return integranteDto;
    }

    private UsuarioResponseDTO ConvertUsuarioToResponseDto(Usuario usuario)
    {
        var usuarioDto = new UsuarioResponseDTO
        {
            UsuarioToken = usuario.Token,
            Nome = usuario.Nome,
            Sobrenome = usuario.Sobrenome,
            Email = usuario.Email,
            Username = usuario.Username
        };
        
        return usuarioDto;
    }
    
    public async Task<Integrante> SetupIntegranteEntity(CreateIntegranteRequestDTO createIntegranteRequestDto,
        string grupoToken)
    {
        var usuario = await _usuarioRepository.GetByToken(createIntegranteRequestDto.UsuarioToken);
        var integrante = new Integrante
        {
            UsuarioId = usuario.UsuarioId,
            Usuario = usuario
        };

        return integrante;
    }
}