import { OAuthService } from 'angular-oauth2-oidc';
import { HttpClient } from 'utilities/httpClient';
import { Observable } from 'rxjs';

import authConfig from './auth.config.js';


class Auth {

  constructor() {
    this.oauth = new OAuthService();
    this.oauth.http = HttpClient;
    this.oauth.configure(authConfig);
    this.oauth.loadDiscoveryDocument(authConfig.issuer + '/.well-known/openid-configuration');
  }

  login() {
    this.oauth.initImplicitFlow();
  }

  getAccessToken() {
    return Observable.of(this.oauth.getAccessToken());
  }
}

var AuthService = new Auth();
console.log(AuthService)
export default AuthService;
