namespace CozinhaFreela.Infrastructure.Email
{
    public enum ResultadoConfirmacaoEmail
    {
        Sucesso,
        CodigoInvalido,
        CodigoExpirado,
        LimiteTentativasExcedido,
        CodigoNaoEncontrado
    }
}