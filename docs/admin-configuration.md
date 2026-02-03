# Administrator Configuration Guide

This guide explains how to configure administrator access for the IoT Hub Portal.

## Default Admin User Behavior

The IoT Hub Portal has built-in support for automatically granting administrator privileges to users. This happens in two scenarios:

### 1. First User (Automatic)

When the portal is first deployed and has no users, the **first user to log in** will automatically be granted full administrator privileges. This ensures that there is always at least one administrator who can manage the system.

### 2. Configured Global Admins

You can pre-configure specific email addresses that should automatically receive administrator privileges when they first log in to the portal.

## Configuration

### GlobalAdminEmails Setting

Add the `GlobalAdminEmails` configuration setting to specify which users should automatically receive administrator privileges.

**Format**: Comma-separated list of email addresses

**Example**:
```json
{
  "GlobalAdminEmails": "admin@company.com,admin2@company.com,superadmin@company.com"
}
```

### Configuration Methods

#### Azure App Service Configuration

For Azure deployments, add this as an Application Setting:

```
Name: GlobalAdminEmails
Value: admin@company.com,admin2@company.com
```

#### AWS Environment Variables

For AWS deployments, set this as an environment variable:

```bash
export GlobalAdminEmails="admin@company.com,admin2@company.com"
```

#### appsettings.json (Development)

For local development, add to your `appsettings.json`:

```json
{
  "GlobalAdminEmails": "developer@company.com",
  "PortalName": "IoT Hub Portal",
  ...
}
```

## How It Works

1. **User Authentication**: When a user successfully authenticates with the portal
2. **User Lookup**: The system checks if a user record exists for their email address
3. **New User Creation**: If no user record exists, a new user is created
4. **Admin Check**: The system then checks if the user should be granted admin privileges by:
   - Checking if this is the first user in the system (no other users exist)
   - OR checking if their email address is in the `GlobalAdminEmails` configuration
5. **Role Assignment**: If either condition is true, the user is automatically assigned to the "Administrators" role with full ("*") scope

## Administrators Role

The "Administrators" role is created automatically through database migrations and includes all available permissions:

- User management (read/write)
- Role management (read/write)
- Access control management (read/write)
- Device management (read/write/import/export)
- Device model management (read/write)
- Edge device management (read/write/execute)
- Edge model management (read/write)
- Device configuration management (read/write)
- Device tag management (read/write)
- Layer management (read/write)
- Planning management (read/write)
- Schedule management (read/write)
- Concentrator management (read/write)
- Dashboard access (read)
- Settings access (read)
- Ideas submission (write)

## Security Considerations

1. **Email Validation**: Email addresses are compared case-insensitively
2. **One-Time Assignment**: Admin privileges are only assigned when a new user first logs in
3. **Persistent Access**: Once assigned, admin privileges persist until manually removed
4. **Configuration Security**: Protect your configuration files and environment variables as they control who gets admin access
5. **Audit Trail**: All access control assignments are logged in the database

## Troubleshooting

### "Unauthorized Access" Error After Login

If you see an "Unauthorized Access" error after successfully logging in, it means:

1. You are not the first user (someone else logged in before you)
2. Your email is not in the `GlobalAdminEmails` configuration
3. You have not been assigned any roles by an existing administrator

**Solution**: 
- Add your email address to the `GlobalAdminEmails` configuration
- Restart the application
- Log out and log in again
- Your account should now have administrator privileges

### Verifying Configuration

To verify your configuration is working:

1. Check that the `GlobalAdminEmails` setting is present in your configuration
2. Check that your email address is correctly spelled in the list
3. Ensure there are no extra spaces around email addresses (they are automatically trimmed)
4. Check the application logs for any errors during user creation

### Checking the Administrators Role

The "Administrators" role should be created automatically by the database migrations. If it's missing:

1. Check that all database migrations have been applied
2. Look for the `AddDefaultAdminRole` migration in the migrations list
3. If missing, the migration may need to be manually applied

## Managing Administrators

After the initial setup, administrators can be managed through the portal's user management interface:

1. Navigate to the Users section
2. Select a user
3. Assign or remove the "Administrators" role
4. Set the appropriate scope ("*" for global access)

## Best Practices

1. **Limit Admin Count**: Only configure essential admin email addresses in `GlobalAdminEmails`
2. **Use Role-Based Access**: For most users, assign specific roles with limited permissions rather than full admin access
3. **Regular Audits**: Periodically review who has administrator access
4. **Document Admins**: Keep a record of who is configured as a global admin and why
5. **Secure Configuration**: Store configuration securely and limit access to configuration files
