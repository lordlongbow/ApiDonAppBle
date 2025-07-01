
using Microsoft.EntityFrameworkCore;
using ApiDonAppBle.Models;

namespace ApiDonAppBle.Models;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options) { }

    public DbSet<Usuario> Usuario { get; set; }
    public DbSet<Publicacion> Publicacion { get; set; }
    public DbSet<Comentario> Comentario { get; set; }
    public DbSet<Etiqueta> Etiqueta { get; set; }
    public DbSet<MensajePrivado> MensajePrivado { get; set; }

protected override void OnModelCreating(ModelBuilder modelBuilder)
    {


    }


}