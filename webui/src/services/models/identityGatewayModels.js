export const toUserModel = (response = []) => response.map(user => camelCaseReshape(user, {
    'id': 'id',
    'name': 'name',
    'type': 'type'
  }));
