using LibraryBlazorApp.Models;

namespace LibraryBlazorApp.Services;

public interface ILibraryService
{
    List<Book> GetBooks();
    List<User> GetUsers();

    void ReadBooks();
    void ReadUsers();

    void AddBook(Book book);
    void EditBook(Book book);
    void DeleteBook(int id);

    void AddUser(User user);
    void EditUser(User user);
    void DeleteUser(int id);

    bool BorrowBook(int userId, int bookId);
    bool ReturnBook(int userId, int bookId);

    List<Book> GetBorrowedBooksByUser(int userId);
    bool IsBookBorrowed(int bookId);
}
