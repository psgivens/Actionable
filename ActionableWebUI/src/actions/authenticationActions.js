import * as types from './actionTypes';
import { takeEvery, delay } from 'redux-saga';
import { call, put, take, select } from 'redux-saga/effects';
import {push, routeActions} from 'react-router-redux';
import { redirectActions } from './routerActions';

//import AuthenticationApi from '../api/mockAuthenticationApi';

import AuthenticationApi from '../api/AuthenticationApi';

//import {beginAjaxCall, ajaxCallError} from './ajaxStatusActions';

export function requestAuthentication(info) {
  return Object.assign({type: types.AUTHENTICATE_REQUEST}, info);}
export function authenticationSuccess(token) {
  return { type: types.AUTHENTICATE_SUCCESS, token};}
export function authenticationDenied(error) {
  return { type: types.AUTHENTICATE_DENIED, error};}

function* handleAuthenticationRequest(action) {
  const credentials = action.credentials;
  if (credentials.password1 !== credentials.password2) {
    yield put(authenticationDenied("Passwords do not match"));
  } else {
    try {
      const response = yield call(AuthenticationApi.authenticateUser, {
        grant_type: 'password',
        username: action.credentials.username,
        password: action.credentials.password1});
      const auth = response;
      yield put(authenticationSuccess(auth.access_token));
    }
    catch (error) {
      yield put(authenticationDenied((
        error&&error.statusText!=="")
        ?error.statusText
        :"no error msg supplied"));
    }
    yield put(redirectActions());
  }
}

export function requestAuthLogout() {
  return Object.assign({type: types.LOGOUT_REQUEST });}
export function authLogoutSuccess() {
  return { type: types.LOGOUT_SUCCESS };}
export function authLogoutDenied(error) {
  return { type: types.LOGOUT_DENIED, error };}

function* handleAuthLogoutRequest(action) {
    try {
      // const response = yield call(AuthenticationApi.authenticateUser, {
      //   grant_type: 'password',
      //   username: action.credentials.username,
      //   password: action.credentials.password1});
      // const auth = response;
      yield put(authLogoutSuccess());
    }
    catch (error) {
      yield put(authLogoutDenied((
        error&&error.statusText!=="")
        ?error.statusText
        :"no error msg supplied"));
    }
    yield put(push('/'));
}

export function* watchForAuthenticationRequests () {
  yield takeEvery(types.AUTHENTICATE_REQUEST, handleAuthenticationRequest);
  yield takeEvery(types.LOGOUT_REQUEST, handleAuthLogoutRequest);
}
