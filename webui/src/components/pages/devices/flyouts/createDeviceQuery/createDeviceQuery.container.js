// Copyright (c) Microsoft. All rights reserved.

import { connect } from 'react-redux';
import { withNamespaces } from 'react-i18next';
import { CreateDeviceQuery } from './createDeviceQuery';
import { epics as devicesEpics } from 'store/reducers/devicesReducer';
import {
  redux as appRedux,
  epics as appEpics,
  getActiveDeviceQueryConditions,
  getActiveDeviceGroupConditions
} from 'store/reducers/appReducer';

const processQuery = (queryConditions, activeDeviceGroupConditions, dispatch) => {
  const conditions = queryConditions.concat(activeDeviceGroupConditions);
  dispatch(devicesEpics.actions.fetchDevices(conditions));
}

const mapStateToProps = state => ({
  activeDeviceQueryConditions: getActiveDeviceQueryConditions(state),
  activeDeviceGroupConditions: getActiveDeviceGroupConditions(state)
});

const mapDispatchToProps = dispatch => ({
  closeFlyout: () => dispatch(appRedux.actions.setCreateDeviceQueryFlyoutStatus(false)),
  setActiveDeviceQueryConditions: queryConditions => dispatch(appRedux.actions.setActiveDeviceQueryConditions(queryConditions)),
  queryDevices: (queryConditions, activeDeviceGroupConditions) => processQuery(queryConditions, activeDeviceGroupConditions, dispatch),
  insertDeviceGroup: (deviceGroup) => dispatch(appRedux.actions.insertDeviceGroups([deviceGroup])),
  logEvent: diagnosticsModel => dispatch(appEpics.actions.logEvent(diagnosticsModel))
});

export const CreateDeviceQueryContainer = withNamespaces()(connect(mapStateToProps, mapDispatchToProps)(CreateDeviceQuery));
