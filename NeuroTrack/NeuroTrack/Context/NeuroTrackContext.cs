using Microsoft.EntityFrameworkCore;
using NeuroTrack.Models;

namespace NeuroTrack.Context;

public class NeuroTrackContext : DbContext
{

    private readonly IConfiguration _configuration;

    public NeuroTrackContext(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public DbSet<GsDailyLogs> GsDailyLogs { get; set; }
    public DbSet<GsPredictions> GsPredictions { get; set; }
    public DbSet<GsScores> GsScores { get; set; }
    public DbSet<GsStatusRisk> GsStatusRisk { get; set; }
    public DbSet<GsLimits> GsLimits { get; set; }
    public DbSet<GsRole> GsRole { get; set; }
    public DbSet<GsUser> GsUser { get; set; }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        
        optionsBuilder.UseOracle(connectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GsDailyLogs>(entity =>
        {
            entity.ToTable("GS_DAILY_LOGS");

            entity.HasKey(e => e.IdLog)
                .HasName("PK_GS_DAILY_LOGS");

            entity.Property(e => e.IdLog)
                .HasColumnName("ID_LOG")
                .HasColumnType("NUMBER")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.WorkHours)
                .HasColumnName("WORK_HOURS")
                .HasColumnType("NUMBER(2)")
                .IsRequired();

            entity.Property(e => e.Meetings)
                .HasColumnName("MEETINGS")
                .HasColumnType("NUMBER(2)")
                .IsRequired();

            entity.Property(e => e.LogDate)
                .HasColumnName("LOG_DATE")
                .HasColumnType("DATE")
                .IsRequired();

            entity.Property(e => e.IdUser)
                .HasColumnName("ID_USER")
                .HasColumnType("NUMBER")
                .IsRequired();

            entity.HasOne(e => e.GsUser)
                .WithMany()
                .HasForeignKey(e => e.IdUser)
                .HasConstraintName("FK_GS_USER_GS_DAILY_LOGS");
        });
        
        modelBuilder.Entity<GsPredictions>(entity =>
        {
            entity.ToTable("GS_PREDICTIONS");

            entity.HasKey(e => e.IdPrediction)
                .HasName("PK_GS_PREDICTIONS");

            entity.Property(e => e.IdPrediction)
                .HasColumnName("ID_PREDICTION")
                .HasColumnType("NUMBER")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.StressPredicted)
                .HasColumnName("STRESS_PREDICTED")
                .HasColumnType("FLOAT(2)")
                .IsRequired();

            entity.Property(e => e.Message)
                .HasColumnName("MESSAGE")
                .HasColumnType("VARCHAR2(50)")
                .IsRequired();

            entity.Property(e => e.DatePredicted)
                .HasColumnName("DATE_PREDICTED")
                .HasColumnType("DATE")
                .IsRequired();

            entity.Property(e => e.IdUser)
                .HasColumnName("ID_USER")
                .HasColumnType("NUMBER")
                .IsRequired();

            entity.HasOne(e => e.GsUser)
                .WithMany()
                .HasForeignKey(e => e.IdUser)
                .HasConstraintName("FK_GS_USER_GS_PREDICTIONS");

            entity.Property(e => e.IdScores)
                .HasColumnName("ID_SCORES")
                .HasColumnType("NUMBER")
                .IsRequired();

            entity.HasOne(e => e.GsScores)
                .WithMany()
                .HasForeignKey(e => e.IdScores)
                .HasConstraintName("FK_GS_SCORES_GS_PREDICTIONS");

            entity.Property(e => e.IdStatusRisk)
                .HasColumnName("ID_STATUS_RISK")
                .HasColumnType("NUMBER")
                .IsRequired();

            entity.HasOne(e => e.GsStatusRisk)
                .WithMany()
                .HasForeignKey(e => e.IdStatusRisk)
                .HasConstraintName("FK_GS_STATUS_RISK_GS_PREDICTIONS");
        });

