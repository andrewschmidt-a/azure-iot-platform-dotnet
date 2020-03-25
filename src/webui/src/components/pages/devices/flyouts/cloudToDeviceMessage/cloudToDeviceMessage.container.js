// Copyright (c) Microsoft. All rights reserved.

import { connect } from 'react-redux';
import { withNamespaces } from 'react-i18next';
import { CloudToDeviceMessage } from './cloudToDeviceMessage';
import { redux as deviceRedux } from 'store/reducers/devicesReducer';

// Wrap the dispatch method
const mapDispatchToProps = dispatch => ({
});

export const CloudToDeviceMessageContainer = withNamespaces()(connect(null, mapDispatchToProps)(CloudToDeviceMessage));
