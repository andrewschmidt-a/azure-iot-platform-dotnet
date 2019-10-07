// Copyright (c) Microsoft. All rights reserved.

import { Observable } from 'rxjs';

import Config from 'app.config';
import { stringify } from 'query-string';
import { HttpClient } from 'utilities/httpClient';
import {
  toUserModel
} from './models';

const ENDPOINT = Config.serviceUrls.auth;

/** Contains methods for calling the Device service */
export class IdentityGatewayService {

  /** Returns a list of devices */
  static getUsers() {
    console.log("GET USERS")
    const data = [{'id':'guid', 'type':'invited', 'name':'Andrew Schmidt'}]//HttpClient.get(`${ENDPOINT}tenant/users`)
      .map(toUserModel);
    console.log(data);
    return Observable.of(data);
  }

}
