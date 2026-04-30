using LibraryBlazorApp.Models;

namespace LibraryBlazorApp.Services;

public class LibraryService : ILibraryService
{
    public List<Book> Books { get; private set; } = new();
    public List<User> Users { get; private set; } = new();
    public Dictionary<int, List<Book>> BorrowedBooks { get; private set; } = new();

    private readonly string booksFilePath;
    private readonly string usersFilePath;

    public LibraryService(IWebHostEnvironment env)
    {
        var dataFolder = Path.Combine(env.ContentRootPath, "Data");
        Directory.CreateDirectory(dataFolder);

        booksFilePath = Path.Combine(dataFolder, "Books.csv");
        usersFilePath = Path.Combine(dataFolder, "Users.csv");

        if (!File.Exists(booksFilePath))
        {
            File.WriteAllLines(booksFilePath, ["Id,Title,Author,ISBN"]);
        }

        if (!File.Exists(usersFilePath))
        {
            File.WriteAllLines(usersFilePath, ["Id,Name,Email"]);
        }

        ReadBooks();
        ReadUsers();
    }

    public List<Book> GetBooks() => Books;
    public List<User> GetUsers() => Users;

    public void ReadBooks()
    {
        Books.Clear();

        if (!File.Exists(booksFilePath))
            return;

        foreach (var line in File.ReadAllLines(booksFilePath).Skip(1))
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var fields = line.Split(',');
            if (fields.Length < 4)
                continue;

            if (!int.TryParse(fields[0].Trim(), out int id))
                continue;

            Books.Add(new Book
            {
                Id = id,
                Title = fields[1].Trim(),
                Author = fields[2].Trim(),
                ISBN = fields[3].Trim()
            });
        }
    }

    public void ReadUsers()
    {
        Users.Clear();

        if (!File.Exists(usersFilePath))
            return;

        foreach (var line in File.ReadAllLines(usersFilePath).Skip(1))
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var fields = line.Split(',');
            if (fields.Length < 3)
                continue;

            if (!int.TryParse(fields[0].Trim(), out int id))
                continue;

            Users.Add(new User
            {
                Id = id,
                Name = fields[1].Trim(),
                Email = fields[2].Trim()
            });
        }
    }

    public void SaveBooks()
    {
        var lines = new[] { "Id,Title,Author,ISBN" }
            .Concat(Books.Select(b => $"{b.Id},{b.Title},{b.Author},{b.ISBN}"));
        File.WriteAllLines(booksFilePath, lines);
    }

    public void SaveUsers()
    {
        var lines = new[] { "Id,Name,Email" }
            .Concat(Users.Select(u => $"{u.Id},{u.Name},{u.Email}"));
        File.WriteAllLines(usersFilePath, lines);
    }

    public void AddBook(Book book)
    {
        book.Id = Books.Any() ? Books.Max(b => b.Id) + 1 : 1;
        Books.Add(new Book
        {
            Id = book.Id,
            Title = book.Title,
            Author = book.Author,
            ISBN = book.ISBN
        });
        SaveBooks();
    }

    public void EditBook(Book updatedBook)
    {
        var book = Books.FirstOrDefault(b => b.Id == updatedBook.Id);
        if (book is null) return;

        book.Title = updatedBook.Title;
        book.Author = updatedBook.Author;
        book.ISBN = updatedBook.ISBN;
        SaveBooks();
    }

    public void DeleteBook(int id)
    {
        var book = Books.FirstOrDefault(b => b.Id == id);
        var wasRemoved = false;

        if (book is not null)
        {
            Books.Remove(book);
            wasRemoved = true;
        }

        foreach (var borrowed in BorrowedBooks.Values)
        {
            if (borrowed.RemoveAll(b => b.Id == id) > 0)
            {
                wasRemoved = true;
            }
        }

        if (wasRemoved)
        {
            SaveBooks();
        }
    }

    public void AddUser(User user)
    {
        user.Id = Users.Any() ? Users.Max(u => u.Id) + 1 : 1;
        Users.Add(new User
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email
        });
        SaveUsers();
    }

    public void EditUser(User updatedUser)
    {
        var user = Users.FirstOrDefault(u => u.Id == updatedUser.Id);
        if (user is null) return;

        user.Name = updatedUser.Name;
        user.Email = updatedUser.Email;
        SaveUsers();
    }

    public void DeleteUser(int id)
    {
        var user = Users.FirstOrDefault(u => u.Id == id);
        if (user is null) return;

        Users.Remove(user);
        BorrowedBooks.Remove(id);
        SaveUsers();
    }

    public bool BorrowBook(int userId, int bookId)
    {
        return false;

        var user = Users.FirstOrDefault(u => u.Id == userId);
        var book = Books.FirstOrDefault(b => b.Id == bookId);

        if (user is null || book is null)
            return false;

        if (!BorrowedBooks.ContainsKey(userId))
            BorrowedBooks[userId] = new List<Book>();

        if (BorrowedBooks.Values.Any(list => list.Any(b => b.Id == bookId)))
            return false;

        BorrowedBooks[userId].Add(book);
        Books.Remove(book);
        SaveBooks();
        return true;
    }

    public bool ReturnBook(int userId, int bookId)
    {
        if (!BorrowedBooks.ContainsKey(userId))
            return false;

        var book = BorrowedBooks[userId].FirstOrDefault(b => b.Id == bookId);
        if (book is null)
            return false;

        BorrowedBooks[userId].Remove(book);
        Books.Add(book);
        SaveBooks();
        return true;
    }

    public List<Book> GetBorrowedBooksByUser(int userId)
    {
        if (!BorrowedBooks.ContainsKey(userId))
            return new List<Book>();

        return BorrowedBooks[userId];
    }

    public bool IsBookBorrowed(int bookId)
    {
        return BorrowedBooks.Values.Any(list => list.Any(book => book.Id == bookId));
    }
}
