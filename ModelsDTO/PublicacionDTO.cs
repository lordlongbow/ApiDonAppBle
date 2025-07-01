public class PublicacionDTO
{
    public int IdPublicacion { get; set; }
    public string Titulo { get; set; }
    public string? Descripcion { get; set; }
    public DateTime Fecha { get; set; }
    public string? FotoPublicacion { get; set; }
    public IFormFile? FotoPublicacionIFormFile { get; set; }
    public bool? Disponibilidad { get; set; }
    public string? Categoria { get; set; }
    public UsuarioDTO? Usuario { get; set; }
    public int IdUsuario { get; set; }
}