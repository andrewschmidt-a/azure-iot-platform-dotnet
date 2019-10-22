
import { TenantService } from 'services';

export const toTenantModel = (response = { Models: [] }) => response.Models.map(model => {
  return{
    id: model.tenantId,
    displayName: TenantService.processDisplayValue(model.tenantId),
    role: model.roles.includes('admin') ? 'Admin' : 'Read Only',
  };
});
