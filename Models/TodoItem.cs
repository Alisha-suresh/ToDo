using System;

namespace ToDo.Models
{
    public class TodoItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; }
        public bool Completed { get; set; }
        public DateTime CreationDate { get; set; }
        public string? UserId { get; set; }
    }
}
