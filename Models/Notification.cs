namespace GestionCotisations.Web.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime DateEnvoi { get; set; } = DateTime.Now;
        public bool EstLue { get; set; } = false;

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }
    }
}