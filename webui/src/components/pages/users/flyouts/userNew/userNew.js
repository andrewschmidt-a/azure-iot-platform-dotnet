// Copyright (c) Microsoft. All rights reserved.

import React from 'react';
import update from 'immutability-helper';

import { IoTHubManagerService } from 'services';
import {
  authenticationTypeOptions,
  permissions,
  toNewUserRequestModel,
  toSinglePropertyDiagnosticsModel,
  toUserDiagnosticsModel,
  toDiagnosticsModel
} from 'services/models';
import {
  copyToClipboard,
  int,
  isEmptyObject,
  LinkedComponent,
  stringToBoolean,
  svgs,
  Validator
} from 'utilities';
import {
  AjaxError,
  Btn,
  BtnToolbar,
  Flyout,
  FormControl,
  FormGroup,
  FormLabel,
  FormSection,
  Indicator,
  Protected,
  Radio,
  SectionDesc,
  SectionHeader,
  SummaryBody,
  SummaryCount,
  SummarySection,
  Svg
} from 'components/shared';

import './userNew.scss';
import Config from 'app.config';

const isEmailRegex = /^(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*|"(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21\x23-\x5b\x5d-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])*")@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\[(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?|[a-z0-9-]*[a-z0-9]:(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21-\x5a\x53-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])+)\])$/;
const emailAddress = x => !x.match(isEmailRegex);
const stringToInt = x => x === '' || x === '-' ? x : int(x);

const userOptions = {
  labelName: 'users.flyouts.new.user.label',
  user: {
    labelName: 'users.flyouts.new.user.user',
    value: false
  },
  edgeUser: {
    labelName: 'users.flyouts.new.user.edgeUser',
    value: true
  }
};

const userTypeOptions = {
  labelName: 'users.flyouts.new.userType.label',
  simulated: {
    labelName: 'users.flyouts.new.userType.simulated',
    value: true
  },
  physical: {
    labelName: 'users.flyouts.new.userType.physical',
    value: false
  }
};

const userIdTypeOptions = {
  labelName: 'users.flyouts.new.userId.label',
  manual: {
    hintName: 'users.flyouts.new.userId.hint',
    value: false
  },
  generate: {
    labelName: 'users.flyouts.new.userId.sysGenerated',
    value: true
  }
};

const authTypeOptions = {
  labelName: 'users.flyouts.new.authenticationType.label',
  symmetric: {
    labelName: 'users.flyouts.new.authenticationType.symmetric',
    value: authenticationTypeOptions.symmetric
  },
  x509: {
    labelName: 'users.flyouts.new.authenticationType.x509',
    value: authenticationTypeOptions.x509
  }
};

const authKeyTypeOptions = {
  labelName: 'users.flyouts.new.authenticationKey.label',
  generate: {
    labelName: 'users.flyouts.new.authenticationKey.generateKeys',
    value: true
  },
  manual: {
    labelName: 'users.flyouts.new.authenticationKey.manualKeys',
    value: false
  }
};

const UserDetail = ({ label, value }) => (
  <FormSection className="user-detail">
    <SectionHeader>{label}</SectionHeader>
    <div className="user-detail-contents">
      <div className="user-detail-value">{value}</div>
      <Svg className="copy-icon" path={svgs.copy} onClick={() => copyToClipboard(value)} />
    </div>
  </FormSection>
);

const UserConnectionString = ({ label, userId, hostName, sharedAccessKey }) => (
  <UserDetail label={label} value={`HostName=${hostName};UserId=${userId};SharedAccessKey=${sharedAccessKey}`} />
);

const ProvisionedUser = ({ user, t }) => {
  // When an error occurs, the user has no data... and so there is nothing to display here.
  if (isEmptyObject(user)) return null;

  const {
    id,
    iotHubHostName: hostName,
    authentication: { primaryKey },
    authentication: { secondaryKey }
  } = user;

  return (
    <div>
      <UserDetail label={t('users.flyouts.new.userId.label')} value={id} />
      <UserDetail label={t('users.flyouts.new.authenticationKey.primaryKey')} value={primaryKey} />
      <UserDetail label={t('users.flyouts.new.authenticationKey.secondaryKey')} value={secondaryKey} />
      <UserConnectionString label={t('users.flyouts.new.authenticationKey.primaryKeyConnection')} userId={id} hostName={hostName} sharedAccessKey={primaryKey} />
      <UserConnectionString label={t('users.flyouts.new.authenticationKey.secondaryKeyConnection')} userId={id} hostName={hostName} sharedAccessKey={secondaryKey} />
    </div>
  );
};

