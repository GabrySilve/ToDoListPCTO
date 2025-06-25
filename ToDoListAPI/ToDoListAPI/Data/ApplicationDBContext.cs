using System.Runtime.ConstrainedExecution;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ToDoListAPI.Models;
namespace ToDoListAPI.Data
{
    public class ApplicationDbContext : DbContext //QUESTO METODO IN PRATICA DEFINISCE LE VARIE RELAZIONE TRA TASK,CATEGORIE E UTENTI,INDICA SUCCESSIVAMENTE CHE "TaskJoinDto" VIENE UTILIZZATO SOLO PER LETTURA.
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) //Classe ApplicationDbContext  rappresenta il contesto del database per l'applicazione ToDoListAPI.
            : base(options) { }

        public DbSet<Task> Task { get; set; } //TASK DA GESTIRE
        public DbSet<Categorie> Categorie { get; set; } //CATEGORIE A CUI APPARTENGONO LE TASK
        public DbSet<Utente> Utente { get; set; } //RAPPRESENTA GLI UTENTI DELL'APPLICAZIONE
        public DbSet<TaskJoinDto> TaskJoinDto { get; set; } //utilizzato per mappare i risultati di una stored procedure (senza chiave primaria).
        public DbSet<CountResultDto> CountResultDto { get; set; }
        public DbSet<SottoTask> SottoTask { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            // Configura la relazione tra Task e Categoria:
            // Ogni Task ha una Categoria (HasOne) e ogni Categoria può avere molti Task (WithMany)
            modelBuilder.Entity<Task>()
                .HasOne(t => t.Categoria) // Relazione tra Task e Categoria
                .WithMany(c => c.Task) // Relazione inversa: Categoria -> molti Task
                .HasForeignKey(t => t.CategoriaID); // Chiave esterna in Task: CategoriaID


            // Configura la relazione tra Task e Utente:
            // Ogni Task è assegnato a un Utente, e ogni Utente può avere molti Task
            modelBuilder.Entity<Task>()
                .HasOne(t => t.Utente)     // Relazione: Task -> un Utente
                .WithMany(u => u.Task)      // Relazione inversa: Utente -> molti Task
                .HasForeignKey(t => t.UtenteID);   // Chiave esterna in Task: UtenteID

            modelBuilder.Entity<CountResultDto>().HasNoKey();
            modelBuilder.Entity<TaskJoinDto>().HasNoKey(); // Mappa TaskJoinDto come entità senza chiave primaria, usata solo per leggere dati (es. stored procedure)
        }
    }
}
