﻿// <auto-generated />
using System;
using ApiDonAppBle.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace ApiDonAppBle.Migrations
{
    [DbContext(typeof(DataContext))]
    [Migration("20241202063728_InitialCreate")]
    partial class InitialCreate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("ApiDonAppBle.Models.Comentario", b =>
                {
                    b.Property<int>("IdComentario")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<DateTime>("Fecha")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("IdPublicacion")
                        .HasColumnType("int");

                    b.Property<int>("IdUsuario")
                        .HasColumnType("int");

                    b.Property<string>("Texto")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("IdComentario");

                    b.HasIndex("IdPublicacion");

                    b.ToTable("Comentario");
                });

            modelBuilder.Entity("ApiDonAppBle.Models.Etiqueta", b =>
                {
                    b.Property<int>("IdEtiqueta")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Descripcion")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("IdEtiqueta");

                    b.ToTable("Etiqueta");
                });

            modelBuilder.Entity("ApiDonAppBle.Models.Publicacion", b =>
                {
                    b.Property<int>("IdPublicacion")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<int>("Categoria")
                        .HasColumnType("int");

                    b.Property<bool>("Disponibilidad")
                        .HasColumnType("tinyint(1)");

                    b.Property<int>("Estado")
                        .HasColumnType("int");

                    b.Property<DateTime>("Fecha")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("FotoPublicacion")
                        .HasColumnType("longtext");

                    b.Property<int>("IdComenatario")
                        .HasColumnType("int");

                    b.Property<int>("IdEtiqueta")
                        .HasColumnType("int");

                    b.Property<int>("IdUsuario")
                        .HasColumnType("int");

                    b.Property<string>("Titulo")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("VideoPublicacion")
                        .HasColumnType("longtext");

                    b.HasKey("IdPublicacion");

                    b.HasIndex("IdUsuario");

                    b.ToTable("Publicacion");
                });

            modelBuilder.Entity("ApiDonAppBle.Models.Usuario", b =>
                {
                    b.Property<int>("IdUsuario")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Apellido")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Avatar")
                        .HasColumnType("longtext");

                    b.Property<string>("Direccion")
                        .HasColumnType("longtext");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Nombre")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("IdUsuario");

                    b.ToTable("Usuario");
                });

            modelBuilder.Entity("ApiDonAppBle.Models.Comentario", b =>
                {
                    b.HasOne("ApiDonAppBle.Models.Publicacion", "Publicacion")
                        .WithMany()
                        .HasForeignKey("IdPublicacion")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Publicacion");
                });

            modelBuilder.Entity("ApiDonAppBle.Models.Publicacion", b =>
                {
                    b.HasOne("ApiDonAppBle.Models.Usuario", "Usuario")
                        .WithMany()
                        .HasForeignKey("IdUsuario")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Usuario");
                });
#pragma warning restore 612, 618
        }
    }
}
