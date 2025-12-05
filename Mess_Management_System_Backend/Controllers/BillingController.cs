using Mess_Management_System_Backend.Models;
using Mess_Management_System_Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mess_Management_System_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BillingController : ControllerBase
    {
        private readonly IBillingService _billingService;

        public BillingController(IBillingService billingService)
        {
            _billingService = billingService;
        }

        /// <summary>
        /// Generate a bill for a user for a specific period (Admin only)
        /// </summary>
        [HttpPost("generate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GenerateBill([FromBody] GenerateBillRequest request)
        {
            try
            {
                var bill = await _billingService.GenerateCurrentBillAsync(
                    request.UserId,
                    request.StartDate,
                    request.EndDate
                );
                return CreatedAtAction(nameof(GetBillById), new { id = bill.Id }, bill);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get bill by ID
        /// Users can view their own bills, Admin can view any
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetBillById(int id)
        {
            var bill = await _billingService.GetUserBillByIdAsync(id);
            if (bill == null)
                return NotFound(new { message = $"Bill with ID {id} not found" });

            return Ok(bill);
        }

        /// <summary>
        /// Get all bills for a specific user
        /// Users can view their own bills, Admin can view anyone's
        /// </summary>
        [HttpGet("user/{userId:int}")]
        public async Task<IActionResult> GetUserBills(int userId)
        {
            var bills = await _billingService.GetUserBillsAsync(userId);
            return Ok(bills);
        }

        /// <summary>
        /// Get all bills (Admin only)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllBills(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            var bills = await _billingService.GetAllBillsAsync(startDate, endDate);
            return Ok(bills);
        }

        /// <summary>
        /// Mark a bill as paid (Admin only)
        /// </summary>
        [HttpPut("{id:int}/pay")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> MarkBillAsPaid(int id)
        {
            var bill = await _billingService.MarkBillAsPaidAsync(id);
            if (bill == null)
                return NotFound(new { message = $"Bill with ID {id} not found" });

            return Ok(bill);
        }

        /// <summary>
        /// Delete a bill (Admin only)
        /// </summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteBill(int id)
        {
            var result = await _billingService.DeleteBillAsync(id);
            if (!result)
                return NotFound(new { message = $"Bill with ID {id} not found" });

            return NoContent();
        }
    }

    // Request models
    public class GenerateBillRequest
    {
        public int UserId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
