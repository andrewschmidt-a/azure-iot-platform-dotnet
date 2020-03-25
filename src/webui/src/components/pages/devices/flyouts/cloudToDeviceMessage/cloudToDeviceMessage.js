// Copyright (c) Microsoft. All rights reserved.

import React, { Component } from 'react';
import { Observable } from 'rxjs';
import JSONInput from 'react-json-editor-ajrm';
import locale    from 'react-json-editor-ajrm/locale/en';

import { IoTHubManagerService } from 'services';
import { svgs } from 'utilities';
import { permissions } from 'services/models';
import {
  AjaxError,
  Btn,
  BtnToolbar,
  Flyout,
  Indicator,
  Protected,
  SectionDesc,
  SectionHeader,
  SummaryBody,
  SummaryCount,
  SummarySection,
  Svg,
} from 'components/shared';

import './cloudToDeviceMessage.scss';

export class CloudToDeviceMessage extends Component {
  constructor(props) {
    super(props);
    this.state = {
      physicalDevices: [],
      containsSimulatedDevices: false,
      confirmStatus: true,
      isPending: false,
      error: undefined,
      successCount: 0,
      changesApplied: false,
      json: {
        "message": "Text to send to devices"
      }
    };
  }

  componentDidMount() {
    if (this.props.devices) {
      this.populateDevicesState(this.props.devices);
    }
  }

  componentWillReceiveProps(nextProps) {
    if (nextProps.devices && (this.props.devices || []).length !== nextProps.devices.length) {
      this.populateDevicesState(nextProps.devices);
    }
  }

  componentWillUnmount() {
    if (this.subscription) this.subscription.unsubscribe();
  }

  populateDevicesState = (devices = []) => {
    const physicalDevices = devices.filter(({ isSimulated }) => !isSimulated);
    this.setState({ physicalDevices, containsSimulatedDevices: (physicalDevices.length !== devices.length) });
  }

  toggleConfirm = (value) => {
    if (this.state.changesApplied) {
      this.setState({ confirmStatus: value, changesApplied: false, successCount: 0 });
    } else {
      this.setState({ confirmStatus: value });
    }
  }

  sendCloudToDeviceMessage = (event) => {
    event.preventDefault();
    this.setState({ isPending: true, error: null });

    this.subscription = Observable.from(this.state.physicalDevices)
      .flatMap(({ id }) =>
        IoTHubManagerService.sendCloudToDeviceMessages(id, JSON.stringify(this.state.json))
          .map(() => id)
      )
      .subscribe(
        sentDeviceId => {
          this.setState({ successCount: this.state.successCount + 1 });
        },
        error => this.setState({ error, isPending: false, changesApplied: true }),
        () => this.setState({ isPending: false, changesApplied: true, confirmStatus: false })
      );
  }

  getSummaryMessage() {
    const { t } = this.props;
    const { isPending, changesApplied } = this.state;

    if (isPending) {
      return t('devices.flyouts.c2dMessage.pending');
    } else if (changesApplied) {
      return t('devices.flyouts.c2dMessage.applySuccess');
    } else {
      return t('devices.flyouts.c2dMessage.affected');
    }
  }

  messageJSONUpdated = (changeObject) => {
    this.setState({json : changeObject.jsObject})
  }

  render() {
    const { t, onClose } = this.props;
    const {
      physicalDevices,
      containsSimulatedDevices,
      confirmStatus,
      isPending,
      error,
      successCount,
      changesApplied
    } = this.state;

    const summaryCount = changesApplied ? successCount : physicalDevices.length;
    const completedSuccessfully = changesApplied && !error;
    const summaryMessage = this.getSummaryMessage();

    return (
      <Flyout header={t('devices.flyouts.c2dMessage.title')} t={t} onClose={onClose}>
          <Protected permission={permissions.deleteDevices}>
            <form className="device-c2dMessage-container" onSubmit={this.sendCloudToDeviceMessage}>
              <div className="device-c2dMessage-header">{t('devices.flyouts.c2dMessage.header')}</div>
              <div className="device-c2dMessage-descr">{t('devices.flyouts.c2dMessage.description')}</div>
               
              <JSONInput
                    id          = 'id'
                    placeholder = { this.state.json }
                    locale      = { locale }
                    height      = '550px'
                    width       = '100%'
                    onChange    = {this.messageJSONUpdated}
                />
              {
                containsSimulatedDevices &&
                <div className="simulated-device-selected">
                  <Svg path={svgs.infoBubble} className="info-icon" />
                  {t('devices.flyouts.c2dMessage.simulatedNotSupported')}
                </div>
              }

              <SummarySection>
                <SectionHeader>{t('devices.flyouts.c2dMessage.summaryHeader')}</SectionHeader>
                <SummaryBody>
                  <SummaryCount>{summaryCount}</SummaryCount>
                  <SectionDesc>{summaryMessage}</SectionDesc>
                  {this.state.isPending && <Indicator />}
                  {completedSuccessfully && <Svg className="summary-icon" path={svgs.apply} />}
                </SummaryBody>
              </SummarySection>

              {error && <AjaxError className="device-c2dMessage-error" t={t} error={error} />}
              {
                !changesApplied &&
                <BtnToolbar>
                  <Btn svg={svgs.trash} primary={true} disabled={isPending || physicalDevices.length === 0} type="submit">{t('devices.flyouts.c2dMessage.apply')}</Btn>
                  <Btn svg={svgs.cancelX} onClick={onClose}>{t('devices.flyouts.c2dMessage.cancel')}</Btn>
                </BtnToolbar>
              }
              {
                !!changesApplied &&
                <BtnToolbar>
                  <Btn svg={svgs.cancelX} onClick={onClose}>{t('devices.flyouts.c2dMessage.close')}</Btn>
                </BtnToolbar>
              }
            </form>
          </Protected>
      </Flyout>
    );
  }
}
