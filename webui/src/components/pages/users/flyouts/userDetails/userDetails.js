// Copyright (c) Microsoft. All rights reserved.

import React, { Component } from 'react';
import { Trans } from 'react-i18next';
import { Subject } from 'rxjs';
import moment from 'moment';

import Config from 'app.config';
import { TelemetryService } from 'services';
import { UserIcon } from './userIcon';
import { RulesGrid, rulesColumnDefs } from 'components/pages/rules/rulesGrid';
import {
  copyToClipboard,
  int,
  svgs,
  translateColumnDefs,
  DEFAULT_TIME_FORMAT
} from 'utilities';
import {
  Btn,
  BtnToolbar,
  ComponentArray,
  ErrorMsg,
  PropertyGrid as Grid,
  PropertyGridBody as GridBody,
  PropertyGridHeader as GridHeader,
  PropertyRow as Row,
  PropertyCell as Cell,
  SectionDesc,
  TimeSeriesInsightsLinkContainer
} from 'components/shared';
import Flyout from 'components/shared/flyout';
import { TelemetryChartContainer as TelemetryChart, chartColorObjects } from 'components/pages/dashboard/panels/telemetry';
import { transformTelemetryResponse } from 'components/pages/dashboard/panels';
import { getEdgeAgentStatusCode } from 'utilities';

import './userDetails.scss';

const Section = Flyout.Section;

export class UserDetails extends Component {
  constructor(props) {
    super(props);

    this.state = {
      alerts: undefined,
      isAlertsPending: false,
      alertsError: undefined,

      telemetry: {},
      telemetryIsPending: true,
      telemetryError: null,

      showRawMessage: false,
      currentModuleStatus: undefined
    };
    this.baseState = this.state;
    this.columnDefs = [
      {
        ...rulesColumnDefs.ruleName,
        cellRendererFramework: undefined // Don't allow soft select from an open flyout
      },
      rulesColumnDefs.severity,
      rulesColumnDefs.alertStatus,
      rulesColumnDefs.explore
    ];

    this.resetTelemetry$ = new Subject();
    this.telemetryRefresh$ = new Subject();
    if (this.props.moduleStatus) {
      this.state = {
        ...this.state,
        currentModuleStatus: this.props.moduleStatus
      };
    } else {
      this.props.fetchModules(this.props.user.id);
    }
  }

  componentDidMount() {
    if (!this.props.rulesLastUpdated) this.props.fetchRules();

    const {
      user = {},
      user: {
        telemetry: {
          interval = '0'
        } = {}
      } = {}
    } = this.props;

    const userId = user.id;
    this.fetchAlerts(userId);

    const [hours = 0, minutes = 0, seconds = 0] = interval.split(':').map(int);
    const refreshInterval = ((((hours * 60) + minutes) * 60) + seconds) * 1000;

    // Telemetry stream - START
    const onPendingStart = () => this.setState({ telemetryIsPending: true });

    const telemetry$ =
      this.resetTelemetry$
        .do(_ => this.setState({ telemetry: {} }))
        .switchMap(userId =>
          TelemetryService.getTelemetryByUserIdP15M([userId])
            .merge(
              this.telemetryRefresh$ // Previous request complete
                .delay(refreshInterval || Config.dashboardRefreshInterval) // Wait to refresh
                .do(onPendingStart)
                .flatMap(_ => TelemetryService.getTelemetryByUserIdP1M([userId]))
            )
            .flatMap(messages =>
              transformTelemetryResponse(() => this.state.telemetry)(messages)
                .map(telemetry => ({ telemetry, lastMessage: messages[0] }))
            )
            .map(newState => ({ ...newState, telemetryIsPending: false })) // Stream emits new state
        )
    // Telemetry stream - END

    this.telemetrySubscription = telemetry$.subscribe(
      telemetryState => this.setState(
        telemetryState,
        () => this.telemetryRefresh$.next('r')
      ),
      telemetryError => this.setState({ telemetryError, telemetryIsPending: false })
    );

    this.resetTelemetry$.next(userId);
  }

