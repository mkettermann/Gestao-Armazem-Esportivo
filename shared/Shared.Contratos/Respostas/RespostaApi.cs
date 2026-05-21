namespace Shared.Contratos.Respostas;

public sealed class RespostaApi<T>
{
    public bool sucesso { get; init; }
    public string mensagem { get; init; } = string.Empty;
    public T? dados { get; init; }

    private RespostaApi() { }

    public static RespostaApi<T> Ok(T dados, string mensagem = "Operação realizada com sucesso.") =>
        new() { sucesso = true, mensagem = mensagem, dados = dados };

    public static RespostaApi<T> Erro(string mensagem) =>
        new() { sucesso = false, mensagem = mensagem, dados = default };
}

public sealed class RespostaApi
{
    public bool sucesso { get; init; }
    public string mensagem { get; init; } = string.Empty;

    private RespostaApi() { }

    public static RespostaApi Ok(string mensagem = "Operação realizada com sucesso.") =>
        new() { sucesso = true, mensagem = mensagem };

    public static RespostaApi Erro(string mensagem) =>
        new() { sucesso = false, mensagem = mensagem };
}
