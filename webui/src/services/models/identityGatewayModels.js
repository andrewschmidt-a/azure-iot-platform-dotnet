
import { camelCaseReshape } from 'utilities';
export const toUserTenantModel = (response = []) => response.map(user => {
  user = camelCaseReshape(user, {
    'id': 'PartitionKey',
    'name': 'PartitionKey',
    'type': 'type',
    'role': 'roleList'
  })
  user.role = user.role.join(",")
  user.type = "Member"
  return user;
});
