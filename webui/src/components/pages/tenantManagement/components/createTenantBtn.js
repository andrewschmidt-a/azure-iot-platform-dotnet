// Copyright (c) Microsoft. All rights reserved.

import React, { Component }  from 'react';

import { Btn } from 'components/shared';
import { svgs } from 'utilities';
import { toDiagnosticsModel } from 'services/models';

export class CreateTenantBtn extends Component {

    onClick = () => {
        this.props.logEvent(toDiagnosticsModel('DeviceGroupManage_Click', {}));
        alert("Create Tenant Placeholder")
    }

    render() {
        return (
            <Btn svg={svgs.plus} onClick={this.onClick}>
                {this.props.t('tenantManagement.create')}
            </Btn>
        );
    }
}
