using System.Text.RegularExpressions;
using Pedidos.Domain.Excecoes;

namespace Pedidos.Domain.ValueObjects;

/// <summary>
/// Value object que encapsula e valida um documento de cliente (CPF ou CNPJ).
/// Garante que apenas documentos com dígitos válidos trafeguem pelo domínio.
/// </summary>
public sealed record DocumentoCliente
{
    public string valor { get; }

    private DocumentoCliente(string valor) => this.valor = valor;

    public static DocumentoCliente criar(string documento)
    {
        if (string.IsNullOrWhiteSpace(documento))
            throw new DomainException("O documento do cliente é obrigatório.");

        var digitos = Regex.Replace(documento.Trim(), @"\D", "");

        if (digitos.Length != 11 && digitos.Length != 14)
            throw new DomainException(
                "O documento deve ser um CPF (11 dígitos) ou CNPJ (14 dígitos) válido.");

        return new DocumentoCliente(documento.Trim());
    }

    public static implicit operator string(DocumentoCliente doc) => doc.valor;
    public override string ToString() => valor;
}
