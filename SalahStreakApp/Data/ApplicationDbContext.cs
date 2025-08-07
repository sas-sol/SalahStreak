using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SalahStreakApp.Models;

namespace SalahStreakApp.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Attendance Tracking Models
    public DbSet<Participant> Participants { get; set; }
    public DbSet<AgeGroup> AgeGroups { get; set; }
    public DbSet<BiometricLog> BiometricLogs { get; set; }
    public DbSet<AttendanceCalendar> AttendanceCalendars { get; set; }
    public DbSet<AttendanceScore> AttendanceScores { get; set; }
    public DbSet<Round> Rounds { get; set; }
    public DbSet<Winner> Winners { get; set; }
    public DbSet<Reward> Rewards { get; set; }
    public DbSet<BioTimeTransaction> BioTimeTransactions { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure relationships and constraints
        builder.Entity<Participant>()
            .HasIndex(p => p.ParticipantId)
            .IsUnique();

        builder.Entity<Participant>()
            .HasIndex(p => p.Email)
            .IsUnique();

        builder.Entity<Participant>()
            .HasOne(p => p.AgeGroup)
            .WithMany(ag => ag.Participants)
            .HasForeignKey(p => p.AgeGroupId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<BiometricLog>()
            .HasOne(bl => bl.Participant)
            .WithMany(p => p.BiometricLogs)
            .HasForeignKey(bl => bl.ParticipantId_int)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<AttendanceScore>()
            .HasOne(as_ => as_.Participant)
            .WithMany(p => p.AttendanceScores)
            .HasForeignKey(as_ => as_.ParticipantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<AttendanceScore>()
            .HasOne(as_ => as_.AttendanceCalendar)
            .WithMany(ac => ac.AttendanceScores)
            .HasForeignKey(as_ => as_.AttendanceCalendarId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<AttendanceScore>()
            .HasOne(as_ => as_.BiometricLog)
            .WithMany()
            .HasForeignKey(as_ => as_.BiometricLogId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Winner>()
            .HasOne(w => w.Round)
            .WithMany(r => r.Winners)
            .HasForeignKey(w => w.RoundId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Winner>()
            .HasOne(w => w.Participant)
            .WithMany()
            .HasForeignKey(w => w.ParticipantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Winner>()
            .HasOne(w => w.AgeGroup)
            .WithMany()
            .HasForeignKey(w => w.AgeGroupId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Reward>()
            .HasOne(r => r.AgeGroup)
            .WithMany(ag => ag.Rewards)
            .HasForeignKey(r => r.AgeGroupId)
            .OnDelete(DeleteBehavior.Restrict);

        // Ensure unique combinations
        builder.Entity<AttendanceScore>()
            .HasIndex(as_ => new { as_.ParticipantId, as_.AttendanceCalendarId })
            .IsUnique();

        builder.Entity<Winner>()
            .HasIndex(w => new { w.RoundId, w.ParticipantId })
            .IsUnique();

        // BioTime raw transactions unique on remote id
        builder.Entity<BioTimeTransaction>()
            .HasIndex(t => t.RemoteId)
            .IsUnique();
    }
}
