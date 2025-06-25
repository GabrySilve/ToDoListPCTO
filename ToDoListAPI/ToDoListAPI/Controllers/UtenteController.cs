using System.Runtime.ConstrainedExecution;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToDoListAPI.Data;
using ToDoListAPI.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;
namespace ToDoListAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UtenteController : ControllerBase //QUI GESTIAMO I GLI UTENTI        LA CARTELLA CONTROLLERS SI OCCUPA SI GESTIRE,CONTROLLARE...UTENTI,TASK E CATEGORIA ANDANDO A CREARE,RECUPERARE O ELIMINARE SECONDO I LORO ID
    {
        private readonly ApplicationDbContext _context;

        public UtenteController(ApplicationDbContext context)  //Utilizza ApplicationDbContext per accedere al database tramite Entity Framework Core,COME ANCHE NEL CASO DELLE TASK E CATEGORIE.
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetAll() //Recupera la lista di tutti gli utenti.
        {
            var utenti = _context.Utente.ToList();
            return Ok(utenti);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id) //Recupera un utente specifico tramite ID.
        {
            var utente = _context.Utente.Find(id);
            if (utente == null)
                return NotFound();

            return Ok(utente);
        }

        [HttpPost]
        public IActionResult Create([FromBody] UtenteDto dto) //Crea un nuovo utente con i dati forniti nel body della richiesta.
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var utente = new Data.Utente
            {
                Nome = dto.Nome,
            };

            _context.Utente.Add(utente);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetAll), new { id = utente.ID }, utente);
        }

        [HttpPut("{id}")]      //Aggiorna un utente esistente tramite ID.
        public IActionResult Update(int id, [FromBody] UtenteDto updated)
        {
            var utente = _context.Utente.Find(id);
            if (utente == null)
                return NotFound();

            utente.Nome = updated.Nome;
            _context.SaveChanges();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var utente = _context.Utente.Find(id);
            if (utente == null)
                return NotFound();

            // Trova tutte le task collegate all'utente
            var tasksConUtente = _context.Task
                .Where(t => t.UtenteID == id)
                .ToList();

            // Imposta UtenteID a null per ogni task
            foreach (var task in tasksConUtente)
            {
                task.UtenteID = null;
            }

            // Ora elimina l'utente
            _context.Utente.Remove(utente);
            _context.SaveChanges();

            return NoContent();
        }
    }
}
