
import { camelCaseReshape } from 'utilities';
export const toUserTenantModel = (response = []) => response.map(user => {
  user = camelCaseReshape(user, {
    'partitionKey': 'id',
    'userId': 'name',
    'type': 'type',
    'roleList': 'role'
  })
  user.role = user.role.join(",")
  user.type = "Member"
  return user;
});
