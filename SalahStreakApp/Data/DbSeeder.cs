using Microsoft.EntityFrameworkCore;
using SalahStreakApp.Models;

namespace SalahStreakApp.Data
{
    public static class DbSeeder
    {
        public static void Seed(ApplicationDbContext context)
        {
            // Ensure database is created
            context.Database.EnsureCreated();

            // Check if data already exists
            if (context.AgeGroups.Any())
            {
                return; // Database has been seeded
            }

            // Seed Age Groups
            var ageGroups = new AgeGroup[]
            {
                new AgeGroup { Name = "Children (5-12)", MinAge = 5, MaxAge = 12, Description = "Young children" },
                new AgeGroup { Name = "Teens (13-17)", MinAge = 13, MaxAge = 17, Description = "Teenagers" },
                new AgeGroup { Name = "Young Adults (18-25)", MinAge = 18, MaxAge = 25, Description = "Young adults" },
                new AgeGroup { Name = "Adults (26-40)", MinAge = 26, MaxAge = 40, Description = "Adults" },
                new AgeGroup { Name = "Seniors (40+)", MinAge = 40, MaxAge = 100, Description = "Senior participants" }
            };

            context.AgeGroups.AddRange(ageGroups);
            context.SaveChanges();

            // Seed Sample Participants
            var participants = new Participant[]
            {
                new Participant { 
                    FullName = "Ahmed Ali", 
                    Age = 15, 
                    Gender = "Male", 
                    Phone = "+92-300-1234567", 
                    Email = "ahmed.ali@example.com",
                    ParentName = "Ali Hassan",
                    ParentCNIC = "35202-1234567-8",
                    AgeGroupId = ageGroups[1].Id, // Teens
                    ParticipantId = "P001",
                    CreatedAt = DateTime.Now.AddDays(-30)
                },
                new Participant { 
                    FullName = "Fatima Khan", 
                    Age = 22, 
                    Gender = "Female", 
                    Phone = "+92-301-2345678", 
                    Email = "fatima.khan@example.com",
                    ParentName = "Khan Sahab",
                    ParentCNIC = "35202-2345678-9",
                    AgeGroupId = ageGroups[2].Id, // Young Adults
                    ParticipantId = "P002",
                    CreatedAt = DateTime.Now.AddDays(-25)
                },
                new Participant { 
                    FullName = "Muhammad Hassan", 
                    Age = 35, 
                    Gender = "Male", 
                    Phone = "+92-302-3456789", 
                    Email = "m.hassan@example.com",
                    ParentName = "Hassan Ali",
                    ParentCNIC = "35202-3456789-0",
                    AgeGroupId = ageGroups[3].Id, // Adults
                    ParticipantId = "P003",
                    CreatedAt = DateTime.Now.AddDays(-20)
                },
                new Participant { 
                    FullName = "Aisha Rahman", 
                    Age = 12, 
                    Gender = "Female", 
                    Phone = "+92-303-4567890", 
                    Email = "aisha.rahman@example.com",
                    ParentName = "Rahman Khan",
                    ParentCNIC = "35202-4567890-1",
                    AgeGroupId = ageGroups[0].Id, // Children
                    ParticipantId = "P004",
                    CreatedAt = DateTime.Now.AddDays(-15)
                },
                new Participant { 
                    FullName = "Omar Farooq", 
                    Age = 45, 
                    Gender = "Male", 
                    Phone = "+92-304-5678901", 
                    Email = "omar.farooq@example.com",
                    ParentName = "Farooq Ahmed",
                    ParentCNIC = "35202-5678901-2",
                    AgeGroupId = ageGroups[4].Id, // Seniors
                    ParticipantId = "P005",
                    CreatedAt = DateTime.Now.AddDays(-10)
                }
            };

            context.Participants.AddRange(participants);
            context.SaveChanges();

            // Seed Sample Attendance Calendar (for current month)
            var currentDate = DateTime.Now.Date;
            var calendarEntries = new List<AttendanceCalendar>();

            for (int day = 1; day <= 30; day++)
            {
                var date = new DateTime(currentDate.Year, currentDate.Month, day);
                if (date <= DateTime.Now.Date) // Only create entries up to today
                {
                    // 5 prayer times per day - convert to TimeSpan
                    var prayerTimes = new[] 
                    { 
                        new TimeSpan(5, 15, 0), // 05:15
                        new TimeSpan(12, 30, 0), // 12:30
                        new TimeSpan(16, 0, 0), // 16:00
                        new TimeSpan(18, 30, 0), // 18:30
                        new TimeSpan(20, 0, 0) // 20:00
                    };
                    
                    foreach (var time in prayerTimes)
                    {
                        calendarEntries.Add(new AttendanceCalendar
                        {
                            Date = date,
                            ExpectedTime = time,
                            TimeWindowMinutes = 30, // ±30 minutes default
                            IsActive = true,
                            Description = $"Prayer time on {date:MMM dd, yyyy}"
                        });
                    }
                }
            }

            context.AttendanceCalendars.AddRange(calendarEntries);
            context.SaveChanges();

            // Seed Sample Biometric Logs (some check-ins)
            var biometricLogs = new BiometricLog[]
            {
                new BiometricLog { 
                    ParticipantId = "P001",
                    ParticipantId_int = participants[0].Id, 
                    CheckInTime = DateTime.Now.AddDays(-1).AddHours(5).AddMinutes(15), // Fajr
                    DeviceId = "DEV001",
                    RawData = "Sample biometric data",
                    IsProcessed = false,
                    CreatedAt = DateTime.Now
                },
                new BiometricLog { 
                    ParticipantId = "P001",
                    ParticipantId_int = participants[0].Id, 
                    CheckInTime = DateTime.Now.AddDays(-1).AddHours(12).AddMinutes(30), // Dhuhr
                    DeviceId = "DEV001",
                    RawData = "Sample biometric data",
                    IsProcessed = false,
                    CreatedAt = DateTime.Now
                },
                new BiometricLog { 
                    ParticipantId = "P002",
                    ParticipantId_int = participants[1].Id, 
                    CheckInTime = DateTime.Now.AddDays(-2).AddHours(5).AddMinutes(20), // Fajr
                    DeviceId = "DEV002",
                    RawData = "Sample biometric data",
                    IsProcessed = false,
                    CreatedAt = DateTime.Now
                },
                new BiometricLog { 
                    ParticipantId = "P003",
                    ParticipantId_int = participants[2].Id, 
                    CheckInTime = DateTime.Now.AddDays(-3).AddHours(16).AddMinutes(5), // Asr
                    DeviceId = "DEV003",
                    RawData = "Sample biometric data",
                    IsProcessed = false,
                    CreatedAt = DateTime.Now
                },
                new BiometricLog { 
                    ParticipantId = "P004",
                    ParticipantId_int = participants[3].Id, 
                    CheckInTime = DateTime.Now.AddDays(-4).AddHours(18).AddMinutes(45), // Maghrib
                    DeviceId = "DEV001",
                    RawData = "Sample biometric data",
                    IsProcessed = false,
                    CreatedAt = DateTime.Now
                },
                new BiometricLog { 
                    ParticipantId = "P005",
                    ParticipantId_int = participants[4].Id, 
                    CheckInTime = DateTime.Now.AddDays(-5).AddHours(20).AddMinutes(10), // Isha
                    DeviceId = "DEV002",
                    RawData = "Sample biometric data",
                    IsProcessed = false,
                    CreatedAt = DateTime.Now
                }
            };

            context.BiometricLogs.AddRange(biometricLogs);
            context.SaveChanges();

            // Seed Sample Rewards
            var rewards = new Reward[]
            {
                new Reward { 
                    Title = "Gold Medal", 
                    Description = "Exclusive gold medal for top performers", 
                    Quantity = 10, 
                    AgeGroupId = ageGroups[0].Id, // Children
                    DeliveryStatus = "Pending"
                },
                new Reward { 
                    Title = "Silver Medal", 
                    Description = "Silver medal for excellent attendance", 
                    Quantity = 15, 
                    AgeGroupId = ageGroups[1].Id, // Teens
                    DeliveryStatus = "Pending"
                },
                new Reward { 
                    Title = "Certificate of Excellence", 
                    Description = "Certificate for outstanding performance", 
                    Quantity = 20, 
                    AgeGroupId = ageGroups[2].Id, // Young Adults
                    DeliveryStatus = "Pending"
                }
            };

            context.Rewards.AddRange(rewards);
            context.SaveChanges();

            // Seed Current Round
            var currentRound = new Round
            {
                Name = "Ramadan 2024",
                StartDate = DateTime.Now.AddDays(-30),
                EndDate = DateTime.Now.AddDays(10),
                DurationDays = 40,
                IsActive = true
            };

            context.Rounds.Add(currentRound);
            context.SaveChanges();
        }
    }
}
