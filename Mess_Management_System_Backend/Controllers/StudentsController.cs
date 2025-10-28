using Mess_Management_System_Backend.Data;
using Mess_Management_System_Backend.Models;
using Microsoft.AspNetCore.Mvc;

namespace Mess_Management_System_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public StudentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_context.Students.ToList());
        }

        [HttpPost]
        public IActionResult Add(Student student)
        {
            _context.Students.Add(student);
            _context.SaveChanges();
            return Ok(student);
        }
    }
}
