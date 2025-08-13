import { Routes } from '@angular/router';

// Lazy-loaded components for better performance
const routes: Routes = [
  // Public routes
  {
    path: 'login',
    loadComponent: () => import('../pages/login/login.component').then(m => m.LoginComponent),
    title: 'Login - BoatShare'
  },
  {
    path: 'register',
    loadComponent: () => import('../pages/register-user/register.component').then(m => m.RegisterUserComponent),
    title: 'Register - BoatShare'
  },

  // Protected routes
  {
    path: 'dashboard',
    loadComponent: () => import('../pages/dashboard/dashboard.component').then(m => m.DashboardComponent),
    title: 'Painel de Controle - BoatShare',
    // canActivate: [AuthGuard] // Uncomment when AuthGuard is implemented
  },
  {
    path: 'profile',
    loadComponent: () => import('../pages/profile/profile.component').then(m => m.ProfileComponent),
    title: 'Profile - BoatShare',
    // canActivate: [AuthGuard]
  },

  // Admin routes
  {
    path: 'admin',
    // canActivate: [AuthGuard, AdminGuard], // Uncomment when guards are implemented
    children: [
      {
        path: 'users',
        loadComponent: () => import('../pages/manage-users/mange-users.component').then(m => m.ManageUsersComponent),
        title: 'Manage Users - BoatShare'
      },
      {
        path: 'boats',
        loadComponent: () => import('../pages/manage-boats/manage-boats.component').then(m => m.ManageBoatsComponent),
        title: 'Manage Boats - BoatShare'
      },
      {
        path: '',
        redirectTo: 'users',
        pathMatch: 'full'
      }
    ]
  },

  // Legacy route redirects for backward compatibility
  {
    path: 'register-user',
    redirectTo: 'register',
    pathMatch: 'full'
  },
  {
    path: 'manage-users',
    redirectTo: 'admin/users',
    pathMatch: 'full'
  },
  {
    path: 'manage-boats',
    redirectTo: 'admin/boats',
    pathMatch: 'full'
  },

  // Development/Testing routes (remove in production)
  {
    path: 'test',
    loadComponent: () => import('../testing-page/testing-page.component').then(m => m.TestingPageComponent),
    title: 'Testing - BoatShare'
  },

  // Default routes
  {
    path: '',
    redirectTo: 'dashboard',
    pathMatch: 'full'
  },
  {
    path: '**',
    redirectTo: 'dashboard',
    pathMatch: 'full'
  }
];

export { routes };
