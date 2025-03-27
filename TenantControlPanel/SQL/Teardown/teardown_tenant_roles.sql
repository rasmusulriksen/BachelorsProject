-- SQL script to tear down all tenant-specific database roles

-- Drop all queue roles for this tenant
DO $$
DECLARE
  role_name text;
BEGIN
  -- Drop queue role pattern: {tenant}_queue_{queue_name}_{role_type}
  -- Queue role types: inserter, processor, discarder
  
  -- Drop queue inserter roles
  FOR role_name IN (SELECT rolname FROM pg_roles WHERE rolname LIKE '@TenantIdentifier\_queue\_%\_inserter')
  LOOP
    EXECUTE format('DROP ROLE IF EXISTS %I', role_name);
    RAISE NOTICE 'Dropped role: %', role_name;
  END LOOP;
  
  -- Drop queue processor roles
  FOR role_name IN (SELECT rolname FROM pg_roles WHERE rolname LIKE '@TenantIdentifier\_queue\_%\_processor')
  LOOP
    EXECUTE format('DROP ROLE IF EXISTS %I', role_name);
    RAISE NOTICE 'Dropped role: %', role_name;
  END LOOP;
  
  -- Drop queue discarder roles
  FOR role_name IN (SELECT rolname FROM pg_roles WHERE rolname LIKE '@TenantIdentifier\_queue\_%\_discarder')
  LOOP
    EXECUTE format('DROP ROLE IF EXISTS %I', role_name);
    RAISE NOTICE 'Dropped role: %', role_name;
  END LOOP;
  
  -- Drop the tenant-specific database user
  EXECUTE format('DROP ROLE IF EXISTS %I', '@TenantIdentifier_user');
  RAISE NOTICE 'Dropped tenant user: @TenantIdentifier_user';
END $$; 