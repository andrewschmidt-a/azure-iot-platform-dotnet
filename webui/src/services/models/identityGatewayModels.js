
import { camelCaseReshape } from 'utilities';
export const toUserTenantModel = (response = []) => response.map(user => {
  user = camelCaseReshape(user, {
    'userId': 'id',
    'name': 'name',
    'type': 'type',
    'roleList': 'role'
  })
  user.role = user.role.join(",")
  user.type = "Member"
  return user;
});
