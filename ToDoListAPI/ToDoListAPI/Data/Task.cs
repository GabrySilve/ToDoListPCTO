namespace ToDoListAPI.Data
{
    public class Task
    {
        public int Id { get; set; }
        public string Titolo { get; set; }
        public string? Descrizione { get; set; } 
        public DateTime? Scadenza { get; set; }
        public bool Stato { get; set; }

        public int? CategoriaID { get; set; }
        public Categorie? Categoria { get; set; }

        public int? UtenteID { get; set; }
        public Utente? Utente { get; set; } 
    }
}
