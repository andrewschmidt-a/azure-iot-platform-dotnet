
import { camelCaseReshape } from 'utilities';
import {Policies} from 'utilities'
export const toUserTenantModel = (response = []) => response.map(user => {
  user = camelCaseReshape(user, {
    'userId': 'id',
    'name': 'name',
    'type': 'type',
    'roleList': 'role'
  })
  user.role = user.role.map(r => Policies.filter(p=> p.Role == r).concat({DisplayName:"No Roles"})[0].DisplayName).join(",")
  return user;
});
