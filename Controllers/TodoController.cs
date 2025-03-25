using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Collections.Generic;
using ToDo.Models;
using ToDo.Services;
using Microsoft.AspNetCore.Authorization;

namespace ToDo.Controllers
{
    [Authorize] 
    [ApiController]
    [Route("api/[controller]")]
    public class TodoController : ControllerBase
    {
        private readonly TodoService _todoService;

       
        public TodoController(TodoService todoService)
        {
            _todoService = todoService;
        }

        // Get all To-Do items with filtering and sorting
        [HttpGet]
        [Authorize(Roles = "User,Admin")] 
        public ActionResult<IEnumerable<TodoItem>> Get(
    [FromQuery] string? completed = null,
    [FromQuery] string? dueDate = null,
    [FromQuery] string? sortBy = null,
    [FromQuery] string? titleFilter = null,
    [FromQuery] string? userId = null,
    [FromQuery] bool descending = false)
        {
            try
            {
                string authenticatedUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
                bool isAdmin = User.IsInRole("Admin");

                
                if (string.IsNullOrEmpty(authenticatedUserId))
                {
                    return Unauthorized(new { message = "User identification failed." });
                }

               
                if (!isAdmin)
                {
                    userId = authenticatedUserId;
                }

                var todos = _todoService.GetAll(
                    completed: completed,
                    dueDate: dueDate,
                    sortBy: sortBy,
                    descending: descending,
                    titleFilter: titleFilter,
                    userId: userId,
                    isAdmin: isAdmin
                );

                if (todos == null || todos.Count == 0)
                {
                    return NotFound(new { message = "No to-do items found." });
                }

                return Ok(todos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the items.", error = ex.Message });
            }
        }

        // Get a specific To-Do item by ID
        [HttpGet("{id}")]
        [Authorize(Roles = "User,Admin")] 
        public IActionResult GetById(int id)
        {
            try
            {
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User identification failed." });
                }

                var item = _todoService.GetById(id, userId);

                if (item == null)
                {
                    return NotFound(new { message = "Item not found or you don't have access to this item." });
                }

                return Ok(item);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the item.", error = ex.Message });
            }
        }

        // Create a new To-Do item
        [HttpPost]
        [Authorize(Roles = "User,Admin")] 
        public IActionResult Create([FromBody] TodoItem item)
        {
            if (item == null || string.IsNullOrWhiteSpace(item.Title))
            {
                return BadRequest(new { message = "Title is required." });
            }

            try
            {
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

               
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User identification failed." });
                }

               
                item.UserId = userId;

                var newItem = _todoService.Create(item);
                return CreatedAtAction(nameof(GetById), new { id = newItem.Id }, newItem);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the item.", error = ex.Message });
            }
        }

        // Update an existing To-Do item by ID
        [HttpPut("{id}")]
        [Authorize(Roles = "User,Admin")] 
        public IActionResult Update(int id, [FromBody] TodoItem updatedItem)
        {
            if (updatedItem == null || string.IsNullOrWhiteSpace(updatedItem.Title))
            {
                return BadRequest(new { message = "Title is required." });
            }

            try
            {
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User identification failed." });
                }

                if (!_todoService.Update(id, updatedItem, userId))
                {
                    return NotFound(new { message = "Item not found or you don't have permission to edit it." });
                }

                var updated = _todoService.GetById(id, userId);
                return Ok(updated);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the item.", error = ex.Message });
            }
        }

        // Marking a To-Do item complete
        [HttpPut("{id}/complete")]
        [Authorize(Roles = "User,Admin")] 
        public IActionResult MarkAsCompleted(int id)
        {
            try
            {
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User identification failed." });
                }

                var item = _todoService.MarkAsCompleted(id, userId);

                if (item == null)
                {
                    return NotFound(new { message = "Item not found or you don't have permission to complete this item." });
                }

                return Ok(item);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while marking the item as complete.", error = ex.Message });
            }
        }

        //Search by User ID
        [HttpGet("admin/search")]
        [Authorize(Roles = "Admin")] // Only Admins can search
        public IActionResult AdminSearch([FromQuery] string userId)
        {
            try
            {
                // Validate userId is provided
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest(new { message = "User ID is required for search." });
                }

                var searchResults = _todoService.GetAll(
                    userId: userId,
                    isAdmin: true
                );

                if (searchResults == null || searchResults.Count == 0)
                {
                    return NotFound(new { message = "No items found for the specified user." });
                }

                return Ok(searchResults);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "An error occurred during the admin search.",
                    error = ex.Message
                });
            }
        }

            // Delete a To-Do item by ID
            [HttpDelete("{id}")]
        [Authorize(Roles = "User,Admin")] // Both User and Admin can delete their own todos
        public IActionResult Delete(int id)
        {
            try
            {
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
                bool isAdmin = User.IsInRole("Admin");

               
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User identification failed." });
                }

                bool deleteResult;
                if (isAdmin)
                {
                    // Admin can delete any task
                    deleteResult = _todoService.DeleteForAdmin(id);
                }
                else
                {
                    // Regular user can only delete their own tasks
                    deleteResult = _todoService.Delete(id, userId);
                }

                if (!deleteResult)
                {
                    return NotFound(new { message = "Item not found or you don't have permission to delete." });
                }

                return Ok(new { message = "Item deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the item.", error = ex.Message });
            }
        }
    }
}