namespace ToDoListAPI.Data
{
    // Rappresenta una categoria nel sistema ToDoList
    public class Categorie
    {
        public int ID { get; set; }
        public string Descrizione { get; set; }

        // Collezione di task associati a questa categoria (relazione uno-a-molti)
        public ICollection<Task> Task { get; set; } 
    }
}
//ci sono dei semplici parametri con nessun controllo perché la classe Categorie è un'entità di database, non un modello di input con validazioni.