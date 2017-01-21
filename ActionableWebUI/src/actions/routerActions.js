import * as types from './actionTypes';
import { takeEvery, delay } from 'redux-saga';
import { call, put, take, select } from 'redux-saga/effects';
import { push, routeActions } from 'react-router-redux';
import {requestLoadActionItems} from './actionItemActions';

export function redirectActions () {
  return { type: types.ROUTE_ACTIONS };}

function* handleActionsRoute (action) {
  try {
    yield put(requestLoadActionItems ());
  } catch (error) {
  }
  yield put(push('/actions'));
}

export function* watchForRouterRequests () {
  yield takeEvery(types.ROUTE_ACTIONS, handleActionsRoute);
}
