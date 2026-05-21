using Identidade.Domain.Enums;
using Identidade.Domain.Excecoes;
using Identidade.Domain.ValueObjects;

namespace Identidade.Domain.Entidades;

public class Usuario
{
    public Guid id { get; private set; }
    public string nome { get; private set; } = string.Empty;
    public Email email { get; private set; } = null!;
    public string senhaHash { get; private set; } = string.Empty;
    public TipoUsuario tipoUsuario { get; private set; }
    public DateTime dataCadastro { get; private set; }
    public bool ativo { get; private set; }

    private Usuario() { }

    internal Usuario(Guid id, string nome, Email email,
                     string senhaHash, TipoUsuario tipoUsuario)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("O nome do usuário é obrigatório.");

        this.id = id;
        this.nome = nome.Trim();
        this.email = email;
        this.senhaHash = senhaHash;
        this.tipoUsuario = tipoUsuario;
        this.dataCadastro = DateTime.UtcNow;
        this.ativo = true;
    }

    public static Usuario criar(Guid id, string nome, Email email,
                                string senhaHash, TipoUsuario tipoUsuario) =>
        new(id, nome, email, senhaHash, tipoUsuario);

    public void desativar()
    {
        if (!ativo)
            throw new DomainException("O usuário já está inativo.");
        ativo = false;
    }

    public void reativar()
    {
        if (ativo)
            throw new DomainException("O usuário já está ativo.");
        ativo = true;
    }

    public void trocarSenha(string novaSenhaHash)
    {
        if (string.IsNullOrWhiteSpace(novaSenhaHash))
            throw new DomainException("O hash da nova senha não pode ser vazio.");
        senhaHash = novaSenhaHash;
    }
}
