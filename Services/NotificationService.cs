using GestionCotisations.Web.Data;
using GestionCotisations.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace GestionCotisations.Web.Services
{
    public class NotificationService
    {
        private readonly ApplicationDbContext _context;

        public NotificationService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Crée une notification pour un utilisateur
        public async Task CreerNotificationAsync(string userId, string message)
        {
            var notification = new Notification
            {
                UserId = userId,
                Message = message,
                DateEnvoi = DateTime.Now,
                EstLue = false
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Notification>> GetNotificationsNonLuesAsync(string userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId && !n.EstLue)
                .OrderByDescending(n => n.DateEnvoi)
                .ToListAsync();
        }

        public async Task<List<Notification>> GetToutesNotificationsAsync(string userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.DateEnvoi)
                .ToListAsync();
        }

        // Marque une notification comme lue
        public async Task MarquerCommeLueAsync(int notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification != null)
            {
                notification.EstLue = true;
                await _context.SaveChangesAsync();
            }
        }

        // Marque  les notifications comme lues
        public async Task MarquerToutesCommeLuesAsync(string userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.EstLue)
                .ToListAsync();

            foreach (var n in notifications)
                n.EstLue = true;

            await _context.SaveChangesAsync();
        }

        // Vérifie les cotisations en retard et crée des notifications
        public async Task VerifierCotisationsEnRetardAsync()
        {
            var adminUsers = await _context.Users
                .Where(u => u.EstActif)
                .ToListAsync();

            var cotisationsEnRetard = await _context.Cotisations
                .Include(c => c.Membre)
                .Where(c => c.Statut == StatutCotisation.EnRetard)
                .ToListAsync();

            foreach (var cotisation in cotisationsEnRetard)
            {
                var joursRetard = (DateTime.Now - cotisation.DateEcheance).Days;
                var message = $"⚠️ Cotisation en retard : {cotisation.Membre.Prenom} " +
                              $"{cotisation.Membre.Nom} — {cotisation.Libelle} " +
                              $"({cotisation.Montant:N0} FCFA) — {joursRetard} jour(s) de retard";

                // Notifier chaque admin/gestionnaire
                foreach (var user in adminUsers)
                {
                    // Vérifier si une notification similaire existe déjà aujourd'hui
                    var dejaNotifie = await _context.Notifications
                        .AnyAsync(n => n.UserId == user.Id &&
                                       n.Message.Contains(cotisation.Libelle) &&
                                       n.DateEnvoi.Date == DateTime.Now.Date);

                    if (!dejaNotifie)
                        await CreerNotificationAsync(user.Id, message);
                }
            }
        }
    }
}