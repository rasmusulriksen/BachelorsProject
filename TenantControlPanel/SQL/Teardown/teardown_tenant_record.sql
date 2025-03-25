-- SQL script to tear down a tenant record from the tenant control panel database

-- Delete the tenant record
DELETE FROM tenant.tenant WHERE tenant_identifier = @TenantIdentifier; 