// Copyright (c) Microsoft. All rights reserved.

import Config from 'app.config';
// import AuthenticationContext from 'adal-angular/dist/adal.min.js'
import { UserManager, UserManagerSettings, User, WebStorageStateStore, } from 'oidc-client';
import { Observable } from 'rxjs';
import { HttpClient } from 'utilities/httpClient';
import { toUserModel, authDisabledUser } from './models';
import{ Policies } from './policies.json'

const ENDPOINT = Config.serviceUrls.auth;
function getQueryStringValue (key) {
  return decodeURIComponent(window.location.search.replace(new RegExp("^(?:.*[&\\?]" + encodeURIComponent(key).replace(/[\.\+\*]/g, "\\$&") + "(?:\\=([^&]*))?)?.*$", "i"), "$1"));
}

export class AuthService {

  static authContext; // Created on AuthService.initialize()
  static authEnabled = true;
  static aadInstance = '';
  static appId = '00000000-0000-0000-0000-000000000000';
  static tenantId = '00000000-0000-0000-0000-000000000000';
  static clientId = '00000000-0000-0000-0000-000000000000';
  static issuer = '';
  static _userManager = null;
  static settings = { };

  static initialize() {
    if (typeof global.DeploymentConfig === 'undefined') {
      alert('The dashboard configuration is missing.\n\nVerify the content of webui-config.js.');
      throw new Error('The global configuration is missing. Verify the content of webui-config.js.');
    }

    // if (typeof global.DeploymentConfig.authEnabled !== 'undefined') {
    //   AuthService.authEnabled = global.DeploymentConfig.authEnabled;
    //   if (!AuthService.authEnabled) {
    //     console.warn('Auth is disabled! (see webui-config.js)');
    //   }
    // }

    // AuthService.tenantId = global.DeploymentConfig.aad.tenant;
    // AuthService.clientId = global.DeploymentConfig.aad.appId;
    // AuthService.appId = global.DeploymentConfig.aad.appId;
    // AuthService.aadInstance = global.DeploymentConfig.aad.instance;
    // AuthService.issuer = global.DeploymentConfig.issuer;

    // if (AuthService.aadInstance && AuthService.aadInstance.endsWith('{0}')) {
    //   AuthService.aadInstance = AuthService.aadInstance.substr(0, AuthService.aadInstance.length - 3);
    // }

    // // TODO: support multiple types/providers
    // if (AuthService.isEnabled() && global.DeploymentConfig.authType !== 'aad') {
    //   throw new Error(`Unknown auth type: ${global.DeploymentConfig.authType}`);
    // }

    // AuthService.authContext = new AuthenticationContext({
    //   instance: AuthService.aadInstance,
    //   tenant: AuthService.tenantId,
    //   clientId: AuthService.clientId,
    //   redirectUri: window.location.origin,
    //   expireOffsetSeconds: 300, // default is 120
    //   postLogoutRedirectUri: window.location.origin
    // });

    AuthService.settings = {
      authority: 'https://crsliotkubedev.centralus.cloudapp.azure.com/auth/',
      client_id: 'IoTPlatform',
      redirect_uri: window.location.origin,
      post_logout_redirect_uri: window.location.origin,
      response_type: 'id_token',
      response_mode: 'fragment',
      userStore: new WebStorageStateStore({ store: window.localStorage })
    };
    AuthService._userManager = new UserManager(AuthService.settings);
  }

  static isCallback(hash) {
    return (
      hash.includes('id_token') ||
      hash.includes('access_token') ||
      hash.includes('error_description')
    );
  }

  static isDisabled() {
    return AuthService.authEnabled === false;
  }

  static isEnabled() {
    return !AuthService.isDisabled();
  }

