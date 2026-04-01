using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace CinemaServer.Models;

public partial class CinemaOnlineContext : DbContext
{
    public CinemaOnlineContext()
    {
    }

    public CinemaOnlineContext(DbContextOptions<CinemaOnlineContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Collection> Collections { get; set; }

    public virtual DbSet<CollectionMovie> CollectionMovies { get; set; }

    public virtual DbSet<Comment> Comments { get; set; }

    public virtual DbSet<Favorite> Favorites { get; set; }

    public virtual DbSet<Genre> Genres { get; set; }

    public virtual DbSet<Movie> Movies { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Rating> Ratings { get; set; }

    public virtual DbSet<Subscription> Subscriptions { get; set; }

    public virtual DbSet<SubscriptionHistory> SubscriptionHistories { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<VActiveSubscription> VActiveSubscriptions { get; set; }

    public virtual DbSet<VMoviesWithGenre> VMoviesWithGenres { get; set; }

    public virtual DbSet<ViewHistory> ViewHistories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Collection>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("collections_pkey");

            entity.ToTable("collections");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.DisplayOrder)
                .HasDefaultValue(0)
                .HasColumnName("display_order");
            entity.Property(e => e.IsFeatured)
                .HasDefaultValue(false)
                .HasColumnName("is_featured");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<CollectionMovie>(entity =>
        {
            entity.HasKey(e => new { e.CollectionId, e.MovieId }).HasName("collection_movies_pkey");

            entity.ToTable("collection_movies");

            entity.Property(e => e.CollectionId).HasColumnName("collection_id");
            entity.Property(e => e.MovieId).HasColumnName("movie_id");
            entity.Property(e => e.Position)
                .HasDefaultValue(0)
                .HasColumnName("position");

            entity.HasOne(d => d.Collection).WithMany(p => p.CollectionMovies)
                .HasForeignKey(d => d.CollectionId)
                .HasConstraintName("collection_movies_collection_fk");

            entity.HasOne(d => d.Movie).WithMany(p => p.CollectionMovies)
                .HasForeignKey(d => d.MovieId)
                .HasConstraintName("collection_movies_movie_fk");
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("comments_pkey");

            entity.ToTable("comments");

            entity.HasIndex(e => e.MovieId, "idx_comments_movie_id");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.IsVisible)
                .HasDefaultValue(true)
                .HasColumnName("is_visible");
            entity.Property(e => e.MovieId).HasColumnName("movie_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Movie).WithMany(p => p.Comments)
                .HasForeignKey(d => d.MovieId)
                .HasConstraintName("comments_movie_fk");

            entity.HasOne(d => d.User).WithMany(p => p.Comments)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("comments_user_fk");
        });

        modelBuilder.Entity<Favorite>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.MovieId }).HasName("favorites_pkey");

