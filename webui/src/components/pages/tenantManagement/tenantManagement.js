// Copyright (c) Microsoft. All rights reserved.

import React, { Component } from 'react';
import { Route, Redirect, Switch } from 'react-router-dom';
import { schema, normalize } from 'normalizr';
import update from 'immutability-helper';
import moment from 'moment';

import Config from 'app.config';
import {TenantGrid} from './grids'
import { ROW_HEIGHT } from 'components/shared/pcsGrid/pcsGridConfig';

import './tenantManagement.scss';
import {
  ComponentArray,
  ContextMenu,
  ContextMenuAlign,
  RefreshBarContainer as RefreshBar,
  PageContent,
  PageTitle,
  Protected
} from 'components/shared';
import {CreateTenantBtnContainer} from "./components";

// TODO: Refactor some of the naming in this file related to rules, alert, and alerts
export class TenantManagement extends Component {

  constructor(props) {
    super(props);

    this.state = {
        table:[
            {status:"Deploying", name:"TestTenant"}
        ]
    };


  }

  render() {

      const {
          t
      } = this.props;
    return (
      <Switch>
        <Route exact path={'/tenantmanagement'}
          render={() =>
              <ComponentArray>
                <ContextMenu>
                  <ContextMenuAlign left={true}>
                  </ContextMenuAlign>
                  <ContextMenuAlign>
                      <CreateTenantBtnContainer />
                  </ContextMenuAlign>
                </ContextMenu>
                <PageContent className="maintenance-container summary-container">
                  <PageTitle titleValue={this.props.t('tenantManagement.title')} />
                    <TenantGrid
                        t={t}
                        style={{ height: 2 * ROW_HEIGHT + 2 }}
                        onGridReady={this.onRuleGridReady}
                        // onContextMenuChange={this.onContextMenuChange('ruleContextBtns')}
                        // onHardSelectChange={this.onHardSelectChange('rules')}
                        rowData={this.state.table}
                        pagination={false}
                        //refresh={this.props.fetchRules}
                        logEvent={this.props.logEvent} />
                    <br/>
                </PageContent>
              </ComponentArray>
          } />
        
      </Switch>
    );
  }

};
