-- SQL script to create a new tenant in the tenantcontrolpanel database

-- Check if tenant schema exists, if not create it
CREATE SCHEMA IF NOT EXISTS tenant;

-- Create tenant table if it doesn't exist
CREATE TABLE IF NOT EXISTS tenant.tenant (
    id SERIAL PRIMARY KEY,
    tenant_identifier VARCHAR(50) NOT NULL UNIQUE,
    tenant_name VARCHAR(100) NOT NULL,
    tenant_tier VARCHAR(20) NOT NULL,
    database_connectionstring TEXT NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Insert the new tenant
INSERT INTO tenant.tenant (tenant_identifier, tenant_name, tenant_tier, database_connectionstring)
VALUES (@TenantIdentifier, @TenantName, @TenantTier, @DatabaseConnectionString); 