// Copyright (c) Microsoft. All rights reserved.

import { withRouter } from 'react-router-dom';
import { connect } from 'react-redux';
import { withNamespaces } from 'react-i18next';

import { AuthService, TenantService } from 'services';
import { getUser } from 'store/reducers/appReducer';
import { Profile } from './profile';
import {
  redux as appRedux,
  epics as appEpics
} from 'store/reducers/appReducer';

import {
  epics as tenantsEpics,
  getTenants,
  getTenantsError,
  getTenantsPendingStatus,
  getTenantsLastUpdated
} from 'store/reducers/tenantsReducer';

function deleteTenantFlow(id, switchId) {
  TenantService.deleteTenant(id)
    .subscribe(
      response => {
        AuthService.switchTenant(switchId);
        console.log(response);
      },
      error => {
        alert("Unable to delete this tenant. You may not have the correct permissions, or it may not be fully deployed yet.")
      },
    );
}

const mapStateToProps = state => ({
  user: getUser(state),
  tenants: getTenants(state),
  tenantsError: getTenantsError(state),
  isPending: getTenantsPendingStatus(state),
  lastUpdated: getTenantsLastUpdated(state)
});

const mapDispatchToProps = dispatch => ({
  fetchTenants: () => dispatch(tenantsEpics.actions.fetchTenants()),
  logout: () => AuthService.logout(),
  switchTenant: (tenant) => AuthService.switchTenant(tenant),
  createTenant: () => TenantService.createTenant(),
  processTenantDisplayValue: (tenant) => TenantService.processDisplayValue(tenant),
  logEvent: diagnosticsModel => dispatch(appEpics.actions.logEvent(diagnosticsModel)),
  deleteTenantThenSwitch: (id, switchId) => deleteTenantFlow(id, switchId)
});

export const ProfileContainer = withRouter(withNamespaces()(connect(mapStateToProps, mapDispatchToProps)(Profile)));
