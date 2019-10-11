// Copyright (c) Microsoft. All rights reserved.

import { Observable } from 'rxjs';

import Config from 'app.config';
import { stringify } from 'query-string';
import { HttpClient } from 'utilities/httpClient';
import { v4 as uuidv4 } from 'uuid'
import {
  toUserTenantModel
} from './models';

const ENDPOINT = Config.serviceUrls.identityGateway;

/** Contains methods for calling the Device service */
export class IdentityGatewayService {

  /** Returns a list of devices */
  static getUsers() {
    
    return HttpClient.get(`${ENDPOINT}tenants`)
      .map(toUsersModel);
    return Observable.of(data).map(toUserTenantModel);
  }

  /** Delete a User */
  static deleteUser(id) {
    // Placeholder to call backend
    return Observable.of(id);
  }
  /** Delete a User */
  static invite(email, role) {
    // Placeholder to call backend
    const data = [{id:uuidv4(), name: email, role: role, type: 'Invited' }]

    return Observable.of(data).map(toUserTenantModel);
  }
}
