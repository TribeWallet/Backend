using System.Diagnostics.CodeAnalysis;
using TribeWallet.Application;
using TribeWallet.Application.Grupo;
using TribeWallet.Application.Integrante;
using TribeWallet.Application.Usuario;
using TribeWallet.Domain.Entities;

namespace TribeWallet.Infrastructure;

public class IntegranteService
{
    private readonly IIntegranteRepository _integranteRepository;
    private readonly UsuarioService _usuarioService;
    private readonly IGrupoRepository _grupoRepository;

    public IntegranteService(IIntegranteRepository integranteRepository, UsuarioService usuarioService, IGrupoRepository grupoRepository)
    {
        _integranteRepository = integranteRepository;
        _usuarioService = usuarioService;
        _grupoRepository = grupoRepository;
    }

    public async Task<ICollection<IntegranteResponseDTO>> GetAllByGrupoToken(string grupoToken)
    {
        var integrantes = await _integranteRepository.GetAllByGrupoToken(grupoToken);
        var responseDto = new List<IntegranteResponseDTO>();
        foreach (var integrante in integrantes)
        {
            var integranteDto = ConvertIntegranteToDto(integrante, grupoToken);
            responseDto.Add(integranteDto);
        }
        
        return responseDto;
    }

    public async Task<Integrante> SetupIntegranteEntity(CreateIntegranteRequestDTO createIntegranteRequestDto, string grupoToken)
    {
        var usuario = await _usuarioService.GetByToken(createIntegranteRequestDto.UsuarioToken);
        var integrante = new Integrante
        {
            UsuarioId = usuario.UsuarioId,
            Usuario = usuario
        };

        return integrante;
    }
    public IntegranteResponseDTO ConvertIntegranteToDto(Integrante integrante, string grupoToken)
    {
        var i = integrante;
        var usuarioDto = ConvertUsuarioToDto(integrante.Usuario);
        var integranteDto = new IntegranteResponseDTO
        {
            IntegranteToken = integrante.Token,
            Usuario = usuarioDto,
            GrupoToken = grupoToken
            //TODO adicionar compromissos
        };
        
        return integranteDto;
    }

    private UsuarioResponseDTO ConvertUsuarioToDto(Usuario usuario)
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
}