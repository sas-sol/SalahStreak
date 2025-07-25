
## ��️ Database Models

### Core Entities
- **Participant** - Registered users with personal info and age group assignment
- **AgeGroup** - Configurable age ranges for participant grouping
- **BiometricLog** - Raw check-in data from biometric devices
- **AttendanceCalendar** - Scheduled prayer times with configurable windows
- **AttendanceScore** - Calculated scores based on check-ins within time windows
- **Round** - 40-day attendance cycles
- **Winner** - Participants scoring 195+ points in a round
- **Reward** - Prizes configured per age group

## �� Configuration

### Age Groups
- Children (5-12)
- Teens (13-17)
- Young Adults (18-25)
- Adults (26-40)
- Seniors (40+)

### Attendance Scoring
- **Max Score**: 5 points per day × 40 days = 200 points
- **Time Window**: ±30 minutes around scheduled prayer times
- **Winner Threshold**: 195+ points to enter winners pool

### Prayer Times
- **Format**: 24-hour (e.g., 0515, 2130)
- **Frequency**: 5 times per day
- **Configurable**: Admin can modify schedules and time windows

## 🔌 Biometric Integration

### BioTime API Setup
1. Configure BioTime server connection
2. Set up polling interval (every N minutes)
3. Map participant IDs to biometric system
4. Store raw check-in data for auditing

### Data Flow
1. Biometric device captures check-in
2. BioTime API stores the data
3. SalahStreak polls API periodically
4. System matches check-ins to participants
5. Scores are calculated and updated

## �� Customization

### Adding New Features
- **SMS Notifications**: Integrate Twilio for prayer reminders
- **Reporting**: Export attendance reports
- **Mobile App**: Create companion mobile application
- **Advanced Analytics**: Detailed attendance analytics

### Styling
- Modify `wwwroot/css/site.css` for custom styling
- Update Bootstrap theme in `Views/Shared/_Layout.cshtml`
- Add custom JavaScript in `wwwroot/js/site.js`

## 🧪 Testing

### Sample Data
The application includes seeded sample data:
- 5 age groups
- 5 sample participants
- 30 days of prayer schedule
- Sample biometric check-ins
- Sample rewards

### Test URLs
- `/Participant` - View all participants
- `/AgeGroup` - Manage age groups
- `/AttendanceCalendar` - View prayer schedule
- `/Reward` - Manage rewards

## 🚀 Deployment

### Local Development
```bash
dotnet run --environment Development
```

### Production
```bash
dotnet publish -c Release
dotnet run --environment Production
```

### Database Migration
```bash
dotnet ef database update
```

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🆘 Support

For support and questions:
- Create an issue in the GitHub repository
- Contact the development team
- Check the documentation in `/docs` folder

## �� Roadmap

### Phase 1: Core Features ✅
- [x] Participant registration
- [x] Age group management
- [x] Basic CRUD operations
- [x] Database setup

### Phase 2: Business Logic 🚧
- [ ] Biometric integration
- [ ] Attendance scoring algorithm
- [ ] Winner calculation
- [ ] Round management

### Phase 3: UI/UX Enhancement 📋
- [ ] Dashboard widgets
- [ ] Charts and statistics
- [ ] Mobile responsiveness
- [ ] Better styling

### Phase 4: Advanced Features 📋
- [ ] SMS notifications
- [ ] API integrations
- [ ] Reporting system
- [ ] Export functionality

---

**Built with ❤️ for the Islamic community**