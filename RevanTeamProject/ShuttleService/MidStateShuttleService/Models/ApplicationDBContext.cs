﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.EntityFrameworkCore;

namespace MidStateShuttleService.Models;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Bus> Buses { get; set; }

    public virtual DbSet<CheckIn> CheckIns { get; set; }

    public virtual DbSet<Driver> Drivers { get; set; }

    public virtual DbSet<Feedback> Feedbacks { get; set; }

    public virtual DbSet<Routes> Routes { get; set; }

    public virtual DbSet<Location> Locations { get; set; }

    public virtual DbSet<Message> Messages { get; set; }

    public virtual DbSet<RegisterModel> RegisterModels { get; set; }

    public virtual DbSet<CommuncateModel> CommuncateModels { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Bus table
        modelBuilder.Entity<Bus>(entity =>
        {
            entity.ToTable("Bus");

            entity.HasKey(b => b.BusId);

            entity.Property(b => b.BusId)
                .ValueGeneratedOnAdd()
                .IsRequired();

            entity.Property(b => b.BusNo)
                .IsRequired();

            entity.Property(b => b.PassengerCapacity)
                .IsRequired();

            entity.Property(b => b.DriverId)
                .IsRequired();

            entity.Property(b => b.IsActive)
                .HasDefaultValue(false);

            entity.HasOne(b => b.Driver)
                .WithMany()
                .HasForeignKey(b => b.DriverId)
                .IsRequired();
        });

        // Driver Table
        modelBuilder.Entity<Driver>(entity =>
        {
            entity.ToTable("Driver");

            entity.HasKey(e => e.DriverId);

            entity.Property(e => e.DriverId)
                .ValueGeneratedOnAdd()
                .IsRequired();

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.PhoneNumber)
                .IsRequired()
                .HasMaxLength(20)
                .HasAnnotation("RegularExpression", @"^\d{10,20}$")
                .IsRequired();

            entity.Property(b => b.IsActive)
                .HasDefaultValue(false);

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(50)
                .HasAnnotation("EmailAddress", "Invalid Email Address")
                .IsRequired();
        });

        // Location Table
        modelBuilder.Entity<Location>(entity =>
        {
            entity.ToTable("Location");

            entity.HasKey(e => e.LocationId);

            entity.Property(e => e.LocationId)
                .ValueGeneratedOnAdd()
                .IsRequired();

            entity.Property(b => b.IsActive)
                .HasDefaultValue(false);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100)
                .HasAnnotation("RegularExpression", "^[A-Za-z\\s]{2,25}$")
                .HasAnnotation("ErrorMessage", "Name must contain only characters, be at least 2 characters long, and not exceed 25 characters.");

            entity.Property(e => e.Address)
                .IsRequired()
                .HasMaxLength(255)
                .HasAnnotation("RegularExpression", "^[A-Za-z0-9\\s]{2,50}$")
                .HasAnnotation("ErrorMessage", "Address must contain only characters and numbers, be at least 2 characters long, and not exceed 50 characters.");

            entity.Property(e => e.City)
                .IsRequired()
                .HasMaxLength(100)
                .HasAnnotation("RegularExpression", "^[A-Za-z\\s]{2,50}$")
                .HasAnnotation("ErrorMessage", "City must contain only characters, be at least 2 characters long, and not exceed 50 characters.");

            entity.Property(e => e.State)
                .IsRequired()
                .HasMaxLength(2)
                .HasDefaultValue("WI");

            entity.Property(e => e.ZipCode)
                .IsRequired()
                .HasMaxLength(10)
                .HasAnnotation("RegularExpression", "^[0-9]{5,10}$")
                .HasAnnotation("ErrorMessage", "Zip code must contain only numbers, be at least 5 digits long, and not exceed 10 digits.");

            entity.Property(e => e.Abbreviation)
                .IsRequired()
                .HasMaxLength(5)
                .HasAnnotation("RegularExpression", "^[A-Za-z]{3,3}$")
                .HasAnnotation("ErrorMessage", "Abbreviation must contain only characters and be exactly 3 characters long.");
        });

        // Routes Table
        modelBuilder.Entity<Routes>(entity =>
        {
            entity.ToTable("Routes");

            entity.HasKey(e => e.RouteID);

            entity.Property(e => e.RouteID)
                .ValueGeneratedOnAdd()
                .IsRequired();

            entity.Property(e => e.PickUpLocationID)
                .IsRequired();

            entity.Property(e => e.DropOffLocationID)
                .IsRequired();

            entity.Property(e => e.PickUpTime)
                .HasColumnName("PickUpTime") // Match column name in SQL
                .IsRequired()
                .HasColumnType("TIME")
                .HasAnnotation("SqlDbType", SqlDbType.Time) // Map to SqlDbType for database specific type
                .HasAnnotation("DataType", "Time")
                .HasAnnotation("RegularExpression", "^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$")
                .HasAnnotation("ErrorMessage", "Please enter a valid time.");

            entity.Property(e => e.DropOffTime)
                .HasColumnName("DropOffTime") // Match column name in SQL
                .IsRequired()
                .HasColumnType("TIME")
                .HasAnnotation("SqlDbType", SqlDbType.Time) // Map to SqlDbType for database specific type
                .HasAnnotation("DataType", "Time")
                .HasAnnotation("RegularExpression", "^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$")
                .HasAnnotation("DisplayFormat", "{0:hh\\:mm tt}")
                .HasAnnotation("ErrorMessage", "Please enter a valid time.");

            entity.Property(e => e.AdditionalDetails)
                .HasMaxLength(300)
                .HasAnnotation("RegularExpression", "^[a-zA-Z0-9.,!?'\";:@#$%^&*()_+=\\-\\/]*$")
                .HasAnnotation("ErrorMessage", "Additional details can only contain letters, numbers, and important special characters.");

            entity.Property(e => e.DriverId)
                .IsRequired();

            entity.Property(b => b.IsActive)
                .HasDefaultValue(false);

            entity.HasOne(e => e.PickUpLocation)
                .WithMany()
                .HasForeignKey(e => e.PickUpLocationID)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.DropOffLocation)
                .WithMany()
                .HasForeignKey(e => e.DropOffLocationID)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Driver)
                .WithMany()
                .HasForeignKey(e => e.DriverId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // CheckIn Table
        modelBuilder.Entity<CheckIn>(entity =>
        {
            entity.ToTable("CheckIn");

            entity.HasKey(e => e.CheckInId);

            entity.Property(e => e.CheckInId)
                .ValueGeneratedOnAdd()
                .IsRequired();

            entity.Property(e => e.Name);

            entity.Property(e => e.Date)
                .HasColumnType("datetime")
                .HasDefaultValueSql("GETDATE()")
                .IsRequired();

            entity.Property(e => e.Comments)
                .HasMaxLength(255);

            entity.Property(e => e.FirstTime)
                .IsRequired()
                .HasDefaultValue(false);

            entity.Property(b => b.IsActive)
                .HasDefaultValue(false);

            entity.HasOne(e => e.Location)
                .WithMany()
                .IsRequired()
                .HasForeignKey(e => e.LocationId);
        });

        // Feedback Table
        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.ToTable("Feedback");

            entity.HasKey(e => e.FeedbackId);

            entity.Property(e => e.FeedbackId)
                .ValueGeneratedOnAdd()
                .IsRequired();

            entity.Property(e => e.Comment)
                .HasMaxLength(255);

            entity.Property(e => e.DateSubmitted)
                .HasColumnType("datetime")
                .HasDefaultValue(new DateTime())
                .IsRequired();

            entity.Property(e => e.CustomerName)
                .HasMaxLength(50)
                .HasDefaultValue("Anonymous");

            entity.Property(e => e.Rating)
                .IsRequired()
                .HasDefaultValue(0);

            entity.Property(b => b.DisplayTestimonial)
                .HasDefaultValue(false)
                .IsRequired();

            entity.Property(b => b.IsActive)
                .HasDefaultValue(false);
        });

        // Message Table
        modelBuilder.Entity<Message>(entity =>
        {
            entity.ToTable("Message");

            entity.HasKey(e => e.id)
                .HasName("PK_Message");

            entity.Property(e => e.id)
                .HasColumnName("MessageId")
                .ValueGeneratedOnAdd()
                .IsRequired();

            entity.Property(e => e.name)
                .IsRequired()
                .HasMaxLength(160)
                .HasAnnotation("RegularExpression", "^[A-Za-z\\s]{2,}$")
                .HasAnnotation("ErrorMessage", "Must contain only characters and be at least 2 characters long");

            entity.Property(e => e.message)
                .IsRequired()
                .HasMaxLength(160)
                .HasAnnotation("MaxLength", 160)
                .HasAnnotation("ErrorMessage", "This field must not be more than 160 characters.");

            entity.Property(e => e.responseRequired)
                .IsRequired()
                .HasDefaultValue(false);

            entity.Property(e => e.contactInfo)
                .HasMaxLength(50);

            entity.Property(b => b.IsActive)
                .HasDefaultValue(false);
        });

        // DispatchMessage Table
        modelBuilder.Entity<CommuncateModel>(entity =>
        {
            entity.ToTable("DispatchMessage");

            entity.HasKey(e => e.id);

            entity.Property(e => e.id)
                .HasColumnName("DispatchMessageId")
                .ValueGeneratedOnAdd()
                .IsRequired();

            entity.Property(e => e.message)
                .IsRequired()
                .HasMaxLength(160)
                .HasAnnotation("MaxLength", 160)
                .HasAnnotation("ErrorMessage", "This field must not be more than 160 characters.");

            entity.Property(e => e.PickUpLocationID)
                .IsRequired();

            entity.Property(e => e.DropOffLocationID)
                .IsRequired();

            // Configure foreign key relationship for PickUpLocationID
            entity.HasOne<Location>()
                .WithMany()
                .HasForeignKey(e => e.PickUpLocationID)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure foreign key relationship for DropOffLocationID
            entity.HasOne<Location>()
                .WithMany()
                .HasForeignKey(e => e.DropOffLocationID)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(b => b.IsActive)
                .HasDefaultValue(false);

            // Ignore LocationNames property
            entity.Ignore(e => e.LocationNames);
        });

        // Registration Table
        modelBuilder.Entity<RegisterModel>(entity =>
        {
            entity.ToTable("Registration");

            entity.HasKey(e => e.RegistrationId);

            entity.Property(e => e.RegistrationId)
                .ValueGeneratedOnAdd()
                .IsRequired();

            entity.Property(e => e.FirstName)
                .IsRequired()
                .HasMaxLength(255)
                .HasAnnotation("RegularExpression", "^[A-Za-z\\s]{2,}$")
                .HasAnnotation("ErrorMessage", "Must contain only characters and be at least 2 characters long");

            entity.Property(e => e.LastName)
                .IsRequired()
                .HasMaxLength(255)
                .HasAnnotation("RegularExpression", "^[A-Za-z\\s]{2,}$")
                .HasAnnotation("ErrorMessage", "Must contain only characters and be at least 2 characters long");

            entity.Property(e => e.PhoneNumber)
                .IsRequired()
                .HasMaxLength(255)
                .HasAnnotation("RegularExpression", "^[0-9]{10}$")
                .HasAnnotation("ErrorMessage", "Must be 10 digits");

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.TripType)
                .IsRequired()
                .HasMaxLength(10);

            entity.Property(e => e.PickUpLocationID);

            entity.Property(e => e.DropOffLocationID);

            entity.Property(e => e.NeedTransportation)
                .HasMaxLength(300);

            entity.Property(e => e.SpecialRequest)
                .IsRequired();

            entity.Property(e => e.WhichFriday)
                .HasMaxLength(300);

            entity.Property(e => e.FridayTripType)
                .HasMaxLength(20);

            entity.Property(e => e.ContactPreference)
                //.IsRequired()
                .HasMaxLength(20);

            entity.Property(e => e.AgreeTerms)
                .IsRequired();

            entity.Property(e => e.FridayAgreeTerms)
                .IsRequired();

            entity.Property(b => b.IsActive)
                .HasDefaultValue(false);

            entity.Ignore(e => e.LocationNames);

            // Configure foreign key relationship for PickUpLocationID
            entity.HasOne<Location>()
                .WithMany()
                .HasForeignKey(e => e.PickUpLocationID)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Registration_PickUpLocation");

            // Configure foreign key relationship for DropOffLocationID
            entity.HasOne<Location>()
                .WithMany()
                .HasForeignKey(e => e.DropOffLocationID)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Registration_DropOffLocation");

            // Configure foreign key relationship for FridayPickUpLocationID
            entity.HasOne<Location>()
                .WithMany()
                .HasForeignKey(e => e.FridayPickUpLocationID)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Registration_FridayPickUpLocation");

            // Configure foreign key relationship for FridayDropOffLocationID
            entity.HasOne<Location>()
                .WithMany()
                .HasForeignKey(e => e.FridayDropOffLocationID)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Registration_FridayDropOffLocation");
            
            entity.Property(x => x.Term)
                .HasConversion<string>();
        });
    }
}