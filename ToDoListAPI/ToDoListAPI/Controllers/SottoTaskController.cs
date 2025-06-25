using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToDoListAPI.Data;
using ToDoListAPI.Models;

namespace ToDoListAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SottoTaskController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SottoTaskController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult Create([FromBody] SottoTaskDto dto)
        {
            var sql = "EXEC AggiungiSottoTask @p0, @p1, @p2";
            _context.Database.ExecuteSqlRaw(sql, dto.Titolo, dto.Stato, dto.TaskID);
            return Ok(new { message = "SottoTask aggiunta." });
        }

        [HttpGet("Task/{taskId}")]
        public IActionResult GetByTask(int taskId)
        {
            var sottoTasks = _context.SottoTask
                .FromSqlRaw("EXEC ElencoSottoTaskPerTask @p0", taskId)
                .ToList();

            return Ok(sottoTasks);
        }
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] SottoTaskDto dto)
        {
            var sql = "EXEC AggiornaSottoTask @p0, @p1, @p2, @p3";
            _context.Database.ExecuteSqlRaw(sql, id, dto.Titolo, dto.Stato, dto.TaskID);
            return Ok(new { message = "SottoTask aggiornata." });
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var sql = "EXEC EliminaSottoTask @p0";
            _context.Database.ExecuteSqlRaw(sql, id);
            return Ok(new { message = "SottoTask eliminata." });
        }
    }
}
