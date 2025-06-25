namespace ToDoListAPI.Models
{
    public class TaskCreateDto
    {
        public int Id { get; set; }
        public string Titolo { get; set; }
        public string? Descrizione { get; set; }
        public string? Scadenza { get; set; }
        public bool Stato { get; set; }
        public int? CategoriaID { get; set; }
        public int? UtenteID { get; set; }
    }
}
