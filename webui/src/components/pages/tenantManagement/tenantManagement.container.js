// Copyright (c) Microsoft. All rights reserved.

import { connect } from 'react-redux';
import { withNamespaces } from 'react-i18next';
import { TenantManagement } from './tenantManagement';

const mapStateToProps = state => ({
});


export const TenantManagementContainer = withNamespaces()(connect(mapStateToProps)(TenantManagement));
