
using Microsoft.EntityFrameworkCore;

public class MyDbContext : DbContext
{
    public MyDbContext()
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=kostya;Username=kostya;Password=;");
    }

    public DbSet<Author> authors { get; set; }
    public DbSet<Book> books { get; set; }
}


public class Author
{
    public int id { get; set; }
    public string name { get; set; }
}

public class Book
{
    public int id { get; set; }
    public string title { get; set; }
    public int author_id { get; set; }
}