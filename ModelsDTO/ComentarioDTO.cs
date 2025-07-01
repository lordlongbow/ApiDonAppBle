public class ComentarioDTO
{
    public int IdComentario { get; set; }
    public string Texto { get; set; }
    public DateTime Fecha { get; set; }
    public string NombreUsuario { get; set; }

    public int IdUsuario { get; set; }

}