using LibraryBlazorApp.Models;
using LibraryBlazorApp.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;

namespace LibraryBlazorApp.Tests;

public class LibraryServiceTests : IDisposable
{
    private readonly string tempRoot;
    private readonly LibraryService service;

    public LibraryServiceTests()
    {
        tempRoot = Path.Combine(Path.GetTempPath(), $"Lab5Tests_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempRoot);

        string dataFolder = Path.Combine(tempRoot, "Data");
        Directory.CreateDirectory(dataFolder);

        File.WriteAllLines(Path.Combine(dataFolder, "Books.csv"),
        [
            "Id,Title,Author,ISBN",
            "1,1984,George Orwell,111",
            "2,Dune,Frank Herbert,222"
        ]);

        File.WriteAllLines(Path.Combine(dataFolder, "Users.csv"),
        [
            "Id,Name,Email",
            "1,Alice,alice@example.com",
            "2,Bob,bob@example.com"
        ]);

        var env = new FakeWebHostEnvironment
        {
            ContentRootPath = tempRoot,
            WebRootPath = tempRoot,
            ContentRootFileProvider = new PhysicalFileProvider(tempRoot),
            WebRootFileProvider = new PhysicalFileProvider(tempRoot),
            ApplicationName = "Lab 5.Tests",
            EnvironmentName = "Development"
        };

        service = new LibraryService(env);
    }

    [Fact]
    public void ReadBooks_LoadsBooksFromCsv()
    {
        var books = service.GetBooks();

        Assert.Equal(2, books.Count);
        Assert.Contains(books, b => b.Id == 1 && b.Title == "1984");
        Assert.Contains(books, b => b.Id == 2 && b.Author == "Frank Herbert");
    }

    [Fact]
    public void ReadUsers_LoadsUsersFromCsv()
    {
        var users = service.GetUsers();

        Assert.Equal(2, users.Count);
        Assert.Contains(users, u => u.Id == 1 && u.Name == "Alice");
        Assert.Contains(users, u => u.Id == 2 && u.Email == "bob@example.com");
    }

    [Fact]
    public void AddBook_AddsBookAndAssignsNextId()
    {
        var book = new Book { Title = "Neuromancer", Author = "William Gibson", ISBN = "333" };

        service.AddBook(book);
        var books = service.GetBooks();

        Assert.Equal(3, books.Count);
        Assert.Equal(3, book.Id);
        Assert.Contains(books, b => b.Id == 3 && b.Title == "Neuromancer");
    }

    [Fact]
    public void EditBook_ExistingBook_UpdatesFields()
    {
        var updatedBook = new Book { Id = 1, Title = "Nineteen Eighty-Four", Author = "George Orwell", ISBN = "999" };

        service.EditBook(updatedBook);
        var edited = service.GetBooks().First(b => b.Id == 1);

        Assert.Equal("Nineteen Eighty-Four", edited.Title);
        Assert.Equal("George Orwell", edited.Author);
        Assert.Equal("999", edited.ISBN);
    }

    [Fact]
    public void EditBook_NonexistentBook_DoesNothing()
    {
        var beforeCount = service.GetBooks().Count;
        var updatedBook = new Book { Id = 999, Title = "Missing", Author = "Nobody", ISBN = "000" };

        service.EditBook(updatedBook);

        Assert.Equal(beforeCount, service.GetBooks().Count);
        Assert.DoesNotContain(service.GetBooks(), b => b.Id == 999);
    }

    [Fact]
    public void DeleteBook_ExistingBook_RemovesBook()
    {
        service.DeleteBook(1);
        var books = service.GetBooks();

        Assert.Single(books);
        Assert.DoesNotContain(books, b => b.Id == 1);
    }

    [Fact]
    public void DeleteBook_NonexistentBook_DoesNothing()
    {
        var beforeCount = service.GetBooks().Count;

        service.DeleteBook(999);

        Assert.Equal(beforeCount, service.GetBooks().Count);
    }

    [Fact]
    public void AddUser_AddsUserAndAssignsNextId()
    {
        var user = new User { Name = "Charlie", Email = "charlie@example.com" };

        service.AddUser(user);
        var users = service.GetUsers();

        Assert.Equal(3, users.Count);
        Assert.Equal(3, user.Id);
        Assert.Contains(users, u => u.Id == 3 && u.Name == "Charlie");
    }

    [Fact]
    public void EditUser_ExistingUser_UpdatesFields()
    {
        var updatedUser = new User { Id = 1, Name = "Alice Smith", Email = "alice.smith@example.com" };

        service.EditUser(updatedUser);
        var edited = service.GetUsers().First(u => u.Id == 1);

        Assert.Equal("Alice Smith", edited.Name);
        Assert.Equal("alice.smith@example.com", edited.Email);
    }

    [Fact]
    public void DeleteUser_ExistingUser_RemovesUser()
    {
        service.DeleteUser(2);
        var users = service.GetUsers();

        Assert.Single(users);
        Assert.DoesNotContain(users, u => u.Id == 2);
    }

    [Fact]
    public void BorrowBook_AvailableBook_ReturnsTrueAndMarksBorrowed()
    {
        var result = service.BorrowBook(1, 1);

        Assert.True(result);
        Assert.True(service.IsBookBorrowed(1));
        Assert.Single(service.GetBorrowedBooksByUser(1));
    }

    [Fact]
    public void BorrowBook_AlreadyBorrowed_ReturnsFalse()
    {
        service.BorrowBook(1, 1);

        var result = service.BorrowBook(2, 1);

        Assert.False(result);
        Assert.Single(service.GetBorrowedBooksByUser(1));
        Assert.Empty(service.GetBorrowedBooksByUser(2));
    }

    [Fact]
    public void BorrowBook_InvalidUserOrBook_ReturnsFalse()
    {
        Assert.False(service.BorrowBook(999, 1));
        Assert.False(service.BorrowBook(1, 999));
    }

    [Fact]
    public void ReturnBook_BorrowedBook_ReturnsTrueAndUnmarksBorrowed()
    {
        service.BorrowBook(1, 2);

        var result = service.ReturnBook(1, 2);

        Assert.True(result);
        Assert.False(service.IsBookBorrowed(2));
        Assert.Empty(service.GetBorrowedBooksByUser(1));
        Assert.Contains(service.GetBooks(), b => b.Id == 2);
    }

    [Fact]
    public void ReturnBook_NotBorrowed_ReturnsFalse()
    {
        var result = service.ReturnBook(1, 2);

        Assert.False(result);
    }

    [Fact]
    public void GetBorrowedBooksByUser_UserWithoutBorrowedBooks_ReturnsEmptyList()
    {
        var borrowed = service.GetBorrowedBooksByUser(99);

        Assert.Empty(borrowed);
    }

    [Fact]
    public void DeleteBook_BorrowedBook_RemovesItFromBorrowedCollection()
    {
        service.BorrowBook(1, 1);

        service.DeleteBook(1);

        Assert.False(service.IsBookBorrowed(1));
        Assert.Empty(service.GetBorrowedBooksByUser(1));
    }

    [Fact]
    public void DeleteUser_BorrowingUser_RemovesBorrowedRecord()
    {
        service.BorrowBook(1, 2);

        service.DeleteUser(1);

        Assert.Empty(service.GetBorrowedBooksByUser(1));
        Assert.False(service.IsBookBorrowed(2));
    }

    public void Dispose()
    {
        if (Directory.Exists(tempRoot))
        {
            Directory.Delete(tempRoot, true);
        }
    }

    private sealed class FakeWebHostEnvironment : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = string.Empty;
        public IFileProvider WebRootFileProvider { get; set; } = default!;
        public string WebRootPath { get; set; } = string.Empty;
        public string EnvironmentName { get; set; } = string.Empty;
        public string ContentRootPath { get; set; } = string.Empty;
        public IFileProvider ContentRootFileProvider { get; set; } = default!;
    }
}