  componentWillReceiveProps(nextProps) {
    const {
      userModuleStatus,
      isUserModuleStatusPending,
      userModuleStatusError,
      moduleStatus,
      resetPendingAndError,
      user,
      fetchModules
    } = nextProps;
    let tempState = {};
    /*
      userModuleStatus is a prop fetched by making fetchModules() API call through userDetails.container on demand.
      moduleStatus is a prop sent from deploymentDetailsGrid which it already has in rowData.
      Both userModuleStatus and moduleStatus have the same content,
        but come from different sources based on the page that opens this flyout.
      Depending on which one is available, currentModuleStatus is set in component state.
    */

    if ((this.props.user || {}).id !== user.id) {
      // Reset state if the user changes.
      resetPendingAndError();
      tempState = { ...this.baseState };

      if (moduleStatus) {
        // If moduleStatus exist in props, set it in state.
        tempState = {
          ...tempState,
          currentModuleStatus: moduleStatus
        };
      } else {
        // Otherwise make an API call to get userModuleStatus.
        fetchModules(user.id);
      }

      const userId = (user || {}).id;
      this.resetTelemetry$.next(userId);
      this.fetchAlerts(userId);
    } else if (!moduleStatus && !isUserModuleStatusPending && !userModuleStatusError) {
      // set userModuleStatus in state, if moduleStatus doesn't exist and usersReducer successfully received the API response.
      tempState = { currentModuleStatus: userModuleStatus };
    }

    if (Object.keys(tempState).length) this.setState(tempState);
  }

  componentWillUnmount() {
    this.alertSubscription.unsubscribe();
    this.telemetrySubscription.unsubscribe();
  }

  copyUserPropertiesToClipboard = () => {
    if (this.props.user) {
      copyToClipboard(JSON.stringify(this.props.user.properties || {}));
    }
  }

  toggleRawDiagnosticsMessage = () => {
    this.setState({ showRawMessage: !this.state.showRawMessage });
  }

  applyRuleNames = (alerts, rules) =>
    alerts.map(alert => ({
      ...alert,
      name: (rules[alert.ruleId] || {}).name
    }));

  fetchAlerts = (userId) => {
    this.setState({ isAlertsPending: true });

    this.alertSubscription = TelemetryService.getAlerts({
      limit: 5,
      order: "desc",
      users: userId
    })
      .subscribe(
        alerts => this.setState({ alerts, isAlertsPending: false, alertsError: undefined }),
        alertsError => this.setState({ alertsError, isAlertsPending: false })
      );
  }

