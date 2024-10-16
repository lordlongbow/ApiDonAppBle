
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

protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

       
        modelBuilder.Entity<Publicacion>()
            .HasMany(p => p.Etiquetas)
            .WithMany(e => e.Publicaciones)
            .UsingEntity(j => j.ToTable("PublicacionEtiqueta"));
    }


}