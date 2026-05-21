namespace Estoque.Domain.Excecoes;

public sealed class DomainException : Exception
{
    public DomainException(string mensagem) : base(mensagem) { }
}
