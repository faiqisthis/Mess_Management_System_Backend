namespace Mess_Management_System_Backend.Models
{
    public class Student
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string RoomNumber { get; set; } = "";
        public bool IsActive { get; set; }
    }
}
