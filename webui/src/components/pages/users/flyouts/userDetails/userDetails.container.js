// Copyright (c) Microsoft. All rights reserved.

import { connect } from 'react-redux';
import { withNamespaces } from 'react-i18next';
import { UserDetails } from './userDetails';
import {
  getUserById,
  epics as usersEpics,
  redux as usersRedux
} from 'store/reducers/usersReducer';

// Pass the user details
const mapStateToProps = (state, props) => ({
  user: getUserById(state, props.userId)
});

// Wrap the dispatch method
const mapDispatchToProps = dispatch => ({
  resetPendingAndError: () => dispatch(usersRedux.actions.resetPendingAndError(usersEpics.actions.fetchEdgeAgent))
});

export const UserDetailsContainer = withNamespaces()(connect(mapStateToProps, mapDispatchToProps)(UserDetails));
