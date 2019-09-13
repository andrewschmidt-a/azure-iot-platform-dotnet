// Copyright (c) Microsoft. All rights reserved.

import { connect } from 'react-redux';
import { withNamespaces } from 'react-i18next';
import { redux as appRedux, epics as appEpics } from 'store/reducers/appReducer';

import { CreateTenantBtn } from './createTenantBtn';

const mapDispatchToProps = dispatch => ({
    logEvent: diagnosticsModel => dispatch(appEpics.actions.logEvent(diagnosticsModel))
});

export const CreateTenantBtnContainer = withNamespaces()(connect(null, mapDispatchToProps)(CreateTenantBtn));
