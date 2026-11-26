using UniApp.Api.Domain.Entities;
using UniApp.Api.Domain.Exceptions;
using UniApp.Api.Domain.Rules;
using UniApp.Api.Infrastructure.Security;
using Xunit;

namespace UniApp.Backend.Tests;

public class DomainTests
{
    [Theory]
    [InlineData("joao.marcelo@unifacema.edu.br")]
    [InlineData("ALUNO@UNIFACEMA.EDU.BR")]
    [InlineData("  aluno.teste@campus-ifma.edu.br  ")]
    public void EmailInstitucional_Valido(string email)
        => Assert.True(InstitutionalEmail.IsValid(email));

    [Theory]
    [InlineData("aluno@gmail.com")]
    [InlineData("aluno@unifacema.edu")]
    [InlineData("aluno@edu.br")]
    [InlineData("aluno@unifacema.edu.br.com")]
    [InlineData("aluno unifacema@unifacema.edu.br")]
    [InlineData("")]
    [InlineData(null)]
    public void EmailInstitucional_Invalido(string? email)
        => Assert.False(InstitutionalEmail.IsValid(email));

    [Fact]
    public void Normalizacao_RetiraEspacosEConverteMinusculas()
        => Assert.Equal("joao@unifacema.edu.br", InstitutionalEmail.Normalize(" Joao@Unifacema.edu.br "));

    [Fact]
    public void Doacao_SempreTemPrecoZero()
    {
        var material = new Material { Titulo = "Apostila", Modalidade = "Doação", Preco = 99m };
        MaterialRules.Apply(material);
        Assert.Equal(0, material.Preco);
    }

    [Fact]
    public void ModalidadeInvalida_LancaDomainException()
        => Assert.Throws<DomainException>(() => MaterialRules.Validate("Livro", "Leilão", 10m));

    [Fact]
    public void PrecoNegativo_LancaDomainException()
        => Assert.Throws<DomainException>(() => MaterialRules.Validate("Livro", "Venda", -1m));

    [Fact]
    public void ReservarVaga_DecrementaVagas()
    {
        var grupo = new GrupoEstudo { Nome = "ADS", Disciplina = "Algoritmos", VagasDisponiveis = 3 };
        GrupoRules.ReservarVaga(grupo);
        Assert.Equal(2, grupo.VagasDisponiveis);
    }

    [Fact]
    public void GrupoLotado_NaoPermiteInscricao()
    {
        var grupo = new GrupoEstudo { Nome = "ADS", Disciplina = "Algoritmos", VagasDisponiveis = 0 };
        var ex = Assert.Throws<DomainException>(() => GrupoRules.ReservarVaga(grupo));
        Assert.Equal("Grupo lotado.", ex.Message);
    }

    [Fact]
    public void FormatoInvalido_LancaDomainException()
        => Assert.Throws<DomainException>(() => GrupoRules.Validate("Grupo", "Disc", "Híbrido", 5));

    [Fact]
    public void PasswordHasher_GeraHashESenhaNaoApareceEmClaro()
    {
        var hasher = new PasswordHasher();
        var hash = hasher.Hash("senha123");

        Assert.StartsWith("$uniapp$pbkdf2-sha256$", hash);
        Assert.DoesNotContain("senha123", hash);
        Assert.True(hasher.Verify(hash, "senha123"));
        Assert.False(hasher.Verify(hash, "senhaErrada"));
    }

    [Fact]
    public void PasswordHasher_AceitaHashDiferenteParaMesmaSenha()
    {
        var hasher = new PasswordHasher();
        Assert.NotEqual(hasher.Hash("senha123"), hasher.Hash("senha123"));
    }

    [Fact]
    public void PasswordHasher_MantemCompatibilidadeComSenhaAntigaEmClaro()
    {
        var hasher = new PasswordHasher();
        Assert.True(hasher.Verify("123456", "123456"));
        Assert.False(hasher.Verify("123456", "654321"));
    }
}
