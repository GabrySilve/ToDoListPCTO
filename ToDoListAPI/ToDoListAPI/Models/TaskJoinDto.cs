namespace ToDoListAPI.Models
{
    public class TaskJoinDto
    {
        public int Id { get; set; }
        public string Titolo { get; set; }
        public string Descrizione { get; set; }
        public DateTime? Scadenza { get; set; }
        public bool Stato { get; set; }
        public string? Categoria { get; set; }
        public string? Utente { get; set; }
    }
}