        modelBuilder.Entity<GsScores>(entity =>
        {
            entity.ToTable("GS_SCORES");

            entity.HasKey(e => e.IdScores)
                .HasName("PK_GS_SCORES");

            entity.Property(e => e.IdScores)
                .HasColumnName("ID_SCORES")
                .HasColumnType("NUMBER")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.DateScore)
                .HasColumnName("DATE_SCORE")
                .HasColumnType("DATE")
                .IsRequired();

            entity.Property(e => e.ScoreValue)
                .HasColumnName("SCORE_VALUE")
                .HasColumnType("FLOAT(4)")
                .IsRequired();

            entity.Property(e => e.TimeRecommendation)
                .HasColumnName("TIME_RECOMMENDATION")
                .HasColumnType("NUMBER(2)")
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .HasColumnName("CREATED_AT")
                .HasColumnType("DATE")
                .IsRequired();

            entity.Property(e => e.IdStatusRisk)
                .HasColumnName("ID_STATUS_RISK")
                .HasColumnType("NUMBER")
                .IsRequired();

            entity.HasOne(e => e.GsStatusRisk)
                .WithMany()
                .HasForeignKey(e => e.IdStatusRisk)
                .HasConstraintName("FK_GS_STATUS_RISK_GS_SCORES");

            entity.Property(e => e.IdUser)
                .HasColumnName("ID_USER")
                .HasColumnType("NUMBER")
                .IsRequired();

            entity.HasOne(e => e.GsUser)
                .WithMany()
                .HasForeignKey(e => e.IdUser)
                .HasConstraintName("FK_GS_USER_GS_SCORES");

            entity.Property(e => e.IdLog)
                .HasColumnName("ID_LOG")
                .HasColumnType("NUMBER")
                .IsRequired();

            entity.HasOne(e => e.GsDailyLogs)
                .WithOne()
                .HasForeignKey<GsScores>(e => e.IdLog)
                .HasConstraintName("FK_GS_DAILY_LOGS_GS_SCORES");
        });
        
        modelBuilder.Entity<GsStatusRisk>(entity =>
        {
            entity.ToTable("GS_STATUS_RISK");

            entity.HasKey(e => e.IdStatusRisk)
                .HasName("PK_GS_STATUS_RISK");

            entity.Property(e => e.IdStatusRisk)
                .HasColumnName("ID_STATUS_RISK")
                .HasColumnType("NUMBER")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.StatusNameRisk)
                .HasColumnName("STATUS_NAME_RISK")
                .HasColumnType("VARCHAR2(15)")
                .IsRequired();
        });

        modelBuilder.Entity<GsLimits>(entity =>
        {
            entity.ToTable("GS_LIMITS");

            entity.HasKey(e => e.IdLimits)
                .HasName("PK_GS_LIMITS");

            entity.Property(e => e.IdLimits)
                .HasColumnName("ID_LIMITS")
                .HasColumnType("NUMBER")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.LimitHours)
                .HasColumnName("LIMIT_HOURS")
                .HasColumnType("NUMBER(2)")
                .IsRequired();

            entity.Property(e => e.LimitMeetings)
                .HasColumnName("LIMIT_MEETINGS")
                .HasColumnType("NUMBER(2)")
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .HasColumnName("CREATED_AT")
                .HasColumnType("DATE")
                .IsRequired();
        });

        modelBuilder.Entity<GsRole>(entity =>
        {
            entity.ToTable("GS_ROLE");

            entity.HasKey(e => e.IdRole)
                .HasName("PK_GS_ROLE");

            entity.Property(e => e.IdRole)
                .HasColumnName("ID_ROLE")
                .HasColumnType("NUMBER")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.RoleName)
                .HasColumnName("ROLE_NAME")
                .HasColumnType("VARCHAR2(20)")
                .IsRequired();
        });

        modelBuilder.Entity<GsUser>(entity =>
        {
            entity.ToTable("GS_USER");

            entity.HasKey(e => e.IdUser)
                .HasName("PK_GS_USER");

            entity.Property(e => e.IdUser)
                .HasColumnName("ID_USER")
                .HasColumnType("NUMBER")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.NameUser)
                .HasColumnName("NAME_USER")
                .HasColumnType("VARCHAR2(100)")
                .IsRequired();

            entity.Property(e => e.EmailUser)
                .HasColumnName("EMAIL_USER")
                .HasColumnType("VARCHAR2(255)")
                .IsRequired();

            entity.HasIndex(e => e.EmailUser)
                .IsUnique()
                .HasDatabaseName("UK_GS_USER_EMAIL_USER");

            entity.Property(e => e.PasswordUser)
                .HasColumnName("PASSWORD_USER")
                .HasColumnType("VARCHAR2(255)")
                .IsRequired();

            entity.Property(e => e.Status)
                .HasColumnName("STATUS")
                .HasColumnType("CHAR(1)")
                .IsRequired();

            entity.Property(e => e.IdRole)
                .HasColumnName("ID_ROLE")
                .HasColumnType("NUMBER")
                .IsRequired();

            entity.HasOne(e => e.GsRole)
                .WithMany()
                .HasForeignKey(e => e.IdRole)
                .HasConstraintName("FK_GS_ROLE_GS_USER");

            entity.Property(e => e.IdLimits)
                .HasColumnName("ID_LIMITS")
                .HasColumnType("NUMBER")
                .IsRequired();

            entity.HasOne(e => e.GsLimits)
                .WithOne()
                .HasForeignKey<GsUser>(e => e.IdLimits)
                .HasConstraintName("FK_GS_LIMITS_GS_USER");
        });
    }
}