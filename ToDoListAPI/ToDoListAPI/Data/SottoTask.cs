    namespace ToDoListAPI.Data
{
    public class SottoTask
    {
        public int Id { get; set; }
        public string Titolo { get; set; }
        public bool Stato { get; set; }

        public int TaskID { get; set; }
        public Task? Task { get; set; }
    }
}
