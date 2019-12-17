import Config from 'app.config';
import { HttpClient } from 'utilities/httpClient';

const TENANT_MANAGER_ENDPOINT = Config.serviceUrls.tenantManager;

export class AlertingService {

  static addAlerting() {
    return HttpClient.post(`${TENANT_MANAGER_ENDPOINT}alerting`);
  }

  static startAlerting() {
    return HttpClient.post(`${TENANT_MANAGER_ENDPOINT}alerting/start`);
  }

  static getAlerting() {
    return HttpClient.post(`${TENANT_MANAGER_ENDPOINT}alerting`);
  }

  static tenantHasAlerting() {
    try
    {
      var model = this.getAlerting()
      if (model.SAJobName)
      {
        return true;
      }
      else
      {
        return false;
      }
    }
    catch (error)
    {
      return false;
    }
  }

  static stopAlerting() {
    return HttpClient.post(`${TENANT_MANAGER_ENDPOINT}alerting/stop`);
  }

  static alertingIsActive() {
    var model = this.getAlerting();
    return model.IsActive;
  }
}
