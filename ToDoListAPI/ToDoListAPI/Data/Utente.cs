namespace ToDoListAPI.Data
{
    public class Utente
    {
        public int ID { get; set; }
            public string Nome { get; set; } 

        public ICollection<Task> Task { get; set; }
    }
}
