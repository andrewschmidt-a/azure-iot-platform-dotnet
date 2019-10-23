import Config from 'app.config';
import { HttpClient } from 'utilities/httpClient';
import { toTenantModel } from './models';

const IDENTITY_GATEWAY_ENDPOINT = Config.serviceUrls.identityGateway;
const TENANT_MANAGER_ENDPOINT = Config.serviceUrls.tenantManager;

export class TenantService {

  /** Get all tenants for a user */
  static getAllTenants() {
    return HttpClient.get(`${IDENTITY_GATEWAY_ENDPOINT}tenants/all`).map(toTenantModel);
  }

  /** Creates a new tenant */
  static createTenant() {
    return HttpClient.post(`${TENANT_MANAGER_ENDPOINT}tenant`);
  }

  /** Delete a tenant */
  static deleteTenant(id) {
    return HttpClient.delete(`${TENANT_MANAGER_ENDPOINT}tenant/${id}`);
  }

  /** Returns whether a tenant is ready or not */
  static tenantIsDeployed(tenantGuid) {
    return HttpClient.get(`${TENANT_MANAGER_ENDPOINT}tenantready/${tenantGuid}`);
  }

  /** Returns the display value for the tenantGuid */
  static processDisplayValue(tenantGuid) {
    // TODO: Add tenant name setting here in place of this generic value ~ Joe Bethke
    return `tenant#${tenantGuid.substring(0, 5)}`;
  }

  /** Returns information about a tenant */

}
