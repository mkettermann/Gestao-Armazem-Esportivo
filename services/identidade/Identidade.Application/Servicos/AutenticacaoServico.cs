using BCrypt.Net;
using Identidade.Application.DTOs;
using Identidade.Domain.Factories;
using Identidade.Domain.Interfaces;
using Shared.Contratos.Resultados;

namespace Identidade.Application.Servicos;

public class AutenticacaoServico
{
    private readonly IUsuarioRepositorio _usuarioRepositorio;
    private readonly TokenServico _tokenServico;

    public AutenticacaoServico(IUsuarioRepositorio usuarioRepositorio,
                               TokenServico tokenServico)
    {
        _usuarioRepositorio = usuarioRepositorio;
        _tokenServico = tokenServico;
    }

    public async Task<Resultado<UsuarioRespostaDto>> cadastrarAsync(
        CadastrarUsuarioDto dto, CancellationToken ct = default)
    {
        var emailExistente = await _usuarioRepositorio.obterPorEmailAsync(dto.email, ct);
        if (emailExistente is not null)
            return Resultado<UsuarioRespostaDto>.Falha("O e-mail informado já está em uso.");

        Identidade.Domain.Entidades.Usuario usuario;
        try
        {
            usuario = UsuarioFactory.criar(dto.nome, dto.email, dto.senha, dto.tipoUsuario);
        }
        catch (Identidade.Domain.Excecoes.DomainException ex)
        {
            return Resultado<UsuarioRespostaDto>.Falha(ex.Message);
        }

        await _usuarioRepositorio.adicionarAsync(usuario, ct);
        await _usuarioRepositorio.salvarAlteracoesAsync(ct);

        return Resultado<UsuarioRespostaDto>.Sucesso(new UsuarioRespostaDto
        {
            id = usuario.id,
            nome = usuario.nome,
            email = usuario.email,
            tipoUsuario = usuario.tipoUsuario.ToString(),
            dataCadastro = usuario.dataCadastro
        });
    }

    public async Task<Resultado<TokenRespostaDto>> loginAsync(
        LoginDto dto, CancellationToken ct = default)
    {
        var usuario = await _usuarioRepositorio.obterPorEmailAsync(dto.email, ct);
        if (usuario is null || !BCrypt.Net.BCrypt.Verify(dto.senha, usuario.senhaHash))
            return Resultado<TokenRespostaDto>.Falha("Credenciais inválidas.");

        if (!usuario.ativo)
            return Resultado<TokenRespostaDto>.Falha(
                "Usuário inativo. Entre em contato com o administrador.");

        var tokenDto = _tokenServico.gerarToken(usuario);
        return Resultado<TokenRespostaDto>.Sucesso(tokenDto);
    }
}
