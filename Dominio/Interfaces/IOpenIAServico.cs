namespace Dominio.Interfaces;

public interface IOpenIAServico
{
    Task<List<string>> GerarPlaylist(string mensagemUsuario, int numeroDeMusicas = 20);
}