  static onLoad(successCallback) {
    AuthService.initialize();
    if (AuthService.isDisabled()) {
      console.debug('Skipping Auth onLoad because Auth is disabled');
      if (successCallback) successCallback();
      return;
    };
    console.log("Loading Auth....")
    // atempt to sign in if the current window is not a callback
    if (!AuthService.isCallback(window.location.hash)) {
      console.log("is not a callback.. attempt to find user")
      AuthService.getCurrentUser().subscribe(user => {
        if (user) {
          console.log(user);
          console.log(`Signed in as ${user.name} with ${user.email}`);
          if (successCallback) successCallback();
        } else {
          console.log('The user is not signed in');
          AuthService._userManager.signinRedirect({state: getQueryStringValue('tenant')});
        }
      });
    }else{
      window.location.hash = decodeURIComponent(window.location.hash); // decode hash
      AuthService._userManager.signinRedirectCallback().then(user=>{
        console.log("User state Log at callback")
        console.log(user.state)
        window.location = window.location.origin
      })
    }

    // Note: "window.location.hash" is the anchor part attached by
    //       the Identity Provider when redirecting the user after
    //       a successful authentication.
    // if (AuthService.authContext.isCallback(window.location.hash)) {
    //   console.debug('Handling Auth Window callback');
    //   // Handle redirect after authentication
    //   AuthService.authContext.handleWindowCallback();
    //   const error = AuthService.authContext.getLoginError();
    //   if (error) {
    //     throw new Error(`Authentication Error: ${error}`);
    //   }
    // } else {
    //   AuthService.getUserName(user => {
    //     if (user) {
    //       console.log(`Signed in as ${user.Name} with ${user.Email}`);
    //       if (successCallback) successCallback();
    //     } else {
    //       console.log('The user is not signed in');
    //       AuthService.authContext.login();
    //     }
    //   });
    // }
  }

  static getUserName(callback) {
    if (AuthService.isDisabled()) return;

    if (AuthService._userManager.getUser()) {
      Observable.of({ Name: 'Temp Name', Email: 'temp.name@contoso.com' })
        .map(data => data ? { Name: data.Name, Email: data.Email } : null)
        .subscribe(callback);
    } else {
      console.log('The user is not signed in');
      AuthService._userManager.signinRedirect();
    }
    // if (AuthService.authContext.getCachedUser()) {
    //   Observable.of({ Name: 'Temp Name', Email: 'temp.name@contoso.com' })
    //     .map(data => data ? { Name: data.Name, Email: data.Email } : null)
    //     .subscribe(callback);
    // } else {
    //   console.log('The user is not signed in');
    //   AuthService.authContext.login();
    // }
  }

  /** Returns a the current user */
  static getCurrentUser() {
    if (AuthService.isDisabled()) {
      return Observable.of(authDisabledUser);
    }

    return Observable.create(observer => {
      return AuthService._userManager.getUser().then(user => {
        if (user) {
          // Following two should be Arrays but claims will return a string if only one value.
          var roles = typeof(user.profile.role) == 'string'?[user.profile.role]:user.profile.role;
          var availableTenants = typeof(user.profile.available_tenants) == 'string'?[user.profile.available_tenants]:user.profile.available_tenants

          // Followed format but really shouldnt the data structure be a Map? not a list? -- Andrew Schmidt
          var flattenedPermissions = Policies.filter(policy => roles.indexOf(policy.Role) > -1).map(policy => policy.AllowedActions).flat();
          observer.next(toUserModel({
            id: user.profile.sub,
            name: user.profile.name,
            email: "a90q9zz@mmm.com",
            roles: roles,
            allowedActions: flattenedPermissions,
            tenant: user.profile.tenant,
            availableTenants: availableTenants
          }));
        } else {
          observer.next(null);
        }
        observer.complete();
      });
    });
  }

  static logout() {
    if (AuthService.isDisabled()) return;

    AuthService._userManager.signoutRedirect();
  }

  /**
   * Acquires token from the cache if it is not expired.
   * Otherwise sends request to AAD to obtain a new token.
   */
  static getAccessToken() {
    if (AuthService.isDisabled()) {
      return Observable.of('client-auth-disabled');
    }

    return Observable.create(observer => {
      return AuthService._userManager.getUser().then(user => {
        if (user) {
          observer.next(user.id_token);
        } else {
          console.log('Authentication Error while Aquiring Access Token');
          observer.error('Authentication Error while Aquiring Access Token');
        }
        observer.complete();
      });
    });
    // return Observable.create(observer => {
    //   return AuthService.authContext.acquireToken(
    //     AuthService.appId,
    //     (error, accessToken) => {
    //       if (error) {
    //         console.log(`Authentication Error: ${error}`);
    //         observer.error(error);
    //       }
    //       else observer.next(accessToken);
    //       observer.complete();
    //     }
    //   );
    // });
  }
}
