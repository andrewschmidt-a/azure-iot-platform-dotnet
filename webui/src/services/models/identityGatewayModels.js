export const toUserModel = (response = []) => response.map(job => camelCaseReshape(job, {
    'id': 'id',
    'name': 'name',
    'type': 'type'
  }));
