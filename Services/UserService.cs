using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ToDo.Models;

public class UserService
{
    private const string UserFilePath = "users.json";

    // Read all users from the JSON file
    public List<User> GetAllUsers()
    {
        if (!File.Exists(UserFilePath))
        {
            return new List<User>();
        }

        var jsonData = File.ReadAllText(UserFilePath);
        return JsonConvert.DeserializeObject<List<User>>(jsonData) ?? new List<User>();
    }

    // Find user by username
    public User GetUserByUsername(string username)
    {
        var users = GetAllUsers();
        return users.FirstOrDefault(u => u.Username == username);
    }

    // Update user refresh token
    public void UpdateUser(User user)
    {
        var users = GetAllUsers();
        var existingUser = users.FirstOrDefault(u => u.Username == user.Username);
        if (existingUser != null)
        {
            existingUser.RefreshToken = user.RefreshToken;
            File.WriteAllText(UserFilePath, JsonConvert.SerializeObject(users, Formatting.Indented));
        }
    }

    // Add a new user
    public void AddUser(User user)
    {
        var users = GetAllUsers();

        if (users.Any(u => u.Username == user.Username))
        {
            throw new System.Exception("User with this username already exists");
        }

        users.Add(user);
        File.WriteAllText(UserFilePath, JsonConvert.SerializeObject(users, Formatting.Indented));
    }

    // Create a new user with hashed password and role
    public void CreateUser(string username, string password, string role)
    {
        // Input Validation
        if (string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(role))
        {
            throw new ArgumentException("Invalid user creation parameters");
        }

        var users = GetAllUsers();

     
        if (users.Any(u => u.Username == username))
        {
            throw new InvalidOperationException("Username already exists");
        }

       
        var hashedPassword = PasswordHelper.HashPassword(password);

        var newUser = new User
        {
            Username = username,
            PasswordHash = hashedPassword,
            Role = role,
            RefreshToken = null
        };

        // Add new user to the list and save to file
        users.Add(newUser);
        File.WriteAllText(UserFilePath, JsonConvert.SerializeObject(users, Formatting.Indented));
    }
}
