// Copyright (c) Microsoft. All rights reserved.

import { connect } from 'react-redux';
import { withNamespaces } from 'react-i18next';
import { UserDelete } from './userDelete';
import { redux as deviceRedux } from 'store/reducers/devicesReducer';

// Wrap the dispatch method
const mapDispatchToProps = dispatch => ({
  deleteDevices: deviceIds => dispatch(deviceRedux.actions.deleteDevices(deviceIds))
});

export const UserDeleteContainer = withNamespaces()(connect(null, mapDispatchToProps)(UserDelete));