            entity.ToTable("favorites");

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.MovieId).HasColumnName("movie_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");

            entity.HasOne(d => d.Movie).WithMany(p => p.Favorites)
                .HasForeignKey(d => d.MovieId)
                .HasConstraintName("favorites_movie_fk");

            entity.HasOne(d => d.User).WithMany(p => p.Favorites)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("favorites_user_fk");
        });

        modelBuilder.Entity<Genre>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("genres_pkey");

            entity.ToTable("genres");

            entity.HasIndex(e => e.Name, "genres_name_key").IsUnique();

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.DisplayName)
                .HasMaxLength(100)
                .HasColumnName("display_name");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Movie>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("movies_pkey");

            entity.ToTable("movies");

            entity.HasIndex(e => e.AverageRating, "idx_movies_average_rating").IsDescending();

            entity.HasIndex(e => e.CreatedAt, "idx_movies_created_at").IsDescending();

            entity.HasIndex(e => e.ReleaseYear, "idx_movies_release_year").IsDescending();

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.AverageRating)
                .HasPrecision(3, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("average_rating");
            entity.Property(e => e.BannerUrl).HasColumnName("banner_url");
            entity.Property(e => e.CommentCount)
                .HasDefaultValue(0)
                .HasColumnName("comment_count");
            entity.Property(e => e.Country)
                .HasMaxLength(100)
                .HasColumnName("country");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Director)
                .HasMaxLength(255)
                .HasColumnName("director");
            entity.Property(e => e.DurationMinutes).HasColumnName("duration_minutes");
            entity.Property(e => e.IsPublished)
                .HasDefaultValue(true)
                .HasColumnName("is_published");
            entity.Property(e => e.NeedSubscription)
                .HasDefaultValue(false)
                .HasColumnName("need_subscription");
            entity.Property(e => e.PosterUrl).HasColumnName("poster_url");
            entity.Property(e => e.PublishedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("published_at");
            entity.Property(e => e.RatingsCount)
                .HasDefaultValue(0)
                .HasColumnName("ratings_count");
            entity.Property(e => e.ReleaseYear).HasColumnName("release_year");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");
            entity.Property(e => e.VideoUrl).HasColumnName("video_url");
            entity.Property(e => e.VkVideoUrl).HasColumnName("vk_video_url");
            entity.Property(e => e.ViewCount)
                .HasDefaultValue(0L)
                .HasColumnName("view_count");

            entity.HasMany(d => d.Genres).WithMany(p => p.Movies)
                .UsingEntity<Dictionary<string, object>>(
                    "MovieGenre",
                    r => r.HasOne<Genre>().WithMany()
                        .HasForeignKey("GenreId")
                        .HasConstraintName("movie_genres_genre_fk"),
                    l => l.HasOne<Movie>().WithMany()
                        .HasForeignKey("MovieId")
                        .HasConstraintName("movie_genres_movie_fk"),
                    j =>
                    {
                        j.HasKey("MovieId", "GenreId").HasName("movie_genres_pkey");
                        j.ToTable("movie_genres");
                        j.IndexerProperty<long>("MovieId").HasColumnName("movie_id");
                        j.IndexerProperty<long>("GenreId").HasColumnName("genre_id");
                    });
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("payments_pkey");

            entity.ToTable("payments");

            entity.HasIndex(e => e.Status, "idx_payments_status");

            entity.HasIndex(e => e.UserId, "idx_payments_user_id");

            entity.HasIndex(e => e.TransactionId, "payments_transaction_id_key").IsUnique();

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Amount)
                .HasPrecision(10, 2)
                .HasColumnName("amount");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.PaymentMethod)
                .HasMaxLength(50)
                .HasColumnName("payment_method");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'pending'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.SubscriptionId).HasColumnName("subscription_id");
            entity.Property(e => e.TransactionId)
                .HasMaxLength(255)
                .HasColumnName("transaction_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Subscription).WithMany(p => p.Payments)
                .HasForeignKey(d => d.SubscriptionId)
                .HasConstraintName("payments_subscription_fk");

            entity.HasOne(d => d.User).WithMany(p => p.Payments)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("payments_user_fk");
        });

        modelBuilder.Entity<Rating>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.MovieId }).HasName("ratings_pkey");

            entity.ToTable("ratings");

            entity.HasIndex(e => e.MovieId, "idx_ratings_movie_id");

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.MovieId).HasColumnName("movie_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Rating1).HasColumnName("rating");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Movie).WithMany(p => p.Ratings)
                .HasForeignKey(d => d.MovieId)
                .HasConstraintName("ratings_movie_fk");

            entity.HasOne(d => d.User).WithMany(p => p.Ratings)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("ratings_user_fk");
        });

        modelBuilder.Entity<Subscription>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("subscriptions_pkey");

            entity.ToTable("subscriptions");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.DurationDays).HasColumnName("duration_days");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.Price)
                .HasPrecision(10, 2)
                .HasColumnName("price");
        });

        modelBuilder.Entity<SubscriptionHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("subscription_history_pkey");

            entity.ToTable("subscription_history");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.EndDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("end_date");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.StartDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("start_date");
            entity.Property(e => e.SubscriptionId).HasColumnName("subscription_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Subscription).WithMany(p => p.SubscriptionHistories)
                .HasForeignKey(d => d.SubscriptionId)
                .HasConstraintName("sub_history_subscription_fk");

            entity.HasOne(d => d.User).WithMany(p => p.SubscriptionHistories)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("sub_history_user_fk");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("users_pkey");

            entity.ToTable("users");

            entity.HasIndex(e => e.Email, "idx_users_email");

            entity.HasIndex(e => e.SubscriptionEndDate, "idx_users_subscription_end_date");

            entity.HasIndex(e => e.Email, "users_email_key").IsUnique();

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.HasSubscription)
                .HasDefaultValue(false)
                .HasColumnName("has_subscription");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("password_hash");
            entity.Property(e => e.Role)
                .HasMaxLength(20)
                .HasDefaultValueSql("'user'::character varying")
                .HasColumnName("role");
            entity.Property(e => e.SubscriptionEndDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("subscription_end_date");
            entity.Property(e => e.SubscriptionId).HasColumnName("subscription_id");
            entity.Property(e => e.SubscriptionStartDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("subscription_start_date");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Subscription).WithMany(p => p.Users)
                .HasForeignKey(d => d.SubscriptionId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("users_subscription_fk");
        });

        modelBuilder.Entity<VActiveSubscription>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_active_subscriptions");

            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.HasSubscription).HasColumnName("has_subscription");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.SubscriptionEndDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("subscription_end_date");
            entity.Property(e => e.SubscriptionName)
                .HasMaxLength(100)
                .HasColumnName("subscription_name");
            entity.Property(e => e.SubscriptionStartDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("subscription_start_date");
            entity.Property(e => e.UserId).HasColumnName("user_id");
        });

        modelBuilder.Entity<VMoviesWithGenre>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_movies_with_genres");

            entity.Property(e => e.AverageRating)
                .HasPrecision(3, 2)
                .HasColumnName("average_rating");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.DurationMinutes).HasColumnName("duration_minutes");
            entity.Property(e => e.Genres)
                .HasColumnType("character varying[]")
                .HasColumnName("genres");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.NeedSubscription).HasColumnName("need_subscription");
            entity.Property(e => e.PosterUrl).HasColumnName("poster_url");
            entity.Property(e => e.RatingsCount).HasColumnName("ratings_count");
            entity.Property(e => e.ReleaseYear).HasColumnName("release_year");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.ViewCount).HasColumnName("view_count");
        });

        modelBuilder.Entity<ViewHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("view_history_pkey");

            entity.ToTable("view_history");

            entity.HasIndex(e => e.UserId, "idx_view_history_user_id");

            entity.HasIndex(e => new { e.UserId, e.MovieId }, "idx_view_history_user_movie").IsUnique();

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Completed)
                .HasDefaultValue(false)
                .HasColumnName("completed");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.MovieId).HasColumnName("movie_id");
            entity.Property(e => e.ProgressSeconds)
                .HasDefaultValue(0)
                .HasColumnName("progress_seconds");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Movie).WithMany(p => p.ViewHistories)
                .HasForeignKey(d => d.MovieId)
                .HasConstraintName("view_history_movie_fk");

            entity.HasOne(d => d.User).WithMany(p => p.ViewHistories)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("view_history_user_fk");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
