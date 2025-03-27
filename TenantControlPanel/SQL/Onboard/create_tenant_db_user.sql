-- Create a tenant-specific database user with limited permissions
DO $$
BEGIN
  -- Create user if it doesn't exist
  IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = '@TenantIdentifier_user') THEN
    CREATE USER "@TenantIdentifier_user" WITH PASSWORD '@TenantPassword';
  END IF;
  
  -- Grant connect permission on the database
  GRANT CONNECT ON DATABASE "@TenantIdentifier" TO "@TenantIdentifier_user";
  
  -- Grant usage on schemas
  GRANT USAGE ON SCHEMA notification TO "@TenantIdentifier_user";
  GRANT USAGE ON SCHEMA queues TO "@TenantIdentifier_user";
  
  -- Grant table permissions
  GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA notification TO "@TenantIdentifier_user";
  GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA queues TO "@TenantIdentifier_user";
  
  -- Grant permissions on future tables
  ALTER DEFAULT PRIVILEGES IN SCHEMA notification GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO "@TenantIdentifier_user";
  ALTER DEFAULT PRIVILEGES IN SCHEMA queues GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO "@TenantIdentifier_user";
  
  -- Grant execute on functions
  GRANT EXECUTE ON ALL FUNCTIONS IN SCHEMA queues TO "@TenantIdentifier_user";
  ALTER DEFAULT PRIVILEGES IN SCHEMA queues GRANT EXECUTE ON FUNCTIONS TO "@TenantIdentifier_user";
END $$;
