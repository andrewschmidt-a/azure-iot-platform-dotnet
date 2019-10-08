
import { camelCaseReshape } from 'utilities';
export const toUserTenantModel = (response = []) => response.map(user => {
  user = camelCaseReshape(user, {
    'id': 'id',
    'name': 'name',
    'type': 'type',
    'role': 'role'
  })
  return user;
});
