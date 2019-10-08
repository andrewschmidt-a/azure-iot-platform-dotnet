// Copyright (c) Microsoft. All rights reserved.

import { Observable } from 'rxjs';

import Config from 'app.config';
import { stringify } from 'query-string';
import { HttpClient } from 'utilities/httpClient';
import {
  toUserTenantModel
} from './models';

const ENDPOINT = Config.serviceUrls.auth;

/** Contains methods for calling the Device service */
export class IdentityGatewayService {

  /** Returns a list of devices */
  static getUsers() {
    const data = [{'id':'guid', 'name':'Andrew Schmidt'}, {'id':'', 'name':'Kyle Estes'}]//HttpClient.get(`${ENDPOINT}tenant/users`)
      .map(toUserTenantModel);
    console.log(data);
    return Observable.of(data);
  }

  /** Delete a User */
  static deleteUser(id) {
    // Placeholder to call backend
    return Observable.of(id);
  }
}
