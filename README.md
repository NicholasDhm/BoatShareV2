# 🛥️ BoatShare v2

**A modern boat sharing application built with Angular 18 and .NET 8**

## 📖 Overview

BoatShare v2 is a comprehensive boat sharing platform that allows users to manage boat reservations through a quota-based system. Users can reserve boats for specific dates using different types of quotas (Standard, Substitution, and Contingency), while administrators can manage users and boats.

## ✨ Features

### 🎯 Core Features
- **Quota-based Reservations**: Three types of quotas for flexible booking
- **Real-time Calendar**: Interactive calendar with reservation status
- **User Management**: Admin dashboard for managing users and boats
- **Responsive Design**: Modern, mobile-first interface
- **Authentication**: Secure JWT-based authentication

### 🎨 Modern Design
- **Clean UI**: Built with modern design principles
- **Animations**: Smooth transitions and micro-interactions
- **Accessibility**: WCAG compliant interface
- **Dark Mode**: Support for system theme preferences

### 🔧 Technical Features
- **Real-time Updates**: Live synchronization of reservation data
- **Offline Support**: Progressive Web App capabilities
- **Type Safety**: Full TypeScript implementation
- **Performance**: Optimized bundle size and lazy loading

## 🏗️ Architecture

### Frontend (Angular 18)
```
client/
├── src/
│   ├── app/                     # Main application module
│   ├── components/              # Reusable UI components
│   │   ├── ui-calendar/         # Calendar component
│   │   ├── ui-card/             # Card component
│   │   └── ui-navigation/       # Navigation component
│   ├── pages/                   # Application pages
│   │   ├── dashboard/           # User dashboard
│   │   ├── login/               # Authentication
│   │   └── profile/             # User profile
│   ├── services/                # Business logic services
│   ├── models/                  # TypeScript interfaces
│   └── auth/                    # Authentication services
└── package.json
```

### Backend (.NET 8)
```
server/
├── boat-share/
│   ├── Controllers/             # API endpoints
│   ├── Services/                # Business logic
│   ├── Models/                  # Data models
│   ├── Abstract/                # Interface definitions
│   └── UseCases/                # Use case implementations
└── boat-share.sln
```

## 🚀 Getting Started

### Prerequisites
- **Node.js** 18+ and npm
- **.NET 8 SDK**
- **AWS Account** (for DynamoDB)

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/NicholasDhm/BoatShareV2.git
   cd BoatShareV2
   ```

2. **Setup Frontend**
   ```bash
   cd "client copy"
   npm install
   npm start
   ```
   
   The frontend will be available at `http://localhost:4200`

3. **Setup Backend**
   ```bash
   cd server/boat-share
   dotnet restore
   dotnet run
   ```
   
   The API will be available at `https://localhost:7000`

### Configuration

1. **AWS Configuration**
   - Configure AWS credentials for DynamoDB access
   - Update `appsettings.json` with your AWS region and table names

2. **Environment Variables**
   ```bash
   # Frontend (environment.ts)
   export const environment = {
     production: false,
     apiUrl: 'https://localhost:7000/api'
   };
   ```

## 🎮 Usage

### For Members
1. **Login** with your credentials
2. **View Calendar** to see available dates and existing reservations
3. **Make Reservations** by clicking on available dates
4. **Manage Profile** to view and update your information

### For Administrators
1. **Manage Users** - Add, edit, and remove users
2. **Manage Boats** - Add and configure boats
3. **View Analytics** - Monitor system usage and reservations

## 🔒 Quota System

### Reservation Types
- **Standard**: Regular reservations for planned trips
- **Substitution**: Take over someone else's reservation
- **Contingency**: Last-minute or emergency reservations

### Status Types
- **Pending**: Awaiting confirmation
- **Confirmed**: Reservation is finalized
- **Unconfirmed**: Needs user action

## 🛠️ Development

### Code Quality
- **ESLint** for TypeScript linting
- **Prettier** for code formatting
- **Unit Tests** with Jasmine/Karma
- **Integration Tests** with .NET Test Framework

### Build Commands
```bash
# Frontend
npm run build              # Production build
npm run test               # Run tests
npm run lint               # Lint code

# Backend
dotnet build               # Build solution
dotnet test                # Run tests
```

## 📱 Progressive Web App

BoatShare v2 is built as a PWA with:
- **Offline Support**: Core functionality works without internet
- **Installation**: Can be installed on mobile devices
- **Push Notifications**: Real-time updates (coming soon)

## 🔧 API Documentation

### Authentication Endpoints
```
POST /api/auth/login       # User login
POST /api/auth/register    # User registration
POST /api/auth/refresh     # Refresh token
```

### User Endpoints
```
GET    /api/users          # Get all users (Admin)
GET    /api/users/{id}     # Get user by ID
PUT    /api/users/{id}     # Update user
DELETE /api/users/{id}     # Delete user (Admin)
```

### Reservation Endpoints
```
GET    /api/reservations              # Get all reservations
POST   /api/reservations              # Create reservation
PUT    /api/reservations/{id}         # Update reservation
DELETE /api/reservations/{id}         # Delete reservation
GET    /api/reservations/user/{id}    # Get user reservations
```

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

### Development Guidelines
- Follow the existing code style
- Write unit tests for new features
- Update documentation as needed
- Use semantic commit messages

## 📋 Roadmap

### Phase 1 (Current)
- ✅ Modern UI redesign
- ✅ Improved code quality
- ✅ Better TypeScript implementation
- ✅ Enhanced accessibility

### Phase 2 (Next)
- 🔄 Real-time notifications
- 🔄 Mobile app (React Native)
- 🔄 Analytics dashboard
- 🔄 Boat maintenance tracking

### Phase 3 (Future)
- ⏳ Payment integration
- ⏳ Weather integration
- ⏳ Route planning
- ⏳ Social features

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 👥 Team

- **Nicholas** - Lead Developer
- **Contributors** - [View all contributors](https://github.com/NicholasDhm/BoatShareV2/contributors)

## 📞 Support

For support and questions:
- 📧 Email: support@boatshare.com
- 🐛 Issues: [GitHub Issues](https://github.com/NicholasDhm/BoatShareV2/issues)
- 📖 Docs: [Wiki](https://github.com/NicholasDhm/BoatShareV2/wiki)

---

**Built with ❤️ for the boating community**
