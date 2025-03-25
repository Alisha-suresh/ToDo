using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using ToDo.Models;

namespace ToDo.Services
{
    public class TodoService
    {
        private readonly string _filePath = "data.json";
        private List<TodoItem> _todos;

        public TodoService()
        {
            _todos = LoadData();
        }

        // Loads the todo data from a JSON file
        private List<TodoItem> LoadData()
        {
            try
            {
                if (!File.Exists(_filePath))
                {
                    return new List<TodoItem>();
                }

                var jsonData = File.ReadAllText(_filePath);
                return JsonSerializer.Deserialize<List<TodoItem>>(jsonData) ?? new List<TodoItem>();
            }
            catch (Exception)
            {
                return new List<TodoItem>();
            }
        }
        // Serializes the todo data to JSON and saves it to the file
        private void SaveData()
        {
            try
            {
                var jsonData = JsonSerializer.Serialize(_todos, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_filePath, jsonData);
            }
            catch (Exception)
            {
                throw new InvalidOperationException("Failed to save todo items.");
            }
        }
        // Retrieves all todo items with optional filtering and sorting
        public List<TodoItem> GetAll(
        string? completed = null,
        string? dueDate = null,
        string? sortBy = null,
        bool descending = false,
        string? titleFilter = null,
        string? userId = "",
        bool isAdmin = false)
        {
            // Filtering logic
            var filteredTodos = _todos.AsEnumerable();

            // If not an admin, force userId to the authenticated user's ID
            if (!isAdmin && !string.IsNullOrEmpty(userId))
            {
                filteredTodos = filteredTodos.Where(t => t.UserId == userId);
            }
            // If admin, allow filtering by any userId
            else if (isAdmin && !string.IsNullOrEmpty(userId))
            {
                filteredTodos = filteredTodos.Where(t => t.UserId == userId);
            }

            // Filter by completed status
            if (!string.IsNullOrEmpty(completed))
            {
                bool isCompleted = completed.ToLower() == "true";
                filteredTodos = filteredTodos.Where(t => t.Completed == isCompleted);
            }

            // Filter by due date
            if (!string.IsNullOrEmpty(dueDate) && DateTime.TryParse(dueDate, out DateTime due))
            {
                filteredTodos = filteredTodos.Where(t => t.DueDate.HasValue && t.DueDate.Value.Date == due.Date);
            }

            // Filter by title 
            if (!string.IsNullOrEmpty(titleFilter))
            {
                filteredTodos = filteredTodos.Where(t => t.Title != null && t.Title.Contains(titleFilter, StringComparison.OrdinalIgnoreCase));
            }

            // Sorting logic 
            if (!string.IsNullOrEmpty(sortBy))
            {
                switch (sortBy.ToLower())
                {
                    case "duedate":
                        filteredTodos = descending
                            ? filteredTodos.OrderByDescending(t => t.DueDate ?? DateTime.MaxValue)
                            : filteredTodos.OrderBy(t => t.DueDate ?? DateTime.MinValue);
                        break;

                    case "title":
                        filteredTodos = descending
                            ? filteredTodos.OrderByDescending(t => t.Title)
                            : filteredTodos.OrderBy(t => t.Title);
                        break;

                    case "creationdate":
                        filteredTodos = descending
                            ? filteredTodos.OrderByDescending(t => t.CreationDate)
                            : filteredTodos.OrderBy(t => t.CreationDate);
                        break;

                    case "completed":
                        filteredTodos = descending
                            ? filteredTodos.OrderByDescending(t => t.Completed)
                            : filteredTodos.OrderBy(t => t.Completed);
                        break;

                    default:
                        break;
                }
            }

            return filteredTodos.ToList();
        }

        // Retrieves a specific todo item by ID
        public TodoItem? GetById(int id, string userId)
        {
            return _todos.FirstOrDefault(t => t.Id == id && t.UserId == userId);
        }

        // Creates a new todo item
        public TodoItem Create(TodoItem item)
        {
            if (string.IsNullOrEmpty(item.UserId))
            {
                throw new ArgumentException("User ID is required when creating a todo item.");
            }

            item.Id = _todos.Any() ? _todos.Max(t => t.Id) + 1 : 1;
            item.CreationDate = DateTime.Now;
            _todos.Add(item);
            SaveData(); // Save the newly created todo to the file
            return item;
        }

        // Updates an existing todo item by ID
        public bool Update(int id, TodoItem updatedItem, string userId)
        {
            var item = _todos.FirstOrDefault(t => t.Id == id && t.UserId == userId);
            if (item == null) return false;

            item.Title = updatedItem.Title ?? item.Title;
            item.Completed = updatedItem.Completed;
            item.DueDate = updatedItem.DueDate;

            SaveData();
            return true;
        }

        // Marks a specific todo item as completed
        public TodoItem? MarkAsCompleted(int id, string userId)
        {
            var item = _todos.FirstOrDefault(t => t.Id == id && t.UserId == userId);
            if (item == null) return null;

            item.Completed = true;
            SaveData();
            return item;
        }

        // Admin function to delete a todo item by ID
        public bool DeleteForAdmin(int id)
        {
            var item = _todos.FirstOrDefault(t => t.Id == id);
            if (item == null) return false;

            _todos.Remove(item);
            SaveData();
            return true;
        }

        // Deletes a specific todo item for a user by ID
        public bool Delete(int id, string userId)
        {
            var item = _todos.FirstOrDefault(t => t.Id == id && t.UserId == userId);
            if (item == null) return false;

            _todos.Remove(item);
            SaveData();
            return true;
        }
    }
}
