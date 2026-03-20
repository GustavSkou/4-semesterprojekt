using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace PersistencePlugin.Models;

public partial class ProductionDbContext : DbContext
{
    public ProductionDbContext(DbContextOptions<ProductionDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<_operator> operators { get; set; }

    public virtual DbSet<cache> caches { get; set; }

    public virtual DbSet<cache_lock> cache_locks { get; set; }

    public virtual DbSet<category> categories { get; set; }

    public virtual DbSet<component> components { get; set; }

    public virtual DbSet<computer> computers { get; set; }

    public virtual DbSet<computer_component_list> computer_component_lists { get; set; }

    public virtual DbSet<customer> customers { get; set; }

    public virtual DbSet<failed_job> failed_jobs { get; set; }

    public virtual DbSet<job> jobs { get; set; }

    public virtual DbSet<job_batch> job_batches { get; set; }

    public virtual DbSet<level> levels { get; set; }

    public virtual DbSet<log> logs { get; set; }

    public virtual DbSet<migration> migrations { get; set; }

    public virtual DbSet<order> orders { get; set; }

    public virtual DbSet<password_reset_token> password_reset_tokens { get; set; }

    public virtual DbSet<requirement> requirements { get; set; }

    public virtual DbSet<session> sessions { get; set; }

    public virtual DbSet<source> sources { get; set; }

    public virtual DbSet<specification> specifications { get; set; }

    public virtual DbSet<type> types { get; set; }

    public virtual DbSet<wattage_list> wattage_lists { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<_operator>(entity =>
        {
            entity.HasKey(e => e.id).HasName("operators_pkey");

            entity.HasIndex(e => e.email, "operators_email_unique").IsUnique();

            entity.Property(e => e.created_at).HasColumnType("timestamp(0) without time zone");
            entity.Property(e => e.email).HasMaxLength(255);
            entity.Property(e => e.email_verified_at).HasColumnType("timestamp(0) without time zone");
            entity.Property(e => e.name).HasMaxLength(255);
            entity.Property(e => e.password).HasMaxLength(255);
            entity.Property(e => e.remember_token).HasMaxLength(100);
            entity.Property(e => e.updated_at).HasColumnType("timestamp(0) without time zone");
        });

        modelBuilder.Entity<cache>(entity =>
        {
            entity.HasKey(e => e.key).HasName("cache_pkey");

            entity.ToTable("cache");

            entity.HasIndex(e => e.expiration, "cache_expiration_index");

            entity.Property(e => e.key).HasMaxLength(255);
        });

        modelBuilder.Entity<cache_lock>(entity =>
        {
            entity.HasKey(e => e.key).HasName("cache_locks_pkey");

            entity.HasIndex(e => e.expiration, "cache_locks_expiration_index");

            entity.Property(e => e.key).HasMaxLength(255);
            entity.Property(e => e.owner).HasMaxLength(255);
        });

        modelBuilder.Entity<category>(entity =>
        {
            entity.HasKey(e => e.id).HasName("categories_pkey");

            entity.Property(e => e.created_at).HasColumnType("timestamp(0) without time zone");
            entity.Property(e => e.name).HasMaxLength(255);
            entity.Property(e => e.updated_at).HasColumnType("timestamp(0) without time zone");
        });

        modelBuilder.Entity<component>(entity =>
        {
            entity.HasKey(e => e.id).HasName("components_pkey");

            entity.Property(e => e.created_at).HasColumnType("timestamp(0) without time zone");
            entity.Property(e => e.name).HasMaxLength(255);
            entity.Property(e => e.price).HasPrecision(10, 2);
            entity.Property(e => e.updated_at).HasColumnType("timestamp(0) without time zone");

            entity.HasOne(d => d.category).WithMany(p => p.components)
                .HasForeignKey(d => d.category_id)
                .HasConstraintName("components_category_id_foreign");
        });

        modelBuilder.Entity<computer>(entity =>
        {
            entity.HasKey(e => e.id).HasName("computers_pkey");

            entity.HasIndex(e => e.order_id, "computers_order_id_unique").IsUnique();

            entity.Property(e => e.created_at).HasColumnType("timestamp(0) without time zone");
            entity.Property(e => e.updated_at).HasColumnType("timestamp(0) without time zone");

            entity.HasOne(d => d.order).WithOne(p => p.computer)
                .HasForeignKey<computer>(d => d.order_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("computers_order_id_foreign");
        });

        modelBuilder.Entity<computer_component_list>(entity =>
        {
            entity.HasKey(e => e.id).HasName("computer_component_list_pkey");

            entity.ToTable("computer_component_list");

            entity.Property(e => e.created_at).HasColumnType("timestamp(0) without time zone");
            entity.Property(e => e.updated_at).HasColumnType("timestamp(0) without time zone");

            entity.HasOne(d => d.component).WithMany(p => p.computer_component_lists)
                .HasForeignKey(d => d.component_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("computer_component_list_component_id_foreign");

            entity.HasOne(d => d.computer).WithMany(p => p.computer_component_lists)
                .HasForeignKey(d => d.computer_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("computer_component_list_computer_id_foreign");
        });

        modelBuilder.Entity<customer>(entity =>
        {
            entity.HasKey(e => e.id).HasName("customers_pkey");

            entity.HasIndex(e => e.email, "customers_email_unique").IsUnique();

            entity.Property(e => e.address).HasMaxLength(255);
            entity.Property(e => e.created_at).HasColumnType("timestamp(0) without time zone");
            entity.Property(e => e.email).HasMaxLength(255);
            entity.Property(e => e.name).HasMaxLength(255);
            entity.Property(e => e.updated_at).HasColumnType("timestamp(0) without time zone");
        });

        modelBuilder.Entity<failed_job>(entity =>
        {
            entity.HasKey(e => e.id).HasName("failed_jobs_pkey");

            entity.HasIndex(e => e.uuid, "failed_jobs_uuid_unique").IsUnique();

            entity.Property(e => e.failed_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(0) without time zone");
            entity.Property(e => e.uuid).HasMaxLength(255);
        });

        modelBuilder.Entity<job>(entity =>
        {
            entity.HasKey(e => e.id).HasName("jobs_pkey");

            entity.HasIndex(e => e.queue, "jobs_queue_index");

            entity.Property(e => e.queue).HasMaxLength(255);
        });

        modelBuilder.Entity<job_batch>(entity =>
        {
            entity.HasKey(e => e.id).HasName("job_batches_pkey");

            entity.Property(e => e.id).HasMaxLength(255);
            entity.Property(e => e.name).HasMaxLength(255);
        });

        modelBuilder.Entity<level>(entity =>
        {
            entity.HasKey(e => e.id).HasName("levels_pkey");

            entity.Property(e => e.created_at).HasColumnType("timestamp(0) without time zone");
            entity.Property(e => e.name).HasMaxLength(255);
            entity.Property(e => e.updated_at).HasColumnType("timestamp(0) without time zone");
        });

        modelBuilder.Entity<log>(entity =>
        {
            entity.HasKey(e => e.id).HasName("logs_pkey");

            entity.Property(e => e.created_at).HasColumnType("timestamp(0) without time zone");
            entity.Property(e => e.timestamp)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(0) without time zone");
            entity.Property(e => e.updated_at).HasColumnType("timestamp(0) without time zone");

            entity.HasOne(d => d.level).WithMany(p => p.logs)
                .HasForeignKey(d => d.level_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("logs_level_id_foreign");

            entity.HasOne(d => d.source).WithMany(p => p.logs)
                .HasForeignKey(d => d.source_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("logs_source_id_foreign");

            entity.HasOne(d => d.type).WithMany(p => p.logs)
                .HasForeignKey(d => d.type_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("logs_type_id_foreign");
        });

        modelBuilder.Entity<migration>(entity =>
        {
            entity.HasKey(e => e.id).HasName("migrations_pkey");

            entity.Property(e => e.migration1)
                .HasMaxLength(255)
                .HasColumnName("migration");
        });

        modelBuilder.Entity<order>(entity =>
        {
            entity.HasKey(e => e.id).HasName("orders_pkey");

            entity.Property(e => e.created_at).HasColumnType("timestamp(0) without time zone");
            entity.Property(e => e.status).HasMaxLength(255);
            entity.Property(e => e.updated_at).HasColumnType("timestamp(0) without time zone");

            entity.HasOne(d => d.customer).WithMany(p => p.orders)
                .HasForeignKey(d => d.customer_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("orders_customer_id_foreign");
        });

        modelBuilder.Entity<password_reset_token>(entity =>
        {
            entity.HasKey(e => e.email).HasName("password_reset_tokens_pkey");

            entity.Property(e => e.email).HasMaxLength(255);
            entity.Property(e => e.created_at).HasColumnType("timestamp(0) without time zone");
            entity.Property(e => e.token).HasMaxLength(255);
        });

        modelBuilder.Entity<requirement>(entity =>
        {
            entity.HasKey(e => e.id).HasName("requirements_pkey");

            entity.Property(e => e.created_at).HasColumnType("timestamp(0) without time zone");
            entity.Property(e => e.name).HasMaxLength(255);
            entity.Property(e => e.updated_at).HasColumnType("timestamp(0) without time zone");
            entity.Property(e => e.value).HasMaxLength(255);

            entity.HasOne(d => d.component).WithMany(p => p.requirements)
                .HasForeignKey(d => d.component_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("requirements_component_id_foreign");
        });

        modelBuilder.Entity<session>(entity =>
        {
            entity.HasKey(e => e.id).HasName("sessions_pkey");

            entity.HasIndex(e => e.last_activity, "sessions_last_activity_index");

            entity.HasIndex(e => e.user_id, "sessions_user_id_index");

            entity.Property(e => e.id).HasMaxLength(255);
            entity.Property(e => e.ip_address).HasMaxLength(45);
        });

        modelBuilder.Entity<source>(entity =>
        {
            entity.HasKey(e => e.id).HasName("sources_pkey");

            entity.Property(e => e.created_at).HasColumnType("timestamp(0) without time zone");
            entity.Property(e => e.name).HasMaxLength(255);
            entity.Property(e => e.updated_at).HasColumnType("timestamp(0) without time zone");
        });

        modelBuilder.Entity<specification>(entity =>
        {
            entity.HasKey(e => e.id).HasName("specifications_pkey");

            entity.Property(e => e.created_at).HasColumnType("timestamp(0) without time zone");
            entity.Property(e => e.name).HasMaxLength(255);
            entity.Property(e => e.updated_at).HasColumnType("timestamp(0) without time zone");
            entity.Property(e => e.value).HasMaxLength(255);

            entity.HasOne(d => d.component).WithMany(p => p.specifications)
                .HasForeignKey(d => d.component_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("specifications_component_id_foreign");
        });

        modelBuilder.Entity<type>(entity =>
        {
            entity.HasKey(e => e.id).HasName("types_pkey");

            entity.Property(e => e.created_at).HasColumnType("timestamp(0) without time zone");
            entity.Property(e => e.name).HasMaxLength(255);
            entity.Property(e => e.updated_at).HasColumnType("timestamp(0) without time zone");
        });

        modelBuilder.Entity<wattage_list>(entity =>
        {
            entity.HasKey(e => e.id).HasName("wattage_lists_pkey");

            entity.Property(e => e.created_at).HasColumnType("timestamp(0) without time zone");
            entity.Property(e => e.updated_at).HasColumnType("timestamp(0) without time zone");

            entity.HasOne(d => d.component).WithMany(p => p.wattage_lists)
                .HasForeignKey(d => d.component_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("wattage_lists_component_id_foreign");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