  render() {
    const {
      t,
      onClose,
      user,
      theme,
      timeSeriesExplorerUrl,
      isUserModuleStatusPending,
      userModuleStatusError
    } = this.props;
    const { telemetry, lastMessage, currentModuleStatus } = this.state;
    const lastMessageTime = (lastMessage || {}).time;
    const isPending = this.state.isAlertsPending && this.props.isRulesPending;
    const rulesGridProps = {
      rowData: isPending ? undefined : this.applyRuleNames(this.state.alerts || [], this.props.rules || []),
      t: this.props.t,
      userGroups: this.props.userGroups,
      domLayout: 'autoHeight',
      columnDefs: translateColumnDefs(this.props.t, this.columnDefs),
      suppressFlyouts: true
    };
    const tags = Object.entries(user.tags || {});
    const properties = Object.entries(user.properties || {});
    const moduleQuerySuccessful = currentModuleStatus &&
      currentModuleStatus !== {} &&
      !isUserModuleStatusPending &&
      !userModuleStatusError;
    // Add parameters to Time Series Insights Url

    const timeSeriesParamUrl =
      timeSeriesExplorerUrl
        ? timeSeriesExplorerUrl +
        `&relativeMillis=1800000&timeSeriesDefinitions=[{"name":"${user.id}","measureName":"${Object.keys(telemetry).sort()[0]}","predicate":"'${user.id}'"}]`
        : undefined;

    return (
      <Flyout.Container header={t('users.flyouts.details.title')} t={t} onClose={onClose}>
        <div className="user-details-container">
          {
            !user &&
            <div className="user-details-container">
              <ErrorMsg>{t("users.flyouts.details.noUser")}</ErrorMsg>
            </div>
          }
          {
            !!user &&
            <div className="user-details-container">

              <Grid className="user-details-header">
                <Row>
                  <Cell className="col-3"><UserIcon type={user.type} /></Cell>
                  <Cell className="col-7">
                    <div className="user-name">{user.id}</div>
                    <div className="user-simulated">{user.isSimulated ? t('users.flyouts.details.simulated') : t('users.flyouts.details.notSimulated')}</div>
                    <div className="user-connected">{user.connected ? t('users.flyouts.details.connected') : t('users.flyouts.details.notConnected')}</div>
                  </Cell>
                </Row>
              </Grid>

              {
                (!this.state.isAlertsPending && this.state.alerts && (this.state.alerts.length > 0))
                && <RulesGrid {...rulesGridProps} />
              }

              <Section.Container>
                <Section.Header>{t('users.flyouts.details.telemetry.title')}</Section.Header>
                <Section.Content>
                  {
                    timeSeriesExplorerUrl &&
                    <TimeSeriesInsightsLinkContainer href={timeSeriesParamUrl} />
                  }
                  <TelemetryChart className="telemetry-chart" telemetry={telemetry} theme={theme} colors={chartColorObjects} />
                </Section.Content>
              </Section.Container>

              <Section.Container>
                <Section.Header>{t('users.flyouts.details.tags.title')}</Section.Header>
                <Section.Content>
                  <SectionDesc>
                    <Trans i18nKey={"users.flyouts.details.tags.description"}>
                      To edit, close this panel, click on
                      <strong>{{ jobs: t('users.flyouts.jobs.title') }}</strong>
                      then select
                      <strong>{{ tags: t('users.flyouts.jobs.tags.radioLabel') }}</strong>.
                    </Trans>
                  </SectionDesc>
                  {
                    (tags.length === 0) &&
                    t('users.flyouts.details.tags.noneExist')
                  }
                  {
                    (tags.length > 0) &&
                    <Grid>
                      <GridHeader>
                        <Row>
                          <Cell className="col-3">{t('users.flyouts.details.tags.keyHeader')}</Cell>
                          <Cell className="col-7">{t('users.flyouts.details.tags.valueHeader')}</Cell>
                        </Row>
                      </GridHeader>
                      <GridBody>
                        {
                          tags.map(([tagName, tagValue], idx) =>
                            <Row key={idx}>
                              <Cell className="col-3">{tagName}</Cell>
                              <Cell className="col-7">{tagValue.toString()}</Cell>
                            </Row>
                          )
                        }
                      </GridBody>
                    </Grid>
                  }
                </Section.Content>
              </Section.Container>

              <Section.Container>
                <Section.Header>{t('users.flyouts.details.methods.title')}</Section.Header>
                <Section.Content>
                  <SectionDesc>
                    <Trans i18nKey={"users.flyouts.details.methods.description"}>
                      To edit, close this panel, click on
                      <strong>{{ jobs: t('users.flyouts.jobs.title') }}</strong>
                      then select
                      <strong>{{ methods: t('users.flyouts.jobs.methods.radioLabel') }}</strong>.
                    </Trans>
                  </SectionDesc>
                  {
                    (user.methods.length === 0)
                      ? t('users.flyouts.details.methods.noneExist')
                      :
                      <Grid>
                        {
                          user.methods.map((methodName, idx) =>
                            <Row key={idx}>
                              <Cell>{methodName}</Cell>
                            </Row>
                          )
                        }
                      </Grid>
                  }
                </Section.Content>
              </Section.Container>

              <Section.Container>
                <Section.Header>{t('users.flyouts.details.properties.title')}</Section.Header>
                <Section.Content>
                  <SectionDesc>
                    <Trans i18nKey={"users.flyouts.details.properties.description"}>
                      To edit, close this panel, click on
                      <strong>{{ jobs: t('users.flyouts.jobs.title') }}</strong>
                      then select
                      <strong>{{ properties: t('users.flyouts.jobs.properties.radioLabel') }}</strong>.
                    </Trans>
                  </SectionDesc>
                  {
                    (properties.length === 0) &&
                    t('users.flyouts.details.properties.noneExist')
                  }
                  {
                    (properties.length > 0) &&
                    <ComponentArray>
                      <Grid>
                        <GridHeader>
                          <Row>
                            <Cell className="col-3">{t('users.flyouts.details.properties.keyHeader')}</Cell>
                            <Cell className="col-7">{t('users.flyouts.details.properties.valueHeader')}</Cell>
                          </Row>
                        </GridHeader>
                        <GridBody>
                          {
                            properties.map(([propertyName, propertyValue], idx) => {
                              const desiredPropertyValue = user.desiredProperties[propertyName];
                              const displayValue = !desiredPropertyValue || propertyValue === desiredPropertyValue
                                ? propertyValue.toString()
                                : t('users.flyouts.details.properties.syncing', { reportedPropertyValue: propertyValue.toString(), desiredPropertyValue: desiredPropertyValue.toString() });
                              return (
                                <Row key={idx}>
                                  <Cell className="col-3">{propertyName}</Cell>
                                  <Cell className="col-7">{displayValue}</Cell>
                                </Row>
                              );
                            })
                          }
                        </GridBody>
                      </Grid>
                      <Grid className="user-properties-actions">
                        <Row>
                          <Cell className="col-8">{t('users.flyouts.details.properties.copyAllProperties')}</Cell>
                          <Cell className="col-2"><Btn svg={svgs.copy} onClick={this.copyUserPropertiesToClipboard} >{t('users.flyouts.details.properties.copy')}</Btn></Cell>
                        </Row>
                      </Grid>
                    </ComponentArray>
                  }
                </Section.Content>
              </Section.Container>

              <Section.Container>
                <Section.Header>{t('users.flyouts.details.diagnostics.title')}</Section.Header>
                <Section.Content>
                  <SectionDesc>{t('users.flyouts.details.diagnostics.description')}</SectionDesc>

                  <Grid className="user-details-diagnostics">
                    <GridHeader>
                      <Row>
                        <Cell className="col-3">{t('users.flyouts.details.diagnostics.keyHeader')}</Cell>
                        <Cell className="col-7">{t('users.flyouts.details.diagnostics.valueHeader')}</Cell>
                      </Row>
                    </GridHeader>
                    <GridBody>
                      <Row>
                        <Cell className="col-3">{t('users.flyouts.details.diagnostics.status')}</Cell>
                        <Cell className="col-7">{user.connected ? t('users.flyouts.details.connected') : t('users.flyouts.details.notConnected')}</Cell>
                      </Row>
                      {
                        user.connected &&
                        <ComponentArray>
                          <Row>
                            <Cell className="col-3">{t('users.flyouts.details.diagnostics.lastMessage')}</Cell>
                            <Cell className="col-7">{lastMessageTime ? moment(lastMessageTime).format(DEFAULT_TIME_FORMAT) : '---'}</Cell>
                          </Row>
                          <Row>
                            <Cell className="col-3">{t('users.flyouts.details.diagnostics.message')}</Cell>
                            <Cell className="col-7">
                              <Btn className="raw-message-button" onClick={this.toggleRawDiagnosticsMessage}>{t('users.flyouts.details.diagnostics.showMessage')}</Btn>
                            </Cell>
                          </Row>
                        </ComponentArray>
                      }
                      {
                        this.state.showRawMessage &&
                        <Row>
                          <pre>{JSON.stringify(lastMessage, null, 2)}</pre>
                        </Row>
                      }
                    </GridBody>
                  </Grid>

                </Section.Content>
              </Section.Container>

              <Section.Container>
                <Section.Header>{t('users.flyouts.details.modules.title')}</Section.Header>
                <Section.Content>
                  <SectionDesc>
                    {t("users.flyouts.details.modules.description")}
                  </SectionDesc>
                  <div className="user-details-deployment-contentbox">
                    {
                      !moduleQuerySuccessful &&
                      t('users.flyouts.details.modules.noneExist')
                    }
                    {
                      moduleQuerySuccessful &&
                      <ComponentArray >
                        <div>{currentModuleStatus.code}: {getEdgeAgentStatusCode(currentModuleStatus.code, t)}</div>
                        <div>{currentModuleStatus.description}</div>
                      </ComponentArray >
                    }
                  </div>
                </Section.Content>
              </Section.Container>
            </div>
          }
          <BtnToolbar>
            <Btn svg={svgs.cancelX} onClick={onClose}>{t('users.flyouts.details.close')}</Btn>
          </BtnToolbar>
        </div>
      </Flyout.Container>
    );
  }
}
