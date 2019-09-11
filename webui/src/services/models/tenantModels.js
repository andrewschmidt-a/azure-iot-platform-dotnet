import { camelCaseReshape } from 'utilities';

export const toStatusModel = (response = {}) => camelCaseReshape(response, {
  'properties': 'properties'
});
