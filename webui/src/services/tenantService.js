import Config from 'app.config';
import { HttpClient } from 'utilities/httpClient';

const ENDPOINT = Config.serviceUrls.tenantManager;

export class TenantService {

  /** Creates a new tenant */
  static createTenant() {
    var response = HttpClient.post(`${ENDPOINT}api/tenant`);
    console.log("Create tenant response: ", response);
    return response;
  }

  /** Returns information about a tenant */

  /** Returns whether a tenant is ready or not */
}
