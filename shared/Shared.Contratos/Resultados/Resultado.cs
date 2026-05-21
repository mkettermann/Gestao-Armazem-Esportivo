namespace Shared.Contratos.Resultados;

public sealed class Resultado<T>
{
    public bool foiSucesso { get; }
    public T? valor { get; }
    public string? erro { get; }

    private Resultado(bool foiSucesso, T? valor, string? erro)
    {
        this.foiSucesso = foiSucesso;
        this.valor = valor;
        this.erro = erro;
    }

    public static Resultado<T> Sucesso(T valor) => new(true, valor, null);
    public static Resultado<T> Falha(string erro) => new(false, default, erro);
}
