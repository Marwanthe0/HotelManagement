using HotelManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Infrastructure.Data;

public class HotelDbContext : DbContext
{
    public HotelDbContext(DbContextOptions<HotelDbContext> options) : base(options)
    {

    }
    public DbSet<Room> Rooms { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);



        /*--------Setting Precision For Decimal Values----------*/

        modelBuilder.Entity<Room>()
        .Property(r => r.PricePerNight)
        .HasPrecision(18, 2);

        modelBuilder.Entity<Booking>()
        .Property(b => b.TotalAmount)
        .HasPrecision(18, 2);


        modelBuilder.Entity<Payment>()
        .Property(p => p.Amount)
        .HasPrecision(18, 2);


        modelBuilder.Entity<Employee>()
        .Property(e => e.Salary)
        .HasPrecision(18, 2);

        /*--------Setting Indexes----------*/

        modelBuilder.Entity<Room>()
        .HasIndex(r => r.RoomNumber)
        .IsUnique();

        modelBuilder.Entity<Customer>()
        .HasIndex(c => c.Email)
        .IsUnique();

        modelBuilder.Entity<Employee>()
        .HasIndex(e => e.Email)
        .IsUnique();

        modelBuilder.Entity<User>()
        .HasIndex(u => u.Email)
        .IsUnique();

        modelBuilder.Entity<User>()
        .HasIndex(u => u.Username)
        .IsUnique();

        /*--------Setting Relationship Between Tables------------*/

        //Customer <--> Booking Relationship
        //One Customer  ---> Many Booking
        modelBuilder.Entity<Booking>()
        .HasOne(b => b.Customer)
        .WithMany(c => c.Bookings)
        .HasForeignKey(b => b.CustomerId)
        .OnDelete(DeleteBehavior.Restrict);

        //Room <--> Booking Relationship
        //One Room ---> Many Bookings
        modelBuilder.Entity<Booking>()
        .HasOne(b => b.Room)
        .WithMany()
        .HasForeignKey(b => b.RoomId)
        .OnDelete(DeleteBehavior.Restrict);

        // Booking <--> Payment Relationship
        //One Booking ---> Many Payments
        modelBuilder.Entity<Payment>()
        .HasOne(p => p.Booking)
        .WithMany()
        .HasForeignKey(p => p.BookingId)
        .OnDelete(DeleteBehavior.Restrict);
    }
}