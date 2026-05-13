using GestionCotisations.Web.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GestionCotisations.Web.Data
{
    // On hérite de IdentityDbContext pour avoir les tables Identity automatiquement
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Nos tables dans la base de données
        public DbSet<Membre> Membres { get; set; }
        public DbSet<Cotisation> Cotisations { get; set; }
        public DbSet<Paiement> Paiements { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configuration de la table Membre
            builder.Entity<Membre>(entity =>
            {
                entity.Property(m => m.Nom).IsRequired().HasMaxLength(100);
                entity.Property(m => m.Prenom).IsRequired().HasMaxLength(100);
                entity.Property(m => m.Email).IsRequired().HasMaxLength(200);
            });

            // Configuration de la table Cotisation
            builder.Entity<Cotisation>(entity =>
            {
                entity.Property(c => c.Montant).HasColumnType("decimal(18,2)");
                entity.Property(c => c.Libelle).IsRequired().HasMaxLength(200);

                entity.HasOne(c => c.Membre)
                      .WithMany(m => m.Cotisations)
                      .HasForeignKey(c => c.MembreId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configuration de la table Paiement
            builder.Entity<Paiement>(entity =>
            {
                entity.Property(p => p.Montant).HasColumnType("decimal(18,2)");

                entity.HasOne(p => p.Cotisation)
                      .WithMany(c => c.Paiements)
                      .HasForeignKey(p => p.CotisationId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configuration de la table Notification
            builder.Entity<Notification>(entity =>
            {
                entity.Property(n => n.Message).IsRequired().HasMaxLength(500);

                entity.HasOne(n => n.User)
                      .WithMany()
                      .HasForeignKey(n => n.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}