export class UserNew extends LinkedComponent {
  constructor(props) {
    super(props);

    this.state = {
      isPending: false,
      error: undefined,
      successCount: 0,
      changesApplied: false,
      formData: {
        email: ""
      },
      provisionedUser: {}
    };

    // Linked components
    this.formDataLink = this.linkTo('formData');



    this.emailLink = this.formDataLink.forkTo('email')
      //.reject(emailAddress)
      .check(Validator.notEmpty, () => this.props.t('users.flyouts.new.validation.required'))
  }

  componentWillUnmount() {
    if (this.provisionSubscription) this.provisionSubscription.unsubscribe();
  }

  shouldComponentUpdate(nextProps, nextState) {
    const { formData } = nextState;
    // For setting rules. Like disable if x is true...

    // Update normally
    return true;
  }

  formIsValid() {
    return [
      this.emailLink
    ].every(link => !link.error);
  }


  formControlChange = () => {
    // if (this.state.changesApplied) {
    //   this.setState({
    //     successCount: 0,
    //     changesApplied: false,
    //     provisionedUser: {}
    //   });
    // }
  }

  onFlyoutClose = (eventName) => {
    //this.props.logEvent(toUserDiagnosticsModel(eventName, this.state.formData));
    this.props.onClose();
  }

  apply = (event) => {
    event.preventDefault();
    const { formData } = this.state;

    if (this.formIsValid()) {
      this.setState({ isPending: true, error: null });

      if (this.provisionSubscription) this.provisionSubscription.unsubscribe();

      //this.props.logEvent(toUserDiagnosticsModel('Users_ApplyClick', formData));

    }
  }

  getSummaryMessage() {
    const { t } = this.props;
    const { isPending, changesApplied } = this.state;

    if (isPending) {
      return t('users.flyouts.new.pending');
    } else if (changesApplied) {
      return t('users.flyouts.new.applySuccess');
    } else {
      return t('users.flyouts.new.affected');
    }
  }

  render() {
    const {
      t
    } = this.props;
    const {
      formData,
      provisionedUser,
      isPending,
      error,
      successCount,
      changesApplied
    } = this.state;

    const completedSuccessfully = changesApplied && !error;
    const summaryMessage = this.getSummaryMessage();
    console.log(permissions.inviteUsers)
    return (
      <Flyout header={t('users.flyouts.new.title')} t={t} onClose={() => this.onFlyoutClose('Users_TopXCloseClick')}>
        <Protected permission={permissions.inviteUsers}>
          <form className="users-new-container" onSubmit={this.apply}>
            <div className="users-new-content">
              <FormGroup>
                <FormLabel>{t(userOptions.labelName)}</FormLabel>
                  <FormControl link={this.emailLink} type="text" onChange={this.formControlChange} />
                </FormGroup>
                          </div>

            {error && <AjaxError className="users-new-error" t={t} error={error} />}
            {
              !changesApplied &&
              <BtnToolbar>
                <Btn primary={true} disabled={isPending || !this.formIsValid()} type="submit">{t('users.flyouts.new.apply')}</Btn>
                <Btn svg={svgs.cancelX} onClick={() => this.onFlyoutClose('Users_CancelClick')}>{t('users.flyouts.new.cancel')}</Btn>
              </BtnToolbar>
            }
            {
              !!changesApplied &&
              <>
                <ProvisionedUser user={provisionedUser} t={t} />
                <BtnToolbar>
                  <Btn svg={svgs.cancelX} onClick={() => this.onFlyoutClose('Users_CloseClick')}>{t('users.flyouts.new.close')}</Btn>
                </BtnToolbar>
              </>
            }
          </form>
        </Protected>
      </Flyout>
    );
  }
}
