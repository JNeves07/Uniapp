namespace UniApp.Api.Domain.Services;

/// <summary>
/// Servico de dominio para armazenamento seguro de senhas (contrato; implementacao na Infrastructure).
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Gera um hash PBKDF2 com salt aleatorio.
    /// </summary>
    string Hash(string password);

    /// <summary>
    /// Verifica o hash. Tambem aceita senhas em claro gravadas antes da migracao de seguranca,
    /// para nao quebrar contas ja existentes.
    /// </summary>
    bool Verify(string stored, string password);
}
